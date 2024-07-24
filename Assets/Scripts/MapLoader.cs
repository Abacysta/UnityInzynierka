using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[System.Serializable]
public class Map
{
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private List<Province> provinces;

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public int Width
    {
        get { return width; }
        set { width = value; }
    }

    public int Height
    {
        get { return height; }
        set { height = value; }
    }

    public List<Province> Provinces
    {
        get { return provinces; }
        set { provinces = value; }
    }
}

public class MapLoader : MonoBehaviour
{
    [SerializeField] private TileBase landTile;
    [SerializeField] private TileBase occupiedTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private Tilemap tilemap;


private Vector2 mapSize;


    void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("mapData");

        if (jsonTextAsset != null)
        {
            string jsonContent = jsonTextAsset.text;
            Map map = JsonUtility.FromJson<Map>(jsonContent);
            GenerateMap(map);
            Debug.Log("Map loaded successfully!");
        }
        else
        {
            Debug.LogError("JSON map file not found in Resources!");
        }
    }

    void GenerateMap(Map map)
    {
        foreach (Province province in map.Provinces)
        {
            Vector3Int position = new(province.X, province.Y, 0);

            if (province.IsLand)
            {
                tilemap.SetTile(position, landTile);
                tilemap.SetColor(position, new Color(209 / 255f, 175 / 255f, 112 / 255f, 255 / 255f)); // orange
            }
            else
            {
                tilemap.SetTile(position, waterTile);
                tilemap.SetColor(position, new Color(60 / 255f, 106 / 255f, 130 / 255f, 255 / 255f)); // blue
            }
        }

        tilemap.SetTile(new Vector3Int(3, 4, 0), occupiedTile);
        tilemap.SetColor(new Vector3Int(3, 4, 0), new Color(234 / 255f, 98 / 255f, 84 / 255f, 255 / 255f)); // red
        tilemap.SetColor(new Vector3Int(3, 3, 0), new Color(234 / 255f, 98 / 255f, 84 / 255f, 255 / 255f)); // red
    }

}
