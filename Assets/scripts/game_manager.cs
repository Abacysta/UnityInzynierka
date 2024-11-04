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
using static Assets.classes.actionContainer;
using static Assets.classes.Relation;

public class game_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text turnCntTxt;
    [SerializeField] private AudioSource turn_sound;
    [SerializeField] private float RecruitablePopulationFactor = 0.2f;
    [SerializeField] private float PopulationFactor = 0.1f;
    [SerializeField] private int HappinessFactor = 5;
    [SerializeField] private float ArmyFactor = 0.1f;
    [SerializeField] private fog_of_war fog_Of_War;
    [SerializeField] private GameObject loading_box;
    [SerializeField] private Slider loading_bar;
    [SerializeField] private TMP_Text loading_txt;
    [SerializeField] private map_loader loader;
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
    public int turnCnt { get { return map.turnCnt; } }

    private Save toSave;

    public static readonly int WarHappinessPenaltyConst = 2;
    public static readonly int AllianceHappinessBonusConst = 1;
    public static readonly int VassalageHappinessBonusConstC1 = 1;
    public static readonly int VassalageHappinessPenaltyConstC2 = 1;

    // Loading map data before all scripts
    void Awake()
    {
        //LoadData();
    }

    private void Start() {
        while (loader == null) ;
        while (loader.loading) ;
        while (start_screen == null) ;
        start_screen.welcomeScreen();
		
	}

    void LoadData()
    {
        map.Map_name = "map1";
        map.File_name = "map_prototype_3";
        TextAsset jsonFile = Resources.Load<TextAsset>(map.File_name);

        if (jsonFile != null)
        {
            string jsonContent = "{\"provinces\":" + jsonFile.text + "}";
            JsonUtility.FromJsonOverwrite(jsonContent, map);
        }
        else
        {
            Debug.LogError("JSON map file not found in Resources!");
        }

    }

    public void UndoAll()
    {
        while(map.CurrentPlayer.Actions.Count > 0) {
            map.CurrentPlayer.Actions.revert();
        }
        //foreach (var army in map.Armies)
        //{
        //    if (army.position != army.destination)
        //    {
        //        army_view armyView = map.getView(army);
        //        if (armyView != null)
        //        {
        //            armyView.ReturnTo(army.position);
        //        }
        //        map.updateArmyDestination(army, army.position);
        //    }
        //}

        //foreach (var c in map.Countries)
        //{
        //    map.mergeArmies(c);
        //}
    }

    public void invokeEvent(int id) {
        map.Countries[id].Events[1].call();
    }

    public void undoLast() {
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
            if (!army_click_handler.IsTileAccessibleForArmyMovement(tilePosition))
            {
                Country armyOwner = map.Countries[army.OwnerId];

                // Move the army to the nearest province that meets one of the following requirements:
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
            List<Assets.classes.actionContainer.TurnAction> instants = c.Actions.extractInstants();
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
        

        //yield return new WaitForSeconds(2f);
        //map.moveArmies();
        //yield return new WaitForSeconds(2f);


        loading_txt.text = "Merging armies";
        foreach(var c in map.Countries) {
            loading_bar.value += 0.1f  * 100 / ccnt;
            map.mergeArmies(c);
        }
        foreach(var a in map.Armies.Where(a=>a.OwnerId != 0)) {
            map.Countries[a.OwnerId].modifyResource(Resource.Gold, a.Count * map.Countries[a.OwnerId].techStats.armyUpkeep);
        }
        fog_Of_War.StartTurn();
        turnCntTxt.SetText((++map.turnCnt).ToString());
        loading_box.SetActive(false);
        Debug.Log("stopped bar");
    }

    private void provinceCalc(int pcnt) {
        loading_txt.text = "Calculating provinces";
        Debug.Log("started bar");
        foreach(var p in map.Provinces) {
            loading_bar.value = (0.2f * 100 / pcnt);
            map.growPop(p.coordinates);
            map.calcRecruitablePop(p.coordinates);
            map.calcPopExtremes();
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
            p.Happiness += 3;
            if (p.Buildings.Any(b => b.BuildingType == BuildingType.School) && p.getBuilding(BuildingType.School).BuildingLevel < 4) resources[Resource.SciencePoint] += p.getBuilding(BuildingType.School).BuildingLevel * 3;
            resources[p.ResourcesT] += p.ResourcesP;
            resources[Resource.AP] += 0.1f;
        }
        
        resources[Resource.Gold] *= map.Countries[i].techStats.prodFactor;
        resources[Resource.Wood] *= map.Countries[i].techStats.prodFactor;
        resources[Resource.Iron] *= map.Countries[i].techStats.prodFactor;
        foreach (var army in map.getCountryArmies(map.CurrentPlayer)) {
            resources[Resource.Gold] -= (army.Count / 10 + 1) * map.Countries[i].techStats.armyUpkeep;
        }

        foreach (var res in resources) {
            map.Countries[i].modifyResource(res.Key, res.Value);
        }
        map.Countries[i].setResource(Resource.AP, resources[Resource.AP]);
    }

    public Dictionary<Resource, float> getGain(Country country) {
        var gain = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };
        var tax = getTaxGain(country);
        var prod = getResourceGain(country);
        foreach(var res in gain.Keys.ToList()) {
            if (res == Resource.Gold) gain[res] += tax;
            gain[res] += prod[res];
            gain[res] = (float)Math.Round(gain[res], 1);
        }
        return gain;
    }

    internal float getTaxGain(Country country) {
        var tax = 0f;
        foreach(var prov in country.Provinces) {
            tax += (prov.Population / 10)  * country.Tax.GoldP;
        }
        tax *= country.techStats.taxFactor;
        
        return (float)Math.Round(tax, 1);
    }

    internal Dictionary<Resource, float> getResourceGain(Country country) {
        var prod = new Dictionary<Resource, float> {
            { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };
        foreach (var prov in country.Provinces) {
            prod[prov.ResourcesT] += prov.ResourcesP;
            prod[Resource.AP] += 0.1f;
            if (prov.Buildings.Any(b => b.BuildingType == BuildingType.School) && prov.getBuilding(BuildingType.School).BuildingLevel < 4) prod[Resource.SciencePoint] += prov.getBuilding(BuildingType.School).BuildingLevel * 3;
        }
        foreach(var type in prod.ToList()) {
            if (type.Key != Resource.AP)
                 prod[type.Key] *= country.techStats.prodFactor;
            
        }
        foreach (var army in map.getCountryArmies(map.CurrentPlayer)) {
            prod[Resource.Gold] -= (army.Count/10 + 1)*country.techStats.armyUpkeep;
        }
        foreach(var type in prod.ToList()) {
            prod[type.Key] = (float)Math.Round(prod[type.Key], 1);
        }
        return prod;
    }

    private void countryCalc() {

        for(int i = 1; i < map.Countries.Count; i++) {
            ccc(i);
        }
    }
    public void armyReset()
    {
        foreach(Army army in map.Armies)
        {
            map.destroyArmyView(army);
            map.createArmyView(army);
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
        if (map.currentPlayer < map.Countries.Count - 1)
        {
            alertClear();
            map.currentPlayer++;
            diplomaticActionsManager.ResetRecevierButtonStates();
            
            Debug.Log($"Sending actions.");
        }
        else
        {
            Debug.Log($"Executing actions and performing calculations.");
            turn_sound.Play();
            rebellionCheck();
            executeActions();
            turnCalculations();
            for(int i = 0; i < map.Countries.Count; i++) {
                Debug.Log(map.Countries[i].Name + "--");
                Debug.Log("--" + map.Controllers[i]);
            }
            map.currentPlayer = 1;
			map.Countries[1].Events.Add(new Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
			map.Countries[1].Events.Add(new Event_.GlobalEvent.Happiness(map.Countries[1], dialog_box, camera_controller));
			//toSave = new(map);
            if (map.turnCnt % 5 == 0) {
				toSave = new(map);
				saveGame("autosave");
                toSave = null;
			}
			loader.Reload();
        }

        if (map.Controllers[map.currentPlayer] != Map.CountryController.Local) {
            aiTurn();
            TurnSimulation();
        }
        else {
            Debug.Log($"Now, it's country {map.CurrentPlayer.Id} - {map.CurrentPlayer.Name}'s turn");
            if (map.turnCnt == 0 && map.Controllers[map.currentPlayer] == Map.CountryController.Local) {
                start_screen.welcomeScreen();
            }
            else if (map.turnCnt == 1) {
                start_screen.unHide();
			}
            camera_controller.ZoomCameraOnCountry(map.currentPlayer);
            fog_Of_War.UpdateFogOfWar();
            armyReset();
            armyVisibilityManager.UpdateArmyVisibility(map.CurrentPlayer.RevealedTiles);
            map.UpdateAllArmyViewOrders();
            alerts.loadEvents(map.CurrentPlayer);
        }
    }
}