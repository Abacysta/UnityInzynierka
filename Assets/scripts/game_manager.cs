using Assets.classes;
using Assets.classes.subclasses;
using Assets.map.scripts;
using Assets.Scripts;
using Assets.ui.scripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Assets.classes.Relation;

public class game_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text turnCntTxt;
    [SerializeField] private AudioSource turn_sound;
    //[SerializeField] private float RecruitablePopulationFactor = 0.2f;
    //[SerializeField] private float PopulationFactor = 0.1f;
    //[SerializeField] private int HappinessFactor = 5;
    //[SerializeField] private float ArmyFactor = 0.1f;
    [SerializeField] private fog_of_war fog_Of_War;
    [SerializeField] private GameObject loading_box;
    [SerializeField] private Slider loading_bar;
    [SerializeField] private TMP_Text loading_txt;
    [SerializeField] private filter_modes loader;
    [SerializeField] private camera_controller camera_controller;
    [SerializeField] private army_visibility_manager armyVisibilityManager;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private alerts_manager alerts;
    [SerializeField] private diplomatic_actions_manager diplomaticActionsManager;
    [SerializeField] private battle_manager battle_manager;
    [SerializeField] private random_events_manager random_events;
    [SerializeField] private start_screen start_screen;
    [SerializeField] private diplomatic_relations_manager diplomacy;
    [SerializeField] private info_bar info_bar;
    [SerializeField] private Button save_button;
    [SerializeField] private army_click_handler army_click_handler;
    [SerializeField] private map_ui map_ui;
    
    [SerializeField] private AI_manager ai_manager;

    public int turnCnt { get { return map.turnCnt; } }

    private Save toSave;

    public static readonly int WarHappinessPenaltyConst = 2;
    public static readonly int AllianceHappinessBonusConst = 1;
    public static readonly int VassalageHappinessBonusConstC1 = 1;
    public static readonly int VassalageHappinessPenaltyConstC2 = 1;

    private void Start()
    {
        while (start_screen == null) ;
        start_screen.welcomeScreen();
	}

    public void UndoAll()
    {
        map_ui.DeactivateInterfaces();
        while(map.CurrentPlayer.Actions.Count > 0) {
            map.CurrentPlayer.Actions.revert();
        }
    }
    public void invokeEvent(int id) {
        map.Countries[id].Events[1].call();
    }

    public void undoLast() {
        map_ui.DeactivateInterfaces();
        map.CurrentPlayer.Actions.revert();
    }

    public void undoLast(int id) {
        map.Countries[id].Actions.revert();
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
                    .Where(p => p.Owner_id == army.OwnerId ||
                                map.Relations.Any(rel =>
                                    (rel.type == RelationType.Alliance || rel.type == RelationType.Vassalage) &&
                                    rel.Sides.Contains(armyOwner) && rel.Sides.Contains(map.Countries[p.Owner_id])) ||
                                map.Relations.Any(rel =>
                                    rel.type == RelationType.MilitaryAccess &&
                                    rel.Sides[0] == map.Countries[p.Owner_id] && rel.Sides[1] == armyOwner))
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

    private void executeActions() {
        foreach(var c in map.Countries) {
            List<Assets.classes.TurnAction> instants = c.Actions.extractInstants();
            foreach (var inst in instants) {
                inst.execute(map);
            }
        }

        int acmax = map.Countries.Max(a => a.Actions.Count);
        for(int i = 0; i < acmax; i++) {
            foreach (var c in map.Countries.Where(c => c.Id != 0).OrderBy(c => c.Priority)) {
                bool bswitch = false;
                Army att = null;
                if(c.Actions.Count > 0 && c.Actions.last is TurnAction.army_move) {
                    bswitch = true;
                    att = (c.Actions.last as TurnAction.army_move).Army;
                }
                c.Actions.execute();
                if(bswitch && att != null) {
                    battle_manager.checkBattle(att);
                }
            }
        }

        TeleportUnauthorizedArmies();
    }

    private void turnCalculations() {
        int pcnt = map.Provinces.Count, ccnt = map.Countries.Count;
        loading_txt.text = "txttt";
        loading_bar.value = 0;
        loading_box.SetActive(true);
        provinceCalc(pcnt);
        countryCalc();
        loading_txt.text = "Calculating happiness from relations.";
        happinnessFromRelations();
        map.calcPopExtremes();


        //yield return new WaitForSeconds(2f);
        //map.moveArmies();
        //yield return new WaitForSeconds(2f);


        loading_txt.text = "Merging armies";
        foreach(var c in map.Countries) {
            loading_bar.value += 0.1f  * 100 / ccnt;
            map.mergeArmies(c);
            c.AtWar = map.getRelationsOfType(c, Relation.RelationType.War) != null;
        }
        foreach(var a in map.Armies.Where(a=>a.OwnerId != 0)) {
            map.Countries[a.OwnerId].modifyResource(Resource.Gold, a.Count * map.Countries[a.OwnerId].techStats.armyUpkeep);
        }
        fog_Of_War.StartTurn();
        turnCntTxt.SetText((++map.turnCnt).ToString());
        loading_box.SetActive(false);
        //testRelations();
        Debug.Log("stopped bar");

    }

    private void provinceCalc(int pcnt) {
        loading_txt.text = "Calculating provinces";
        Debug.Log("started bar");

        foreach(var p in map.Provinces.Where(p => p.Type == "land")) {
            loading_bar.value = (0.2f * 100 / pcnt);
            map.growPop(p.coordinates);
            if (p.Owner_id != 0) map.growHap(p.coordinates, 3);
            map.calcRecruitablePop(p.coordinates);
            p.calcStatuses();

            // zarzadzanie okupacja
            if(p.OccupationInfo.OccupyingCountryId != -1)
            {
                map.ManageOccupationDuration(p);
            }
        }
    }

    private void happinnessFromRelations()
    {
        foreach(var country in map.Countries)
        {
            var wars = map.getRelationsOfType(country, Assets.classes.Relation.RelationType.War);
            foreach(var war in wars)
            {
                foreach( var province in country.Provinces) {
                    province.Happiness -= WarHappinessPenaltyConst;
                }
            }
            var alliances = map.getRelationsOfType(country, Assets.classes.Relation.RelationType.Alliance);
            foreach(var a in alliances)
            {
                foreach(var p in country.Provinces)
                {
                    p.Happiness += AllianceHappinessBonusConst;
                }
            }
            var vassalages = map.getRelationsOfType(country, Assets.classes.Relation.RelationType.Vassalage);
            foreach(var v in vassalages)
            {
                foreach(var p in country.Provinces)
                {
                    Country master = map.getMaster(country);
                    if (master == null)
                    {
                        p.Happiness += VassalageHappinessBonusConstC1;
                    }
                    else
                    {
                        p.Happiness -= VassalageHappinessPenaltyConstC2;
                    }
                }
            }
        }
    }

    private void ccc(int i) {
        Dictionary<Resource, float> resources = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };

        loading_txt.text = "Gathering resources for country." + map.Countries[i].Id;
        loading_bar.value += 0.7f * 100 / map.Countries.Count;
        map.Countries[i].Tax.applyCountryTax(map.Countries[i]);

        foreach (var p in map.Countries[i].Provinces) {
            if (p.Buildings.ContainsKey(BuildingType.School)
                && p.GetBuildingLevel(BuildingType.School) < 4) {
                resources[Resource.SciencePoint] += p.GetBuildingLevel(BuildingType.School)* 3;
            }

            if (p.OccupationInfo.IsOccupied)
            {
                var occupierTechStats = map.Countries[p.OccupationInfo.OccupyingCountryId].techStats;
                resources[p.ResourcesT] += p.ResourcesP * (1 - occupierTechStats.occPenalty);
            }
            else
            {
                resources[p.ResourcesT] += p.ResourcesP;
            }

            resources[Resource.AP] += 0.1f;
        }

        resources[Resource.Gold] *= map.Countries[i].techStats.prodFactor;
        resources[Resource.Wood] *= map.Countries[i].techStats.prodFactor;
        resources[Resource.Iron] *= map.Countries[i].techStats.prodFactor;
        resources[Resource.Gold] -= Map.PowerUtilites.getArmyUpkeep(map, map.Countries[i]);
        resources[Resource.AP] += 2.5f;//one simple trick

        foreach (var res in resources) {
            map.Countries[i].modifyResource(res.Key, res.Value);
        }
        map.Countries[i].setResource(Resource.AP, resources[Resource.AP]);
    }

    private void countryCalc() {

        for(int i = 1; i < map.Countries.Count; i++) {
            ccc(i);
        }
    }

    private void alertClear() {
        foreach(var event_ in map.CurrentPlayer.Events) {
            event_.reject();
        }
        map.CurrentPlayer.Events.Clear();
    }

    private void rebellionCheck() {
        foreach(var p in map.Provinces) {
            random_events.checkRebellion(p);
            //^ = bool wiec mozna potem dodac event
        }
    }

    private void aiTurn() {
        try {
            ai_manager.behave();
        }
        catch(Exception e) { 
            Debug.LogError(e);
        }
        
    }

    //private void welcomeScreen() {
    //    if (turnCnt == 0) {
    //        start_screen.SetActive(true);
    //        start_screen.transform.Find("window").GetComponentInChildren<TMP_Text>().text = "You're playing as " + "takie jajca bo mapa sie jeszcze nie zaladowala xd";//map.CurrentPlayer.Name;
    //        var button = start_screen.transform.Find("window").GetComponentInChildren<Button>();
    //        button.onClick.RemoveAllListeners();
    //        button.onClick.AddListener(() => alerts.loadEvents(map.CurrentPlayer));
    //        button.onClick.AddListener(() => alerts.reloadAlerts());
    //        button.onClick.AddListener(() => start_screen.SetActive(false));
    //    }
    //    else {
    //        start_screen.SetActive(false);
    //    }
    //}


    public void saveGame(string name) {
        var path = Application.persistentDataPath + "/" + name + ".save";
        BinaryFormatter form = new BinaryFormatter();
        using(FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write)) {
            form.Serialize(stream, new Save(map));
        }
		Debug.Log($"Game saved to: {path}");
	}

	public void saveGameJson() {
		var path = Application.persistentDataPath + "/save.json";
		string jsonData = JsonConvert.SerializeObject(toSave, Formatting.Indented); // 'Indented' for pretty-printing

		using (StreamWriter writer = new StreamWriter(path, false)) {
			writer.Write(jsonData);
		}

		Debug.Log($"Game saved to: {path}");
	}

    public void loadGame(string name) {
        var path = Application.persistentDataPath + "/" + name + ".save";
        Debug.Log("loading " + path);
        if (File.Exists(path)) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using(FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                Save data = (Save)binaryFormatter.Deserialize(stream);
                Save.loadDataFromSave(data, map, loader, (dialog_box, camera_controller, diplomacy));
				fog_Of_War.UpdateFogOfWar();
				alerts.loadEvents(map.CurrentPlayer);
				alerts.reloadAlerts();
				turnCntTxt.SetText((map.turnCnt).ToString());
			}
        }
        else {
            Debug.LogError("nie pyklo ladowanie");
        }
		loader.Reload();
		foreach (var a in map.Armies) {
			map.reloadArmyView(a);
		}
	}

	public void loadGameJson(string name) {
		var path = Application.persistentDataPath + "/" + name + ".json"; // Save file with .json extension
		Debug.Log("loading " + path);

		if (File.Exists(path)) {  // Check if the file exists
			using (StreamReader reader = new StreamReader(path)) {
				string jsonData = reader.ReadToEnd();  // Read all text from the file

				// Deserialize the JSON string into a Save object
				Save data = JsonConvert.DeserializeObject<Save>(jsonData);

				// Convert the Save object into the Map object
				Save.loadDataFromSave(data, map, loader, (dialog_box, camera_controller, diplomacy));
                fog_Of_War.UpdateFogOfWar();
                alerts.loadEvents(map.CurrentPlayer);
                alerts.reloadAlerts();
			}
		}
		else {
			Debug.LogError("Save file not found or loading failed.");
		}
        
	}

    public string[] getSaveGames() {
        var saves = new List<string>();
        if (Directory.Exists(Application.persistentDataPath)) {
            return Directory.GetFiles(Application.persistentDataPath, "*.save").Select(s=>s.Substring(Application.persistentDataPath.Length + 1).Replace(".save", "")).ToArray();
        }
        else {
            Debug.Log("Data directory is broken");
            return null;
        }
    }

    public bool existsSaveGame(string name) {
        var path = Path.Combine(Application.persistentDataPath, name + ".save");
        return File.Exists(path);
    }

    public bool deleteSaveGame(string name) {
        var path = Path.Combine(Application.persistentDataPath, name + ".save");
        try {
            if (File.Exists(path)) {
                File.Delete(path);
                return true;
            }
            else return false;
        }
        catch(Exception ex) {
            Debug.LogError(ex.Message);
            return false;
        }
    }

	public void LocalTurnSimulation() {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            TurnSimulation();
            return;
        }
        Action a = () => TurnSimulation();
        dialog_box.invokeConfirmBox("Pass the turn", "Do you want to pass the turn?", a, null, null);
    }

    public void TurnSimulation()
    {
        PreTurnInstructions();

        if (map.currentPlayer < map.Countries.Count - 1)
        {
            NextPlayerInstructions();
        }
        else
        {
            EndOfTurnInstructions();
        }

        PostTurnInstructions();
    }

    private void PreTurnInstructions()
    {
        ResetArmies();
    }

    private void NextPlayerInstructions()
    {
        alertClear();
        map.currentPlayer++;
        diplomaticActionsManager.ResetReceiverButtonStates();

        Debug.Log($"Sending actions.");
    }

    private void EndOfTurnInstructions()
    {
        Debug.Log($"Executing actions and performing calculations.");

        turn_sound.Play();

        rebellionCheck();
        executeActions();
        turnCalculations();
        GenerateEventsForCountries();

        for (int i = 0; i < map.Countries.Count; i++)
        {
            Debug.Log(map.Countries[i].Name + "--");
            Debug.Log("--" + map.Controllers[i]);
        }

        map.currentPlayer = 1;

        AutoSave();
        loader.Reload();
    }

    private void PostTurnInstructions()
    {
        if (map.Controllers[map.currentPlayer] == Map.CountryController.Ai)
        {
            aiTurn();
            TurnSimulation();
        }
        else
        {
            Debug.Log($"Now, it's country {map.CurrentPlayer.Id} - {map.CurrentPlayer.Name}'s turn");
            HandleWelcomeScreen();
            camera_controller.ZoomCameraOnCountry(map.currentPlayer);
            fog_Of_War.UpdateFogOfWar();
            armyVisibilityManager.UpdateArmyVisibility(map.CurrentPlayer.RevealedTiles);
            map.UpdateAllArmyViewOrders();
            alerts.loadEvents(map.CurrentPlayer);
        }
    }

    private void GenerateEventsForCountries()
    {
        foreach (var country in map.Countries) 
        {
            if (country.Id != 0) random_events.getRandomEvent(country);         
        }
    }

    public void ResetArmies()
    {
        foreach (Army army in map.Armies)
        {
            map.destroyArmyView(army);
        }

        for (int i = 0; i < map.Armies.Count; i++)
        {
            Army army = map.Armies[i];
            if (army.Destination != army.Position)
            {
                map.undoSetMoveArmy(army);
            }
        }

        foreach (Army army in map.Armies)
        {
            map.createArmyView(army);
        }
    }

    private void AutoSave()
    {
        if (map.turnCnt % 5 == 0)
        {
            toSave = new(map);
            saveGame("autosave");
            toSave = null;
        }
    }

    private void HandleWelcomeScreen()
    {
        if (map.turnCnt == 0 && map.Controllers[map.currentPlayer] == Map.CountryController.Local)
        {
            start_screen.welcomeScreen();
        }
        else if (map.turnCnt == 1)
        {
            start_screen.unHide();
        }
    }
    //to jest wozny
    private class Janitor {
        private Map map;
        public Janitor(Map map) {
            this.map = map;
        }

        public void cleanup() { }
    }

    private void testRelations()
    {
        Country c1 = map.Countries[9];
        Country c2 = map.Countries[10];
		map.Relations.Clear();
        testBuildings();
        if (turnCnt == 1)
        {
            testHapp();
            testPopulation();
            testTech();
            testStatus();
            testEvent();
            map.Relations.Add(new Relation.War(c1, c2));
        }
		if (turnCnt == 2)
		{
			map.Relations.Add(new Relation.Alliance(c1,c2));
		}
		if (turnCnt == 3)
		{
			map.Relations.Add(new Relation.Truce(c1,c2,1));
		}
		if (turnCnt == 4)
		{
			map.Relations.Add(new Relation.Vassalage(c1,c2));
		}
        if(turnCnt == 5)
        {
            map.Relations.Add(new Relation.MilitaryAccess(c1,c2));
        }
	}
    private void testBuildings()
    {
		Province p = map.getProvince(6, 6);
        Country c = map.Countries[9];
        c.Actions.addAction(new TurnAction.building_upgrade(p, BuildingType.Fort));
		c.Actions.addAction(new TurnAction.building_upgrade(p, BuildingType.Mine));
		c.Actions.addAction(new TurnAction.building_upgrade(p, BuildingType.Infrastructure));
		c.Actions.addAction(new TurnAction.building_upgrade(p, BuildingType.School));

	}
    private void testHapp()
    {
        int x = 4;
        int happ = 0;
        while (true)
        {
			map.getProvince(x++, 6).Happiness = happ;
            happ += 20;
            if (x == 10) return;
		}
	}
	private void testPopulation()
	{
		int x = 4;
		int Pop = 0;
		while (true)
		{
			map.getProvince(x++, 6).Population += Pop;
			Pop += 500;
			if (x == 10) return;
		}
	}
    private void testTech()
    {
        Country c = map.Countries[9];
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Military));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Military));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Military));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Military));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Military));

		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Economic));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Economic));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Economic));

		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
		c.Actions.addAction(new TurnAction.technology_upgrade(c, Technology.Administrative));
	}
	private void testStatus()
	{
		Province p = map.getProvince(7, 7);
        p.addStatus(new Illness(1));
        p.addStatus(new Festivities(1));
	}
    private void testEvent()
    {
        Province p = map.getProvince(7, 7);
        Country c = map.Countries[9];

        c.Events.Add(new Event_.GlobalEvent.FloodEvent(c,dialog_box,camera_controller));
        c.Events.Add(new Event_.LocalEvent.GoldRush(p,dialog_box,camera_controller));
    }
}