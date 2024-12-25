using Assets.classes.subclasses.Constants;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class filter_modes : MonoBehaviour
{
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

    [SerializeField] private TileBase desert_1;
    [SerializeField] private TileBase desert_2;
    [SerializeField] private TileBase desert_3;
    [SerializeField] private TileBase forest_1;
    [SerializeField] private TileBase lowlands_1;
    [SerializeField] private TileBase tundra_1;
    [SerializeField] private TileBase tundra_2;

    [SerializeField] private TilemapRenderer mouse_hover_layer_rnd;
    [SerializeField] private TilemapRenderer province_select_layer_rnd;
    [SerializeField] private TilemapRenderer filter_layer_rnd;
    [SerializeField] private GameObject mapmodes_buttons;

    private MapMode mode;

    public MapMode CurrentMode { get => mode; set => mode = value; }

    void Start()
    {
        SetPolitical();
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
            case MapMode.Diplomatic:
                SetDiplomatic(); break;
            default:
                SetTerrain(); break;
        }
    }

    private void SetTerrainFeatures()
    {
        float fillProbability = 0.35f;

        foreach (Province province in map.Provinces)
        {
            Country owner = map.Countries[province.OwnerId];
            Vector3Int position = new(province.X, province.Y, 0);

            int hash = province.X * 73856093 ^ province.Y * 19349663 ^ (int)province.Terrain * 83492791;
            hash = Math.Abs(hash);
            float pseudoRandomValue = (hash % 1000) / 1000.0f;

            if (province.Coordinates != owner.Capital && pseudoRandomValue < fillProbability)
            {
                TileBase selectedTile = null;

                switch (province.Terrain)
                {
                    case Province.TerrainType.Lowlands:
                        selectedTile = lowlands_1;
                        break;
                    case Province.TerrainType.Desert:
                        selectedTile = (hash % 3) switch
                        {
                            0 => desert_1,
                            1 => desert_2,
                            2 => desert_3,
                            _ => null
                        };
                        break;
                    case Province.TerrainType.Tundra:
                        selectedTile = (hash % 2) switch
                        {
                            0 => tundra_1,
                            1 => tundra_2,
                            _ => null
                        };
                        break;
                    case Province.TerrainType.Forest:
                        selectedTile = forest_1;
                        break;
                }

                if (selectedTile != null)
                {
                    terrain_feature_layer_1.SetTile(position, selectedTile);
                }
            }
        }
    }

    public void SetTerrain()
    {
        Color GetTerrainColor(Province.TerrainType type)
        {
            switch (type)
            {
                case Province.TerrainType.Tundra:
                    return ChooseRGBColor(0, 102, 0); // dark green
                case Province.TerrainType.Lowlands:
                    return ChooseRGBColor(0, 255, 0); // lime
                case Province.TerrainType.Forest:
                    return ChooseRGBColor(0, 204, 102); // emerald green
                case Province.TerrainType.Desert:
                    return ChooseRGBColor(255, 204, 0); // golden yellow
                case Province.TerrainType.Ocean:
                    return ChooseRGBColor(60, 106, 130); // blue
                default:
                    return ChooseRGBColor(91, 106, 65); // dark green
            }
        }

        mode = MapMode.Terrain;
        GreyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.IsLand)
            {
                base_layer.SetTile(position, base_tile);
                base_layer.SetColor(position, GetTerrainColor(province.Terrain));
            }
            else
            {
                SetWater(position);
            }
        }

        SetProvinceHoverAndSelectBelowFilterLayer();
        SetTerrainFeatures();
    }

    public void SetResources()
    {
        Color GetResourceColor(Resource resourceType)
        {
            switch (resourceType)
            {
                case Resource.Gold:
                    return ChooseRGBColor(255, 215, 0); // yellow
                case Resource.Iron:
                    return ChooseRGBColor(169, 169, 169); // gray
                case Resource.Wood:
                    return ChooseRGBColor(139, 69, 19); // brown
                case Resource.AP:
                default:
                    return ChooseRGBColor(0, 255, 115); // green
            }
        }

        mode = MapMode.Resource;
        GreyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            Color color;

            if (province.IsLand)
            {
                color = GetResourceColor(province.ResourceType);
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
        mode = MapMode.Happiness;
        GreyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.IsLand)
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
        mode = MapMode.Population;
        GreyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.IsLand)
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
        mode = MapMode.Political;
        GreyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces) {
            Country owner = map.Countries[province.OwnerId];
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.IsLand) {
                base_layer.SetTile(position, base_tile);
                base_layer.SetColor(position, owner.Color);

                if (owner.Capital == province.Coordinates) 
                    terrain_feature_layer_2.SetTile(position, capital_tile);

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
        SetProvinceHoverAndSelectBelowFilterLayer();
        SetTerrainFeatures();
    }

    public void SetDiplomatic() {
        mode = MapMode.Diplomatic;
        GreyOutUnused(mode);
        ClearLayers();

        foreach(Province province in map.Provinces) {
            Vector3Int position = new(province.X, province.Y, 0);

            if(province.IsLand) {

                filter_layer.SetTile(position, base_tile);
                if (province.OwnerId == map.CurrentPlayer.Id)
                {
                    filter_layer.SetColor(position, RelationConstants.CURRENT_PLAYER_COLOR);
                }
                else if (province.OwnerId == 0)
                {
                    filter_layer.SetColor(position, RelationConstants.TRIBAL_COLOR);
                }
                else
                {
                    var relation = map.GetHardRelationType(map.CurrentPlayer, map.Countries[province.OwnerId]);
                    filter_layer.SetColor(position, RelationConstants.GetDiplomaticColor(relation));
                }
            }
            else {
                SetWater(position);
            }
        }
        SetProvinceHoverAndSelectAboveFilterLayer();
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

        int minPopulation = map.PopExtremes.Item1;
        int maxPopulation = map.PopExtremes.Item2;

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
        province_select_layer_rnd.sortingOrder = filter_layer_rnd.sortingOrder + 1;
        mouse_hover_layer_rnd.sortingOrder = filter_layer_rnd.sortingOrder + 2;
    }

    private void SetProvinceHoverAndSelectBelowFilterLayer()
    {
        province_select_layer_rnd.sortingOrder = 4;
        mouse_hover_layer_rnd.sortingOrder = 5;
    }

    private void GreyOutUnused(filter_modes.MapMode mapMode) {
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
        terrain_feature_layer_2.ClearAllTiles();
        filter_layer.ClearAllTiles();
    }
}