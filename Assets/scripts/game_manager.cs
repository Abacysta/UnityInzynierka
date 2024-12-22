using Assets.classes;
using Assets.classes.subclasses;
using Assets.map.scripts;
using Assets.Scripts;
using Assets.ui.scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.classes.Relation;
using static Assets.classes.subclasses.Constants.ProvinceConstants;

public class game_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text turnCntTxt;
    [SerializeField] private AudioSource turn_sound;
    [SerializeField] private fog_of_war fog_Of_War;
    [SerializeField] private GameObject loading_box;
    [SerializeField] private filter_modes loader;
    [SerializeField] private camera_controller camera_controller;
    [SerializeField] private army_visibility_manager armyVisibilityManager;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private alerts_manager alerts;
    [SerializeField] private diplomatic_actions_manager diplomaticActionsManager;
    [SerializeField] private battle_manager battle_manager;
    [SerializeField] private random_events_manager random_events;
    [SerializeField] private start_screen start_screen;
    [SerializeField] private GameObject end_screen;
    [SerializeField] private diplomatic_relations_manager diplomacy;
    [SerializeField] private army_click_handler army_click_handler;
    [SerializeField] private map_ui map_ui;
    [SerializeField] private save_manager save_manager;
    [SerializeField] private AI_manager ai_manager;

    public int turnCnt { get { return map.TurnCnt; } }

    void Awake()
    {
        if (PlayerPrefs.HasKey("saveName"))
        {
            string saveName = PlayerPrefs.GetString("saveName");
            save_manager.LoadGame(saveName);
            PlayerPrefs.DeleteKey("saveName");
        }
    }

    void Start()
    {
        while (start_screen == null) ;
        start_screen.WelcomeScreen();
    }

    internal async void LoadGameFromSave(Save data)
    {
        // This is called in Awake() before all scripts, the map is being initialized
        Save.LoadDataFromSave(data, map, loader, (dialog_box, camera_controller, diplomacy));

        // You need to wait for the Start() methods in other scripts to complete
        // to ensure required data is initialized before proceeding with the next loading steps.
        await Task.Delay(100);

        fog_Of_War.UpdateFogOfWar();
        alerts.LoadEvents(map.CurrentPlayer);
        alerts.ReloadAlerts();
        turnCntTxt.SetText((map.TurnCnt).ToString());
        loader.Reload();
        camera_controller.ZoomCameraOnCountry(map.CurrentPlayerId);

        foreach (var a in map.Armies)
        {
            map.ReloadArmyView(a);
        }

        armyVisibilityManager.UpdateArmyVisibility(map.CurrentPlayer.RevealedTiles);
        map.UpdateAllArmyViewOrders();
    }

    public void UndoAllActions()
    {
        map_ui.DeactivateInterfaces();
        while(map.CurrentPlayer.Actions.Count > 0) {
            map.CurrentPlayer.Actions.RevertLastAction();
        }
    }
    public void InvokeEvent(int id) {
        map.Countries[id].Events[1].Call();
    }

    public void UndoLastAction() {
        map_ui.DeactivateInterfaces();
        map.CurrentPlayer.Actions.RevertLastAction();
    }

    public void UndoLastAction(int id) {
        map.Countries[id].Actions.RevertLastAction();
    }

    private void TeleportUnauthorizedArmies()
    {
        foreach (var army in map.Armies)
        {
            Vector3Int tilePosition = new(army.Position.Item1, army.Position.Item2);
            if (!army_click_handler.IsTileAccessibleForArmyMovement(tilePosition, army.OwnerId))
            {
                Country armyOwner = map.Countries[army.OwnerId];

                // Move the army to the nearest province that meets at least one of the following requirements:
                // - it is the territory of their own country, or
                // - it is the territory of their ally, or
                // - it is the territory of a country with which they have a vassalage relation, or
                // - it is the territory of a country that grants them military access
                Vector3Int nearestOwnerProvincePosition = map.Provinces
                    .Where(p => p.OwnerId == army.OwnerId ||
                                map.Relations.Any(rel =>
                                    (rel.Type == RelationType.Alliance || rel.Type == RelationType.Vassalage) &&
                                    rel.Sides.Contains(armyOwner) && rel.Sides.Contains(map.Countries[p.OwnerId])) ||
                                map.Relations.Any(rel =>
                                    rel.Type == RelationType.MilitaryAccess &&
                                    rel.Sides[0] == map.Countries[p.OwnerId] && rel.Sides[1] == armyOwner))
                    .Select(p => new Vector3Int(p.X, p.Y, 0))
                    .OrderBy(point => Vector3Int.Distance(tilePosition, point))
                    .FirstOrDefault();

                if (nearestOwnerProvincePosition != default)
                {
                    army.Position = army.Destination = (nearestOwnerProvincePosition.x, nearestOwnerProvincePosition.y);
                    map.MoveArmy(army);
                }
            }
        }
    }

    private void ExecuteActions() {
        foreach (var c in map.Countries) {
            List<TurnAction> instants = c.Actions.ExtractInstants();
            foreach (var inst in instants) {
                inst.Execute(map);
            }
        }

        int actionMax = map.Countries.Max(a => a.Actions.Count);

        for (int i = 0; i < actionMax; i++) {
            foreach (var c in map.Countries.Where(c => c.Id != 0).OrderBy(c => c.Priority)) {
                bool isArmyMoveAction = false;
                Army attackerArmy = null;

                if (c.Actions.Count > 0) {
                    if (c.Actions.Last is TurnAction.ArmyMove) {
                        var armyMoveAction = c.Actions.Last as TurnAction.ArmyMove;
                        armyMoveAction.Execute(map);
                        c.Actions.Actions.RemoveAt(0);
                        isArmyMoveAction = true;
                        attackerArmy = armyMoveAction.MovedArmy;
                    }
                    else {
                        c.Actions.ExecuteLastAction();
                    }
                }

                if (isArmyMoveAction && attackerArmy != null) {
                    battle_manager.CheckBattle(attackerArmy);
                }
            }
        }

        TeleportUnauthorizedArmies();
    }

    private void PerformTurnCalculations() {
        int pcnt = map.Provinces.Count, ccnt = map.Countries.Count;
        loading_box.SetActive(true);
        PerformProvinceCalculations(pcnt);
        PerformCountryCalculations();
        diplomacy.PerformTurnRelationCalculations();
        map.CalcPopulationExtremes();

        foreach(var c in map.Countries) {
            map.MergeArmies(c);
            c.AtWar = map.GetRelationsOfType(c, Relation.RelationType.War) != null;
        }
        fog_Of_War.StartTurn();
        turnCntTxt.SetText((++map.TurnCnt).ToString());
        loading_box.SetActive(false);
        Debug.Log("stopped bar");
    }

    private void PerformProvinceCalculations(int pcnt) {
        Debug.Log("started bar");

        foreach(var p in map.Provinces.Where(p => p.IsLand)) {
            if (p.OwnerId != 0) {
                p.GrowPopulation(map);
                p.GrowHappiness(map, 3);
                p.CalcRecruitablePopulation(map);
            }
            p.CalcStatuses();
        }
    }

    private void CalculateCountryResources(int i) {
        Dictionary<Resource, float> resources = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };
        
        map.Countries[i].Tax.ApplyCountryTax(map.Countries[i]);

        foreach (var p in map.Countries[i].Provinces) {
            if (p.Buildings.ContainsKey(BuildingType.School)
                && p.GetBuildingLevel(BuildingType.School) < 4) {
                resources[Resource.SciencePoint] += p.GetBuildingLevel(BuildingType.School)* 3;
            }

            if (p.OccupationInfo.IsOccupied)
            {
                var occupierTechStats = map.Countries[p.OccupationInfo.OccupyingCountryId].TechStats;
                resources[p.ResourceType] += p.ResourcesP * (1 - occupierTechStats.OccPenalty);
            }
            else
            {
                resources[p.ResourceType] += p.ResourcesP;
            }

            resources[Resource.AP] += 0.1f;
        }

        resources[Resource.Gold] *= map.Countries[i].TechStats.ProdFactor;
        resources[Resource.Wood] *= map.Countries[i].TechStats.ProdFactor;
        resources[Resource.Iron] *= map.Countries[i].TechStats.ProdFactor;
        resources[Resource.Gold] -= Map.PowerUtilites.GetArmyUpkeep(map, map.Countries[i]);
        resources[Resource.AP] += 2.5f;

        foreach (var res in resources) {
            map.Countries[i].ModifyResource(res.Key, res.Value);
        }
        map.Countries[i].SetResource(Resource.AP, resources[Resource.AP]);
    }

    private void PerformCountryCalculations() {

        for (int i = 1; i < map.Countries.Count; i++) {
            CalculateCountryResources(i);
        }
    }

    private void AlertClear() {
        foreach(var event_ in map.CurrentPlayer.Events) {
            event_.Reject();
        }
        map.CurrentPlayer.Events.Clear();
    }

    private void CheckRebellion() {
        foreach(var p in map.Provinces.Where(p => p.IsLand)) {
            random_events.CheckRebellion(p);
            //returns bool, so in future can do more with that
        }
    }

    private void PerformAITurn() {
        try {
            ai_manager.Behave();
        }
        catch(Exception e) { 
            Debug.LogError(e);
        }
    }

	public void LocalTurnSimulation() {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            PerformTurnSimulation();
            return;
        }
        Action a = () => PerformTurnSimulation();
        dialog_box.InvokeConfirmBox("Pass the turn", "Do you want to pass the turn?", a);
    }

    public void PerformTurnSimulation()
    {
        ExecutePreTurnInstructions();

        if (map.CurrentPlayerId < map.Countries.Count - 1)
        {
            ExecuteNextPlayerInstructions();
        }
        else
        {
            ExecuteEndOfTurnInstructions();
        }

        ExecutePostTurnInstructions();
    }

    private void ExecutePreTurnInstructions()
    {
        ResetArmies();
    }

    private void ExecuteNextPlayerInstructions()
    {
        AlertClear();
        map.CurrentPlayerId++;
        diplomaticActionsManager.ResetReceiverButtonStates();

        Debug.Log($"Sending actions.");
    }

    private void ExecuteEndOfTurnInstructions()
    {
        Debug.Log($"Executing actions and performing calculations.");

        turn_sound.Play();

        CheckRebellion();
        ExecuteActions();
        PerformTurnCalculations();
        GenerateEventsForCountries();

        for (int i = 0; i < map.Countries.Count; i++)
        {
            Debug.Log(map.Countries[i].Name + "--");
            Debug.Log("--" + map.Controllers[i]);
        }
        var cleanup_crew = new Janitor(map, battle_manager, diplomacy);
        if(cleanup_crew.Cleanup()) EndGame(map.Turnlimit <= map.TurnCnt);
        map.CurrentPlayerId = 1;
        if (map.TurnCnt % 5 == 0) AutoSave();
        loader.Reload();
    }

    private void ExecutePostTurnInstructions()
    {
        if (map.Controllers[map.CurrentPlayerId] == Map.CountryController.Ai)
        {
            PerformAITurn();
            PerformTurnSimulation();
        }
        else
        {
            Debug.Log($"Now, it's country {map.CurrentPlayer.Id} - {map.CurrentPlayer.Name}'s turn");
            HandleWelcomeScreen();
            camera_controller.ZoomCameraOnCountry(map.CurrentPlayerId);
            fog_Of_War.UpdateFogOfWar();
            armyVisibilityManager.UpdateArmyVisibility(map.CurrentPlayer.RevealedTiles);
            map.UpdateAllArmyViewOrders();
            alerts.LoadEvents(map.CurrentPlayer);
        }
    }

    private void GenerateEventsForCountries()
    {
        foreach (var country in map.Countries) 
        {
            if (country.Id != 0) random_events.GetRandomEvent(country);         
        }
    }

    public void ResetArmies()
    {
        foreach (Army army in map.Armies)
        {
            map.DestroyArmyView(army);
        }

        for (int i = 0; i < map.Armies.Count; i++)
        {
            Army army = map.Armies[i];
            if (army.Destination != army.Position)
            {
                map.UndoSetMoveArmy(army);
            }
        }

        foreach (Army army in map.Armies)
        {
            map.CreateArmyView(army);
        }
    }

    private void AutoSave()
    {
        save_manager.ToSave = new(map);
        save_manager.SaveGame("autosave");
        save_manager.ToSave = null;
    }

    private void HandleWelcomeScreen()
    {
        if (map.TurnCnt == 0 && map.Controllers[map.CurrentPlayerId] == Map.CountryController.Local)
        {
            start_screen.WelcomeScreen();
        }
        else if (map.TurnCnt == 1)
        {
            start_screen.UnHide();
        }
    }

    private void EndGame(bool timeout) {
        end_screen.SetActive(true);
    }

    private class Janitor {
        private Map map;
        private battle_manager battle;
        private diplomatic_relations_manager diplomacy;

        public Janitor(Map map, battle_manager battle, diplomatic_relations_manager diplomacy) {
            this.map = map;
            this.battle = battle;
            this.diplomacy = diplomacy;
        }
        public bool Cleanup() { 
            //check if the game should be going at all
            if(map.TurnCnt == map.Turnlimit) {
                return true;
            }
            
            //country
            for(int i = 1; i < map.Countries.Count; i++) {
                var c = map.Countries[i];
                //check if the game has any point in going forward
                var vas = Map.PowerUtilites.GetVassals(map, c);
                if (map.Countries.Where(c => c.Id != 0 || c.Id != i).ToHashSet().Equals(vas)) return true;
                //capital check so if no capitals kill it
                if (!c.Provinces.Contains(map.GetProvince(c.Capital))) {
                    map.Diplomacy = diplomacy;
                    map.KillCountry(c);
                    continue;
                }
                //opinions check
                foreach(var o in c.Opinions.Keys.ToList()) {
                    c.Opinions[o] = Math.Clamp(c.Opinions[o], MIN_OPINION, MAX_OPINION);
                }
                //unfought armies check
                var ownedArmies = map.Armies.Where(a => a.OwnerId == c.Id).ToHashSet();
                foreach (var a in ownedArmies) {
                    battle.CheckBattle(a);
                    //its a hack but what can you do
                    var armiesInProv = map.Armies.Where(ass => ass.Position == a.Position).ToHashSet();
                    foreach (var aa in armiesInProv) {
                        map.UpdateArmyPosition(aa, aa.Position);
                    }
                }
            }
            //provinces
            foreach(var p in map.Provinces) {
                //status check
                if (p.Statuses != null) if (p.Statuses.Count != 0)
                    p.Statuses.RemoveAll(s => s.Duration == 0);
                //happ check
                if(p.Happiness>MAX_HAPP || p.Happiness< MIN_HAPP) p.Happiness = Math.Clamp(p.Happiness, MIN_HAPP, MAX_HAPP);
                //resource check
                if(p.ResourceAmount<MIN_RESOURCE_PROVINCE) p.ResourceAmount = MIN_RESOURCE_PROVINCE;
                //population
                if (p.Population <= 0) p.Population = 1;
                if (p.Population > MAX_POP) p.Population = MAX_POP;
                //buildings
                if(p.Buildings!= null){
                    foreach(var b in p.Buildings.Keys) {
                        if (p.Buildings[b] > 4 || p.Buildings[b] < 0) p.Buildings[b] = 0;
                    }
                    //unlock school at 3k populace
                    if (p.Population > SCHOOL_MIN_POP && p.Buildings[BuildingType.School] == 4) p.Buildings[BuildingType.School] = 0;
                }
                // zarzadzanie okupacja
                if (p.OccupationInfo != null && p.OccupationInfo.OccupyingCountryId != -1)
                {
                    map.ManageOccupationDuration(p);
                }
            }
            return false;
        }
    }
}