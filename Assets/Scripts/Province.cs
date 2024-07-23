using UnityEngine;

[System.Serializable]
public class Province
{
    [SerializeField] private string id;
    [SerializeField] private string name;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private bool isLand;
    [SerializeField] private int[] color;
    [SerializeField] private string[] neighbors;
    [SerializeField] private string producedResource;

    public string Id
    {
        get { return id; }
        set { id = value; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public int X
    {
        get { return x; }
        set { x = value; }
    }

    public int Y
    {
        get { return y; }
        set { y = value; }
    }

    public bool IsLand
    {
        get { return isLand; }
        set { isLand = value; }
    }


    public int[] Color
    {
        get { return color; }
        set { color = value; }
    }

    public string[] Neighbors
    {
        get { return neighbors; }
        set { neighbors = value; }
    }
    public string ProducedResource
    {
        get { return producedResource; }
        set { producedResource = value; }
    }

    public Province(string id, string name, int x, int y, bool isLand, int[] color, string[] neighbors, string producedResource)
    {
        Id = id;
        Name = name;
        X = x;
        Y = y;
        IsLand = isLand;
        Color = color;
        Neighbors = neighbors;
        ProducedResource = producedResource;
    }

    public Province(string name)
    {
        Name = name; 
    }


}