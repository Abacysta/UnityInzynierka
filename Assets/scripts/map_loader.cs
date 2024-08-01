using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class map_loader : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap tile_map_layer_1;
    [SerializeField] private Tilemap tile_map_layer_2;
    [SerializeField] private TileBase base_tile;
    [SerializeField] private TileBase occupied_tile;
    [SerializeField] private TileBase water_tile;
    [SerializeField] private TileBase capital_tile;

    void Start()
    {
        /*
        tile_map_layer_2.SetTile(new Vector3Int(3, 4, 0), occupied_tile);
        tile_map_layer_2.SetColor(new Vector3Int(3, 4, 0), choose_rgb_color(255, 255, 255, 150)); // occupied Tile transparency
        tile_map_layer_1.SetColor(new Vector3Int(3, 4, 0), choose_rgb_color(234, 98, 84)); // red
        tile_map_layer_1.SetColor(new Vector3Int(3, 3, 0), choose_rgb_color(234, 98, 84)); // red
        tile_map_layer_2.SetTile(new Vector3Int(3, 3, 0), capital_tile);
        tile_map_layer_1.SetColor(new Vector3Int(4, 4, 0), choose_rgb_color(234, 98, 84));
        */

        SetTerrain();
        //SetHappiness();
        //SetPopulation();
    }

    public void SetTerrain()
    {
        tile_map_layer_1.ClearAllTiles();
        tile_map_layer_2.ClearAllTiles();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.Type == "land")
            {
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, ChooseRGBColor(91, 106, 65)); // dark green
            }
            else
            {
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, ChooseRGBColor(60, 106, 130)); // blue
                tile_map_layer_2.SetTile(position, water_tile);
            }
        }
    }

    public void SetResources()
    {
        tile_map_layer_1.ClearAllTiles();
        tile_map_layer_2.ClearAllTiles();

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

                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, color);
            }
            else
            {
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, ChooseRGBColor(60, 106, 130)); // blue
                tile_map_layer_2.SetTile(position, water_tile);
            }
        }
    }

    public void SetHappiness()
    {
        tile_map_layer_1.ClearAllTiles();
        tile_map_layer_2.ClearAllTiles();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.Type == "land")
            {
                Color happinessColor = GetColorBasedOnValueHappiness(province.Happiness);
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, happinessColor);
            }
            else
            {
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, ChooseRGBColor(60, 106, 130)); // blue
                tile_map_layer_2.SetTile(position, water_tile);
            }
        }
    }

    public void SetPopulation()
    {

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.Type == "land")
            {
                Color populationColor = GetColorBasedOnValuePop(province.Population);
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, populationColor);
            }
            else{
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, ChooseRGBColor(60, 106, 130)); // blue
                tile_map_layer_2.SetTile(position, water_tile);
            }
        }
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
        
                int minPopulation = map.Provinces.Min(p=>p.Population);
                int maxPopulation = map.Provinces.Max(p => p.Population); ;

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
