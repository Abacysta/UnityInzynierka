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
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap occupation_color_layer;
    [SerializeField] private Tilemap terrain_feature_layer_1;
    [SerializeField] private Tilemap terrain_feature_layer_2;
    [SerializeField] private Tilemap filter_layer;

    [SerializeField] private TileBase base_tile;
    [SerializeField] private TileBase occupied_tile;
    [SerializeField] private TileBase water_tile;
    [SerializeField] private TileBase capital_tile;

    [SerializeField] private TilemapRenderer mouse_hover_layer_rnd;


    void Start()
    {
        int i = 0;

        map.calcPopExtremes();
        map.Countries = new System.Collections.Generic.List<Country> {
            new Country(i++, "", (-1, -1), Color.white),
            new Country(i++, "Kingdom", (0, 0), Color.gray),
            new Country(i++, "TestFog", (9,9), Color.red)
        };
        i = 0;
        foreach(Country country in map.Countries) {
            country.Priority = i++;
            Debug.Log(country.Id);
        }

        map.Countries[0].nullifyResources();
        map.getProvince((0, 0)).Owner_id = 1;
        map.assignProvince((0, 1), 1);
        map.assignProvince((1, 0), 1);

        map.getProvince((9, 9)).Owner_id = 2;
        map.assignProvince((8, 9), 2);


        foreach(var p in map.Provinces) {
            map.calcRecruitablePop(p.coordinates);

            if(p.Type == "land") {
                p.Statuses = new System.Collections.Generic.List<Status>();
                p.Buildings = new System.Collections.Generic.List<Building>{
                    new Building(BuildingType.Infrastructure, 0),
                    new Building(BuildingType.Fort, 0),
                    new Building(BuildingType.School, p.Population > 3000 ? 0 : 4),
                    new Building(BuildingType.Mine, p.Resources == "iron" ? 0 : 4)
                };

                if(p.Owner_id != 0 && p.Owner_id != null) {
                    map.assignProvince(p.coordinates, p.Owner_id);
                }
                p.calcStatuses(map.Countries);
            }
        }
        map.getProvince((0, 0)).addStatus(new TaxBreak(3));
        map.getProvince(0, 0).addStatus(new Disaster(2));
        map.getProvince(1, 0).addStatus(new ProdBoom(3));
        map.getProvince((0, 0)).Buildings.Find(b => b.BuildingType == BuildingType.Infrastructure).Upgrade();
        Army testArmy = new Army(1, 100, (1, 0), (1, 0));
        map.addArmy(testArmy);
        SetPolitical();
    }

    public void SetTerrain()
    {
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
        mouse_hover_layer_rnd.sortingOrder = 8;
    }

    public void SetResources()
    {
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
        mouse_hover_layer_rnd.sortingOrder = 8;
    }

    public void SetHappiness()
    {
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
        mouse_hover_layer_rnd.sortingOrder = 8;
    }

    public void SetPopulation()
    {
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
        mouse_hover_layer_rnd.sortingOrder = 8;
    }

     public void SetPolitical() {
        filter_layer.ClearAllTiles();

        foreach (Province province in map.Provinces) {
            Country owner = map.Countries[province.Owner_id];
            Vector3Int position = new(province.X, province.Y, 0);

            if(province.Type == "land") {
                base_layer.SetTile(position, base_tile);
                base_layer.SetColor(position, owner.Color);
                if(owner.Capital == province.coordinates) terrain_feature_layer_2.SetTile(position, capital_tile);
            }
            else {
                SetWater(position);
            }
        }
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
}
