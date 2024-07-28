using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
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
        map = new Map("map_prototype", "map_prototype");
        TextAsset jsonFile = Resources.Load<TextAsset>(map.File_name);

        if (jsonFile == null)
        {
            Debug.LogError("JSON map file not found in Resources!");
        }
        else
        {
            string jsonContent = "{\"provinces\":" + jsonFile.text + "}";
            //map.Provinces = JsonUtility.FromJson<List<Province>>(jsonContent);
            map = JsonUtility.FromJson<Map>(jsonContent);

            /*set_land_and_water();

            tile_map_layer_2.SetTile(new Vector3Int(3, 4, 0), occupied_tile);
            tile_map_layer_2.SetColor(new Vector3Int(3, 4, 0), choose_rgb_color(255, 255, 255, 150)); // occupied Tile transparency
            tile_map_layer_1.SetColor(new Vector3Int(3, 4, 0), choose_rgb_color(234, 98, 84)); // red
            tile_map_layer_1.SetColor(new Vector3Int(3, 3, 0), choose_rgb_color(234, 98, 84)); // red
            tile_map_layer_2.SetTile(new Vector3Int(3, 3, 0), capital_tile);
            tile_map_layer_1.SetColor(new Vector3Int(4, 4, 0), choose_rgb_color(234, 98, 84));
            */


            set_resources();
        }
    }

    void set_land_and_water()
    {
        tile_map_layer_1.ClearAllTiles();
        tile_map_layer_2.ClearAllTiles();

        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.Type == "land")
            {
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, choose_rgb_color(91, 106, 65)); // dark green
            }
            else
            {
                tile_map_layer_1.SetTile(position, base_tile);
                tile_map_layer_1.SetColor(position, choose_rgb_color(60, 106, 130)); // blue
                tile_map_layer_2.SetTile(position, water_tile);
            }
        }
    }

    void set_resources()
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
                    color = choose_rgb_color(255, 215, 0); // yellow
                    break;
                case "iron":
                    color = choose_rgb_color(169, 169, 169); // gray
                    break;
                case "wood":
                    color = choose_rgb_color(139, 69, 19); // brown
                    break;
                case "empty":
                default:
                    color = choose_rgb_color(255, 255, 255); // white
                    break;
            }
            tile_map_layer_1.SetTile(position, base_tile);
            tile_map_layer_1.SetColor(position, color);
        }
    }

    Color choose_rgb_color(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

}
