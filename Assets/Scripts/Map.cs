using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map
{
    [SerializeField] private string name;
    [SerializeField] private string file_name;
    [SerializeField] private List<Province> provinces;
    // [SerializeField] private List<Country> countires;

    public Map(string name, string file_name)
    {
        this.Name = name;
        this.File_name = file_name;
    }

    public string Name { get => name; set => name = value; }
    public string File_name { get => file_name; set => file_name = value; }
    public List<Province> Provinces { get => provinces; set => provinces = value; }
}
