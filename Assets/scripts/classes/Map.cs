using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class Map:ScriptableObject {
    [SerializeField] private string map_name;
    [SerializeField] private string file_name;
    [SerializeField] private List<Province> provinces;
    [SerializeField] (int, int) selected_province;
    [SerializeField] (int, int) pop_extremes;
    // [SerializeField] private List<Country> countries;

    public string Map_name { get => map_name; set => map_name = value; }
    public string File_name { get => file_name; set => file_name = value; }
    public List<Province> Provinces { get => provinces; set => provinces = value; }

    public (int, int) Selected_province { get => selected_province; set => selected_province = value; }
    public (int, int) Pop_extremes { get => pop_extremes; set => pop_extremes = value; }

    public Province getProvince(int x, int y) {
        return Provinces.Find(p => p.X == x && p.Y == y);
    }

    public Province getProvince((int, int) coordinates) {
        return getProvince(coordinates.Item1, coordinates.Item2);
    }

    public int getProvinceIndex((int, int) coordinates) {
        return Provinces.FindIndex(p => p.X == coordinates.Item1 && p.Y == coordinates.Item2);
    }

    public int getProvinceIndex(int x, int y) { 
        return Provinces.FindIndex(p=>p.X == x && p.Y == y);
    }

    public void calcPopExtremes() {
        int min = Provinces.Min(p=>p.Population), max = Provinces.Max(p=>p.Population);
        Pop_extremes=(min,max);
    }

    public void growPop((int, int) coordinates, float factor) {
        int prov = getProvinceIndex(coordinates);
        Provinces[prov].Population += (int)Math.Floor(Provinces[prov].Population * factor);
    }

    public void calcRecruitablePop((int, int) coordinates, float factor) {
        int prov = getProvinceIndex(coordinates);
        Provinces[prov].RecruitablePopulation = (int)Math.Floor(Provinces[prov].Population * factor);
    }

    public void growHap((int, int) coordinates, int value) {
        int prov = getProvinceIndex(coordinates);
        Provinces[prov].Happiness += value;
    }
    public void addBuilding((int,int) coordinates, Building building)
    {
        int prov = getProvinceIndex(coordinates);
        bool buildingExist = Provinces[prov].Buildings.Any(b => b.BuildingType == building.BuildingType);
        if(!buildingExist)
        {
            Provinces[prov].Buildings.Add(building);
        }
        else
        {
            Debug.Log("Building of this type already exists!");
        }
    }

    public void upgradeBuilding((int,int) coordinates, BuildingType buildingType)
    {
        int prov = getProvinceIndex(coordinates);
        Building buildingToUpgrade = Provinces[prov].Buildings.Find(b => b.BuildingType == buildingType);
        buildingToUpgrade.Upgrade();
    }
    public void downgradeBuilding((int,int) coordinates, BuildingType buildingType)
    {
        int prov = getProvinceIndex(coordinates);
        Building buildingToDowngrade = Provinces[prov].Buildings.Find(b => b.BuildingType == buildingType);
        if(buildingToDowngrade.BuildingLevel > 1)
        {
            buildingToDowngrade.Downgrade();
        }
        else
        {
            Provinces[prov].Buildings.Remove(buildingToDowngrade);
        }
    }
}
