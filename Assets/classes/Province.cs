using Assets.classes.subclasses;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Province {

    

    

    [SerializeField] private string id;
    [SerializeField] private string name;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private string type;
    [SerializeField] private string resources;
    [SerializeField] private int resources_amount;
    [SerializeField] private int population;
    [SerializeField] private int recruitable_population;
    [SerializeField] private int happiness;
    [SerializeField] private bool is_coast;
    [SerializeField] private OccupationInfo occupation_info;
    [SerializeField] private int owner_id;
    [SerializeField] private List<Building> buildings;
    private List<Status> statuses;
    private float prod_mod = 1, pop_mod = 1, pop_static = 0, happ_mod = 1, happ_static = 0, tax_mod = 1, rec_pop = 1; 

    public Province(string id, string name, int x, int y, string type, string resources, int resources_amount, int population, int recruitable_population, int happiness, bool is_coast, OccupationInfo occupation_info, int owner_id) {
        this.id = id;
        this.name = name;
        this.x = x;
        this.y = y;
        this.type = type;
        this.resources = resources;
        this.resources_amount = resources_amount;
        this.population = population;
        this.recruitable_population = recruitable_population;
        this.happiness = happiness;
        this.is_coast = is_coast;
        this.occupation_info = occupation_info;
        this.owner_id = owner_id;
        this.buildings = new List<Building>();
        this.statuses = new List<Status>();
    }

    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public string Type { get => type; set => type = value; }
    public string Resources { get => resources; set => resources = value; }
    public int Resources_amount { get => resources_amount; set => resources_amount = value; }
    public int Population { get => population; set => population = value; }
    public int RecruitablePopulation { get => recruitable_population; set => recruitable_population = value; }
    public int Happiness { get => happiness; set => happiness = value; }
    public bool Is_coast { get => is_coast; set => is_coast = value; }
    public OccupationInfo OccupationInfo { get => occupation_info; set => occupation_info = value; }
    public int Owner_id { get => owner_id; set => owner_id = value; }
    public List<Building> Buildings { get => buildings; set => buildings = value; }
    public (int, int) coordinates { get => (x, y); }

    public Resource ResourcesT { get => RealResource(); }
    public float Prod_mod { get => prod_mod; set => prod_mod = value; }
    public float Pop_mod { get => pop_mod; set => pop_mod = value; }
    public float Pop_static { get => pop_static; set => pop_static = value; }
    public float Happ_mod { get => happ_mod; set => happ_mod = value; }
    public float Happ_static { get => happ_static; set => happ_static = value; }
    public float Tax_mod { get => tax_mod; set => tax_mod = value; }
    public float Rec_pop { get => rec_pop; set => rec_pop = value; }

    private Resource RealResource() {
        switch(Resources) {
            case "iron": return Resource.Iron;
            case "wood": return Resource.Wood;
            case "gold": return Resource.Gold;
            default: return Resource.AP;
        }
    }

    public void calcEffects() {
        prod_mod = 1;
        pop_mod = 1;
        pop_static = 0;
        happ_mod = 1;
        happ_static = 0;
        tax_mod = 1;
        foreach(var status in statuses) {
            if(0 != status.duration--)
            status.applyEffect(this);
            else statuses.Remove(status);
        }
    }


}

