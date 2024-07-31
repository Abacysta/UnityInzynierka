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
    [SerializeField] private TileBase sad_tile;
    [SerializeField] private TileBase neutral_tile;
    [SerializeField] private TileBase smile_tile;
    [SerializeField] private TileBase happy_tile;

    [SerializeField] private TileBase population_0_tile;
    [SerializeField] private TileBase population_1_tile;
    [SerializeField] private TileBase population_2_tile;
    [SerializeField] private TileBase population_3_tile;
    [SerializeField] private TileBase population_4_tile;
    [SerializeField] private TileBase population_5_tile;

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
                if (province.Happiness < 25)
                {
                    tile_map_layer_1.SetTile(position, base_tile);
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(220, 20, 60)); // red - crimson
                    tile_map_layer_2.SetTile(position, sad_tile);
                    tile_map_layer_2.SetColor(position, ChooseRGBColor(255, 255, 255, 150)); // transparency
                }
                else if (province.Happiness < 50)
                {
                    tile_map_layer_1.SetTile(position, base_tile);
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(225, 225, 0)); // yellow
                    tile_map_layer_2.SetTile(position, neutral_tile);
                    tile_map_layer_2.SetColor(position, ChooseRGBColor(255, 255, 255, 150)); // transparency
                }
                else if (province.Happiness < 75)
                {
                    tile_map_layer_1.SetTile(position, base_tile);
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(0, 255, 0)); // green - lime
                    tile_map_layer_2.SetTile(position, smile_tile);
                    tile_map_layer_2.SetColor(position, ChooseRGBColor(255, 255, 255, 150)); // transparency
                }
                else
                {
                    tile_map_layer_1.SetTile(position, base_tile);
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(0, 100, 0)); // dark green
                    tile_map_layer_2.SetTile(position, happy_tile);
                    tile_map_layer_2.SetColor(position, ChooseRGBColor(255, 255, 255, 150)); // transparency
                }
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
        SetTerrain();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);
            if (province.Type == "land")
            {
                /* Color populationColor = GetColorBasedOnPopulation(province.Population);
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, populationColor);
                */
                if(province.Population < 100){
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(255,155,133)); // coral pink
                    tile_map_layer_2.SetTile(position, population_0_tile);
                }
                else if(province.Population < 200){
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(255,122,92)); // bittersweet 
                    tile_map_layer_2.SetTile(position, population_1_tile);
                }
                else if(province.Population < 300){
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(255,88,51)); // tomato
                    tile_map_layer_2.SetTile(position, population_2_tile);
                }
                else if(province.Population < 400){
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(255,55,10)); // coquelicot
                    tile_map_layer_2.SetTile(position, population_3_tile);
                }
                else if(province.Population < 500){
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(224,41,0)); // sinopia
                    tile_map_layer_2.SetTile(position, population_4_tile);
                }
                else {
                    tile_map_layer_1.SetColor(position, ChooseRGBColor(184,34,0)); // engineering orange 
                    tile_map_layer_2.SetTile(position, population_5_tile);
                }
            }
        }
    }

    Color GetColorBasedOnPopulation(int population)
    {
        // Define the range for the colors
        int minPopulation = 0;
        int maxPopulation = 600;

        // Define colors for the range
        Color minColor = ChooseRGBColor(255,165,0); // orange 
        Color maxColor = ChooseRGBColor(165,42,42); //brown - chocolate


        // Interpolate between yellow and brown
        float t = Mathf.InverseLerp(minPopulation, maxPopulation, population);
        return Color.Lerp(minColor, maxColor, t);
    }

    Color ChooseRGBColor(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
