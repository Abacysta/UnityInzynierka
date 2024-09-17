using Assets.classes.subclasses;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class map_loader : MonoBehaviour
{
    private enum Mode {
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
    [SerializeField] private dialog_box_manager kurwa_mac;
    private Mode mode;

    void Start()
    {
        map.currentPlayer = 1;
        int i = 0;

        map.calcPopExtremes();
        map.Countries = new System.Collections.Generic.List<Country> {
            new Country(i++, "", (-1, -1), Color.white, map),
            new Country(i++, "Kingdom", (0, 0), Color.gray, map),
            new Country(i++, "TestFog", (9,9), Color.red, map)
        };
        i = 0;
        Debug.Log(map.CurrentPlayer.Name);
        foreach(Country country in map.Countries) {
            country.Priority = i++;
            Debug.Log(country.Id);
        }

        map.Countries[0].nullifyResources();
        map.getProvince((0, 0)).Owner_id = 1;
        map.assignProvince((0, 1), 1);
        map.assignProvince((1, 0), 1);
        map.assignProvince((2, 0), 1);
        map.assignProvince((3, 0), 1);
        map.getProvince((9, 9)).Owner_id = 2;
        map.assignProvince((8, 9), 2);

        int mapWidth = map.Provinces.Max(p => p.X);

        foreach(var p in map.Provinces) {
            p.Name = $"Province {p.Y * (mapWidth + 1) + p.X + 1}";
            map.calcRecruitablePop(p.coordinates);

            if(p.Type == "land") {
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
        map.Countries[1].Events.Add(new Assets.classes.Event_.GlobalEvent.Discontent(map.Countries[1], kurwa_mac));
        Army testArmy = new Army(1, 100, (1, 0), (1, 0));
        map.addArmy(testArmy);
        SetPolitical();
    }

    public void Reload() {
        switch(mode) {
            case Mode.Resource:
                SetResources(); break;
            case Mode.Happiness:
                SetHappiness(); break;
            case Mode.Population:
                SetPopulation(); break;
            case Mode.Political:
                SetPolitical(); break;
            default:
                SetTerrain(); break;
        }
    }

    public void SetTerrain()
    {
        mode = Mode.Terrain;
        filter_layer.ClearAllTiles();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.Type == "land")
            {
                base_layer.SetTile(position, base_tile);
                base_layer.SetColor(position, ChooseRGBColor(91, 106, 65)); // dark green
            }
            else
            {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();
    }

    public void SetResources()
    {
        mode = Mode.Resource;
        filter_layer.ClearAllTiles();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            Color color;

            if (province.Type == "land")
            {
                switch (province.Resources)
                {
                    case "gold":
                        color = ChooseRGBColor(255, 215, 0); // yellow
                        break;
                    case "iron":
                        color = ChooseRGBColor(169, 169, 169); // gray
                        break;
                    case "wood":
                        color = ChooseRGBColor(139, 69, 19); // brown
                        break;
                    case "empty":
                    default:
                        color = ChooseRGBColor(255, 255, 255); // white
                        break;
                }

                filter_layer.SetTile(position, base_tile);
                filter_layer.SetColor(position, color);
            }
            else
            {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();
    }

    public void SetHappiness()
    {
        mode = Mode.Happiness;
        filter_layer.ClearAllTiles();

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
    }

    public void SetPopulation()
    {
        mode = Mode.Population;
        filter_layer.ClearAllTiles();

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
    }

     public void SetPolitical() {
        mode = Mode.Political;
        filter_layer.ClearAllTiles();
        base_layer.ClearAllTiles();
        occupation_layer.ClearAllTiles();

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
}
