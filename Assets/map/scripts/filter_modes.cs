using Assets.classes;
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

    public static readonly Color WarColor = new(1f, 0f, 0f); // Red
    public static readonly Color TruceColor = new(0.8f, 0.9f, 0.8f); // Light Green
    public static readonly Color AllianceColor = new(0.5f, 0.8f, 1f); // Light Blue
    public static readonly Color VassalageColor = new(0.6f, 0.4f, 0.8f); // Purple
    public static readonly Color RebellionColor = new(0.8f, 0.4f, 0.8f); // Pink
    public static readonly Color DefaultColor = new(0.96f, 0.76f, 0.76f); // Soft Salmon
    public static readonly Color TribalColor = new(0.9f, 0.75f, 0.6f); // Light Beige
    public static readonly Color CurrentPlayerColor = new(0.97f, 0.92f, 0.46f); // Yellow

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
        SetTerrainFeatures();
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
            Country owner = map.Countries[province.Owner_id];
            Vector3Int position = new(province.X, province.Y, 0);

            int hash = province.X * 73856093 ^ province.Y * 19349663 ^ (int)province.Terrain * 83492791;
            hash = Math.Abs(hash);
            float pseudoRandomValue = (hash % 1000) / 1000.0f;

            if (province.coordinates != owner.Capital && pseudoRandomValue < fillProbability)
            {
                TileBase selectedTile = null;

                switch (province.Terrain)
                {
                    case Province.TerrainType.lowlands:
                        selectedTile = lowlands_1;
                        break;
                    case Province.TerrainType.desert:
                        selectedTile = (hash % 3) switch
                        {
                            0 => desert_1,
                            1 => desert_2,
                            2 => desert_3,
                            _ => null
                        };
                        break;
                    case Province.TerrainType.tundra:
                        selectedTile = (hash % 2) switch
                        {
                            0 => tundra_1,
                            1 => tundra_2,
                            _ => null
                        };
                        break;
                    case Province.TerrainType.forest:
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
        Color getTerrainColor(Province.TerrainType type)
        {
            switch (type)
            {
                case Province.TerrainType.tundra:
                    return ChooseRGBColor(0, 102, 0); // dark green
                case Province.TerrainType.lowlands:
                    return ChooseRGBColor(0,255,0); // lime
                case Province.TerrainType.forest:
                    return ChooseRGBColor(0,204,102); // green/blue?
                case Province.TerrainType.desert:
                    return ChooseRGBColor(255,204,0); // yellow/orange
                case Province.TerrainType.ocean:
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
                base_layer.SetColor(position, getTerrainColor(province.Terrain));
            }
            else
            {
                SetWater(position);
            }
        }

        SetProvinceHoverAndSelectAboveFilterLayer();
        SetTerrainFeatures();
    }

    public void SetResources()
    {
        Color getResourceColor(Resource resourceType)
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
        greyOutUnused(mode);
        ClearLayers();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            Color color;

            if (province.Type == "land")
            {
                color = getResourceColor(province.ResourceType);
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
        SetTerrainFeatures();
    }

    public void SetDiplomatic() {

        Color GetDiplomaticColor(Relation.RelationType? relationType)
        {
            switch (relationType)
            {
                case Relation.RelationType.War:
                    return WarColor;
                case Relation.RelationType.Truce:
                    return TruceColor;
                case Relation.RelationType.Alliance:
                    return AllianceColor;
                case Relation.RelationType.Vassalage:
                    return VassalageColor;
                case Relation.RelationType.Rebellion:
                    return RebellionColor;
                default:
                    return DefaultColor;
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
                    filter_layer.SetColor(position, CurrentPlayerColor);
                }
                else if (province.Owner_id == 0)
                {
                    filter_layer.SetColor(position, TribalColor);
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
        province_select_layer_rnd.sortingOrder = filter_layer_rnd.sortingOrder + 1;
        mouse_hover_layer_rnd.sortingOrder = filter_layer_rnd.sortingOrder + 2;
    }

    private void greyOutUnused(filter_modes.MapMode mapMode) {
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