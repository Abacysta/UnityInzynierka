using Assets.classes.subclasses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Country
{
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int prio;
    [SerializeField] private (int, int) capital;
    [SerializeField] private Dictionary<Resource, float> resources;
    [SerializeField] private List<(int, int)> provinces;
    [SerializeField] private Color color;

    public Country(int id, string name, (int, int) capital, Color color) {
        this.id = id;
        this.name = name;
        this.capital = id==0 ? (-1, -1) : capital;
        this.color = id == 0 ? Color.white : color;
        this.resources = technicalDefaultResources.defaultValues;
        this.provinces = new List<(int, int)> { capital };
    }

    public void addProvince((int, int) coordinates) {
        provinces.Add(coordinates);
    }
    public void removeProvince((int, int) coordinates) { 
        
        if(coordinates != capital) provinces.Remove(coordinates);
    }

    public int Id { get { return id; } }
    public string Name { get { return name; } }
    public int Priority { get { return prio; } set => prio = value; }
    public Color Color { get { return color; } }
    public Dictionary<Resource, float> Resources { get { return resources; } }
    public List<(int, int)> Provinces { get { return provinces; } }
    public (int, int) Capital {  get { return capital; } }

    public void modifyResource((Resource, float) values) {
        resources[values.Item1] += values.Item2;
    }

    public void setResource((Resource, float) values) {
        resources[values.Item1] = values.Item2;
    }

    public void nullifyResources() {
        resources = null;
    }

    public void changeCapital((int, int) coordinates) {
        if(provinces.Contains(coordinates)) {
            capital = coordinates;
        }
    }
}


