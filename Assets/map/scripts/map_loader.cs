using Assets.classes;
using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class map_loader : MonoBehaviour
{
    public class LegendItem
    {
        public Color Color { get; set; }
        public string Name { get; set; }

        public LegendItem(Color color, string name)
        {
            Color = color;
            Name = name;
        }
    }

    public enum MapMode {
        Terrain,
        Resource,
        Happiness,
        Population,
        Political,
        Diplomatic
    }

    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap occupation_layer;
    [SerializeField] private Tilemap terrain_feature_layer_1;
    [SerializeField] private Tilemap terrain_feature_layer_2;
    [SerializeField] private Tilemap filter_layer;

    [SerializeField] private TileBase base_tile;
    [SerializeField] private TileBase occupied_tile;
    [SerializeField] private TileBase water_tile;
    [SerializeField] private TileBase capital_tile;

    [SerializeField] private TilemapRenderer mouse_hover_layer_rnd;
    [SerializeField] private TilemapRenderer province_select_layer_rnd;
    [SerializeField] private TilemapRenderer filter_hover_layer_rnd;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private camera_controller camera_controller;
    [SerializeField] private GameObject mapmodes_buttons;
    [SerializeField] private Assets.map.scripts.diplomatic_relations_manager diplomacy;

    [SerializeField] private GameObject map_legend;
    [SerializeField] private TMP_Text filter_name;
    [SerializeField] private GameObject legend_item_prefab;

    private MapMode mode;
    public bool loading;

    public MapMode CurrentMode { get => mode; set => mode = value; }

    private void Awake()
    {
        SetMapLegendActivity(false);
    }

    void Start()
    {
        loading = true;
        map.currentPlayer = 1;
        int i = 0;

        map.calcPopExtremes();
        //map.Countries = new System.Collections.Generic.List<Country> {
        //    new Country(i++, "", (-1, -1), Color.white, map),
        //    new Country(i++, "Kingdom", (0, 0), Color.gray, map),
        //    new Country(i++, "Gauls", (9,9), Color.red, map),
        //    new Country(i++, "Berbers", (10, 0), Color.cyan, map),
        //    new Country(i++, "Egyptians", (20, 0), Color.blue, map),
        //    new Country(i++, "Vikings", (30, 0), Color.green, map),
        //    new Country(i++, "Huns", (40, 0), Color.yellow, map)
        //};
        map.addCountry(new Country(i++, "", (-1, -1), new Color(0.8392f, 0.7216f, 0.4706f), 1, map), Map.CountryController.Ai);
        map.addCountry(new Country(i++, "Kingdom", (0, 0), new Color(0.4392f, 0.5020f, 0.3451f), 2, map), Map.CountryController.Local);
        map.addCountry(new Country(i++, "Gauls", (9, 9), new Color(0.7686f, 0.3019f, 0.1176f), 3, map), Map.CountryController.Local);
        map.addCountry(new Country(i++, "Berbers", (10, 0), Color.cyan, 1, map), Map.CountryController.Local);
        map.addCountry(new Country(i++, "Egyptians", (20, 0), Color.blue, 2, map), Map.CountryController.Ai);
        map.addCountry(new Country(i++, "Vikings", (30, 0), Color.green, 3, map), Map.CountryController.Ai);
        map.addCountry(new Country(i++, "Huns", (40, 0), Color.yellow, 1, map), Map.CountryController.Ai);
        i = 0;
        Debug.Log(map.CurrentPlayer.Name);
        foreach(Country country in map.Countries) {
            country.Priority = i++;
            Debug.Log(country.Id);
        }

        map.Countries[1].Opinions = new Dictionary<int, int> { { 0, 0 }, { 2, -2 }, { 3, 1 }, { 4, 0 }, { 5, 2 }, { 6, -1 } };
        map.Countries[2].Opinions = new Dictionary<int, int> { { 0, 0 }, { 1, 3 }, { 3, 0 }, { 4, 1 }, { 5, -2 }, { 6, 2 } };
        map.Countries[3].Opinions = new Dictionary<int, int> { { 0, 0 }, { 1, -1 }, { 2, 1 }, { 4, 3 }, { 5, -1 }, { 6, -2 } };
        map.Countries[4].Opinions = new Dictionary<int, int> { { 0, 0 }, { 1, 0 }, { 2, -1 }, { 3, -2 }, { 5, 1 }, { 6, 3 } };
        map.Countries[5].Opinions = new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 0 }, { 3, 2 }, { 4, -1 }, { 6, 0 } };
        map.Countries[6].Opinions = new Dictionary<int, int> { { 0, 0 }, { 1, -3 }, { 2, 1 }, { 3, 0 }, { 4, 2 }, { 5, -1 } };

        map.Countries[0].nullifyResources();
        map.getProvince((0, 0)).Owner_id = 1;
        map.assignProvince((0, 1), 1);
        map.assignProvince((1, 0), 1);
        map.assignProvince((2, 0), 1);
        map.assignProvince((3, 0), 1);
        map.assignProvince((2, 1), 1);
        map.assignProvince((3, 2), 1);

        map.getProvince((9, 9)).Owner_id = 2;
        map.assignProvince((8, 9), 2);
        map.assignProvince((8, 8), 2);
        map.assignProvince((7, 8), 2);
        map.assignProvince((7, 7), 2);
        map.assignProvince((6, 7), 2);
        map.assignProvince((6, 6), 2);
        map.assignProvince((6, 5), 2);
        map.assignProvince((5, 5), 2);
        map.assignProvince((4, 5), 2);
        map.assignProvince((4, 4), 2);

        int mapWidth = map.Provinces.Max(p => p.X);

        foreach(var p in map.Provinces) {
            p.Name = $"Province {p.Y * (mapWidth + 1) + p.X + 1}";
            map.calcRecruitablePop(p.coordinates);

            if(true) {
                p.Statuses = new System.Collections.Generic.List<Status>();
                p.Buildings = new System.Collections.Generic.List<Building>{
                    new Building(BuildingType.Infrastructure, 0),
                    new Building(BuildingType.Fort, 0),
                    new Building(BuildingType.School, p.Population > 3000 ? 0 : 4),
                    new Building(BuildingType.Mine, p.Resources == "iron" ? 0 : 4)
                };
                p.OccupationInfo = new OccupationInfo();
                p.calcStatuses();
            }
        }
        map.getProvince((0, 0)).addStatus(new TaxBreak(3));
        map.getProvince(0, 0).addStatus(new Disaster(2));
        map.getProvince(1, 0).addStatus(new ProdBoom(3));
        map.getProvince((0, 0)).Buildings.Find(b => b.BuildingType == BuildingType.Infrastructure).Upgrade();
        map.Countries[1].Events.Add(new Assets.classes.Event_.GlobalEvent.Discontent(map.Countries[1], dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.LocalEvent.PlagueFound(map.Countries[1].Provinces.Last(), dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));

        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.AccessOffer(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));

        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));
        map.Countries[1].Events.Add(new Assets.classes.Event_.DiploEvent.WarDeclared(map.Countries[2], map.Countries[1], diplomacy, dialog_box, camera_controller));

        //map.Relations.Add(new Relation.War(map.Countries[1], map.Countries[2]));
        map.Relations.Add(new Relation.War(map.Countries[1], map.Countries[3]));
        map.Relations.Add(new Relation.Alliance(map.Countries[1], map.Countries[4]));
        Army testArmy = new Army(1, 100, (1, 0), (1, 0));
        map.addArmy(testArmy);
        map.addArmy(new Army(1, 10, (3, 3), (3, 3)));
        map.addArmy(new Army(2, 5, (4, 4), (4, 4)));
        map.addArmy(new Army(3, 5, (4, 4), (4, 4)));
        map.addArmy(new Army(3, 5, (0, 2), (0, 2))); // statek
        SetPolitical();
        loading = false;
    }

    public void Reload() {
        switch (mode) {
            case MapMode.Resource:
                SetResources(); break;
            case MapMode.Happiness:
                SetHappiness(); break;
            case MapMode.Population:
                SetPopulation(); break;
            case MapMode.Political:
                SetPolitical(); break;
            default:
                SetTerrain(); break;
        }
    }

    public void SetTerrain()
    {
        Color getTerrainColor(string type)
        {
            switch (type)
            {
                case "land":
                    return ChooseRGBColor(91, 106, 65); // dark green
                case "ocean":
                    return ChooseRGBColor(60, 106, 130); // blue
                default:
                    return ChooseRGBColor(91, 106, 65); // dark green
            }
        }

        mode = MapMode.Terrain;
        greyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.Type == "land")
            {
                base_layer.SetTile(position, base_tile);
                base_layer.SetColor(position, getTerrainColor("land"));
            }
            else
            {
                SetWater(position);
            }
        }

        SetProvinceHoverAndSelectAboveFilterLayer();

        filter_name.text = "Terrain:";

        List<LegendItem> legendItems = new()
        {
            new(getTerrainColor("land"), "Forest"),
            new(getTerrainColor("ocean"), "Sea")
        };
        SetMapLegend(legendItems);
        SetMapLegendActivity(true);
    }

    public void SetResources()
    {
        Color getResourceColor(string resources)
        {
            switch (resources)
            {
                case "gold":
                    return ChooseRGBColor(255, 215, 0); // yellow
                case "iron":
                    return ChooseRGBColor(169, 169, 169); // gray
                case "wood":
                    return ChooseRGBColor(139, 69, 19); // brown
                case "empty":
                default:
                    return ChooseRGBColor(255, 255, 255); // white
            }
        }

        mode = MapMode.Resource;
        greyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            Color color;

            if (province.Type == "land")
            {
                color = getResourceColor(province.Resources);
                filter_layer.SetTile(position, base_tile);
                filter_layer.SetColor(position, color);
            }
            else
            {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();

        filter_name.text = "Resources:";

        List<LegendItem> legendItems = new()
        {
            new(getResourceColor("gold"), "Gold"),
            new(getResourceColor("iron"), "Iron"),
            new(getResourceColor("wood"), "Wood"),
            new(getResourceColor("empty"), "Empty")
        };

        SetMapLegend(legendItems);
        SetMapLegendActivity(true);
    }

    public void SetHappiness()
    {
        mode = MapMode.Happiness;
        greyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.Type == "land")
            {
                Color happinessColor = GetColorBasedOnValueHappiness(province.Happiness);
                filter_layer.SetTile(position, base_tile);
                filter_layer.SetColor(position, happinessColor);
            }
            else
            {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();

        filter_name.text = "Happiness:";

        List<LegendItem> legendItems = new()
        {
            new(GetColorBasedOnValueHappiness(0), "0%"),
            new(GetColorBasedOnValueHappiness(25), "25%"),
            new(GetColorBasedOnValueHappiness(50), "50%"),
            new(GetColorBasedOnValueHappiness(75), "75%"),
            new(GetColorBasedOnValueHappiness(100), "100%")
        };

        SetMapLegend(legendItems);
        SetMapLegendActivity(true);
    }

    public void SetPopulation()
    {
        mode = MapMode.Population;
        greyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.Type == "land")
            {
                Color populationColor = GetColorBasedOnValuePop(province.Population);
                filter_layer.SetTile(position, base_tile);
                filter_layer.SetColor(position, populationColor);
            }
            else {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();

        filter_name.text = "Population:";

        int step = (map.Pop_extremes.Item2 - map.Pop_extremes.Item1) / 4;
        
        int RoundDownToHundreds(int value) => (int)(Math.Floor(value / 100.0) * 100);
        int RoundUpToHundreds(int value) => (int)(Math.Ceiling(value / 100.0) * 100);

        List<LegendItem> legendItems = new()
        {
            new(GetColorBasedOnValuePop(RoundDownToHundreds(map.Pop_extremes.Item1)), $"{RoundDownToHundreds(map.Pop_extremes.Item1)}"),
            new(GetColorBasedOnValuePop(RoundUpToHundreds(map.Pop_extremes.Item1 + step)), $"{RoundUpToHundreds(map.Pop_extremes.Item1 + step)}"),
            new(GetColorBasedOnValuePop(RoundUpToHundreds(map.Pop_extremes.Item1 + 2 * step)), $"{RoundUpToHundreds(map.Pop_extremes.Item1 + 2 * step)}"),
            new(GetColorBasedOnValuePop(RoundUpToHundreds(map.Pop_extremes.Item1 + 3 * step)), $"{RoundUpToHundreds(map.Pop_extremes.Item1 + 3 * step)}"),
            new(GetColorBasedOnValuePop(RoundUpToHundreds(map.Pop_extremes.Item2)), $"{RoundUpToHundreds(map.Pop_extremes.Item2)}"),
        };

        SetMapLegend(legendItems);
        SetMapLegendActivity(true);
    }

     public void SetPolitical() {
        mode = MapMode.Political;
        greyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces) {
            Country owner = map.Countries[province.Owner_id];
            Vector3Int position = new(province.X, province.Y, 0);

            if(province.Type == "land") {
                base_layer.SetTile(position, base_tile);
                base_layer.SetColor(position, owner.Color);
                if(owner.Capital == province.coordinates) terrain_feature_layer_2.SetTile(position, capital_tile);

                if (province.OccupationInfo.IsOccupied)
                {
                    Country occupier = map.Countries[province.OccupationInfo.OccupyingCountryId];
                    occupation_layer.SetTile(position, occupied_tile);
                    occupation_layer.SetColor(position, occupier.Color);
                }
            }
            else {
                SetWater(position);
            }
        }
        province_select_layer_rnd.sortingOrder = 4;
        mouse_hover_layer_rnd.sortingOrder = 5;

        filter_name.text = "Countries:";

        List<LegendItem> legendItems = new();

        for (int i = 0; i < map.Countries.Count; i++)
        {
            legendItems.Add(new LegendItem(map.Countries[i].Color, i == 0 ? "Tribal" : map.Countries[i].Name));
        }

        SetMapLegend(legendItems);
        SetMapLegendActivity(true);
    }

    public void SetDiplomatic() {

        Color GetDiplomaticColor(Relation.RelationType? relationType)
        {
            switch (relationType)
            {
                case Relation.RelationType.War:
                    return army_view.WarColor;
                case Relation.RelationType.Truce:
                    return army_view.TruceColor;
                case Relation.RelationType.Alliance:
                    return army_view.AllianceColor;
                case Relation.RelationType.Vassalage:
                    return army_view.VassalageColor;
                case Relation.RelationType.Rebellion:
                    return army_view.RebellionColor;
                default:
                    return army_view.DefaultColor;
            }
        }

        mode = MapMode.Diplomatic;
        greyOutUnused(mode);
        ClearLayers();

        foreach(Province province in map.Provinces) {
            Vector3Int position = new(province.X, province.Y, 0);

            if(province.Type == "land") {

                filter_layer.SetTile(position, base_tile);
                if (province.Owner_id == map.CurrentPlayer.Id)
                {
                    filter_layer.SetColor(position, army_view.DefaultColor);
                }
                else if (province.Owner_id == 0)
                {
                    filter_layer.SetColor(position, army_view.TribalColor);
                }
                else
                {
                    var relation = map.GetHardRelationType(map.CurrentPlayer, map.Countries[province.Owner_id]);
                    filter_layer.SetColor(position, GetDiplomaticColor(relation));
                }
            }
            else {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();

        filter_name.text = "Diplomacy:";

        List<LegendItem> legendItems = new()
        {
            new(GetDiplomaticColor(null), "You"),
            new(army_view.TribalColor, "Tribal"),
            new(GetDiplomaticColor(Relation.RelationType.War), "War"),
            new(GetDiplomaticColor(Relation.RelationType.Truce), "Truce"),
            new(GetDiplomaticColor(Relation.RelationType.Alliance), "Alliance"),
            new(GetDiplomaticColor(Relation.RelationType.Vassalage), "Vassalage"),
            new(GetDiplomaticColor(Relation.RelationType.Rebellion), "Rebellion"),
        };

        SetMapLegend(legendItems);
        SetMapLegendActivity(true);
    }

    private void SetWater(Vector3Int position)
    {
        base_layer.SetTile(position, base_tile);
        base_layer.SetColor(position, ChooseRGBColor(60, 106, 130)); // blue
        terrain_feature_layer_1.SetTile(position, water_tile);
    }

    Color GetColorBasedOnValueHappiness(int value)
    {
        Color minColor = Color.red;     
        Color midColor = Color.yellow; 
        Color maxColor = Color.green;  
    
        int minHappiness = 0;
        int maxHappiness = 100;

        float t = Mathf.InverseLerp(minHappiness, maxHappiness, value);

        if (t < 0.5f)
        {
            return Color.Lerp(minColor, midColor, t * 2);
        }
        else
        {
            return Color.Lerp(midColor, maxColor, (t - 0.5f) * 2);
        }
    }

    Color GetColorBasedOnValuePop(int value) {
        Color minColor = Color.white;
        Color midColor = Color.yellow;
        Color maxColor = Color.blue;

        int minPopulation = map.Pop_extremes.Item1;
        int maxPopulation = map.Pop_extremes.Item2;

        float t = Mathf.InverseLerp(minPopulation, maxPopulation, value);

        if(t < 0.5f) {
            return Color.Lerp(minColor, midColor, t * 2);
        }
        else {
            return Color.Lerp(midColor, maxColor, (t - 0.5f) * 2);
        } 
    }

    Color ChooseRGBColor(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    private void SetProvinceHoverAndSelectAboveFilterLayer()
    {
        province_select_layer_rnd.sortingOrder = filter_hover_layer_rnd.sortingOrder + 1;
        mouse_hover_layer_rnd.sortingOrder = filter_hover_layer_rnd.sortingOrder + 2;
    }

    private void greyOutUnused(map_loader.MapMode mapMode) {
        string name;
        switch (mapMode) {
            case MapMode.Terrain:
                name = "terrain";
                break;
            case MapMode.Resource:
                name = "res";
                break;
            case MapMode.Happiness:
                name = "happiness";
                break;
            case MapMode.Population:
                name = "population";
                break;
            case MapMode.Political:
                name = "political";
                break;
            case MapMode.Diplomatic:
                name = "diplo";
                break;
            default:
                name = "terrain";
                break;
        }
        foreach(Transform child in mapmodes_buttons.transform) {
            if(child.name != name + "_button") {
                child.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.HSVToRGB(0, 0, 0.2f);
            }
            else {
                child.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.HSVToRGB(0, 0, 1);
            }
        }
    }

    private void ClearLayers()
    {
        occupation_layer.ClearAllTiles();
        terrain_feature_layer_1.ClearAllTiles();
        terrain_feature_layer_2.ClearAllTiles();
        filter_layer.ClearAllTiles();
    }

    private void SetMapLegend(List<LegendItem> legendData)
    {
        for (int i = 2; i < map_legend.transform.childCount; i++)
        {
            Destroy(map_legend.transform.GetChild(i).gameObject);
        }

        foreach (LegendItem entry in legendData)
        {
            GameObject item = Instantiate(legend_item_prefab, map_legend.transform);
            legend_item_ui legendItem = item.GetComponent<legend_item_ui>();
            legendItem.SetLegendItem(entry.Color, entry.Name);
        }
    }

    private void SetMapLegendActivity(bool state)
    {
        map_legend.gameObject.SetActive(state);
    }
}