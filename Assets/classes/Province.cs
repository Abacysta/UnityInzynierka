using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProvinceModifiers
{
    public float ProdMod { get; set; } = 1;
    public float PopMod { get; set; } = 1;
    public float PopStatic { get; set; } = 0;
    public float HappMod { get; set; } = 1;
    public float HappStatic { get; set; } = 0;
    public float TaxMod { get; set; } = 1;
    public float RecPop { get; set; } = 1;

    public void ResetModifiers()
    {
        ProdMod = 1;
        PopMod = 1;
        PopStatic = 0;
        HappMod = 1;
        HappStatic = 0;
        TaxMod = 1;
        RecPop = 1;
    }
}

[System.Serializable]
public class Province {
    public enum TerrainType {
        tundra,
        forest,
        lowlands,
        desert,
        ocean
    }

    [SerializeField] private string id;
    [SerializeField] private string name;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private string type;
    [SerializeField] private string resources;
    [SerializeField] private float resources_amount;
    [SerializeField] private int population;
    [SerializeField] private int recruitable_population;
    [SerializeField] private int happiness;
    [SerializeField] private bool is_coast;
    [SerializeField] private OccupationInfo occupationInfo;
    [SerializeField] private int owner_id;
    [SerializeField] private List<Building> buildings;
    private ProvinceModifiers modifiers;
    private TerrainType terrain;
    private List<Status> statuses;

    public Province(string id, string name, int x, int y, string type, TerrainType terrain, string resources, 
        float resources_amount, int population, int recruitable_population, int happiness, bool is_coast, int owner_id) {
        this.id = id;
        this.name = name;
        this.x = x;
        this.y = y;
        this.type = type;
        this.terrain = terrain;
        this.resources = resources;
        this.resources_amount = resources_amount;
        this.population = population;
        this.recruitable_population = recruitable_population;
        this.happiness = happiness;
        this.is_coast = is_coast;
        this.owner_id = owner_id;

        occupationInfo = new OccupationInfo();
        buildings = new List<Building>();
        statuses = new List<Status>();
        modifiers = new ProvinceModifiers();
    }

    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public string Type { get => type; set => type = value; }
    public string Resources { get => resources; set => resources = value; }
    public float Resources_amount { get => (float)System.Math.Round(resources_amount, 1); set => resources_amount = value; }
    public int Population { get => population; set => population = value; }
    public int RecruitablePopulation { get => recruitable_population; set => recruitable_population = value; }
    public int Happiness { get => happiness; set => happiness = value; }
    public bool Is_coast { get => is_coast; set => is_coast = value; }
    public OccupationInfo OccupationInfo{ get => occupationInfo; set => occupationInfo = value; }
    public int Owner_id { get => owner_id; set => owner_id = value; }
    public List<Building> Buildings { get => buildings; set => buildings = value; }
    public (int, int) coordinates { get => (x, y); }
    public Resource ResourcesT { get => RealResource(); }
    public float ResourcesP { get => RealProduction(); }
    public List<Status> Statuses { get => statuses; set => statuses = value; }
    internal TerrainType Terrain { get => terrain; set => terrain = value; }
    public ProvinceModifiers Modifiers { get => modifiers; set => modifiers = value; }

    private Resource RealResource() {
        switch(Resources) {
            case "iron": return Resource.Iron;
            case "wood": return Resource.Wood;
            case "gold": return Resource.Gold;
            default: return Resource.AP;
        }
    }

    public void UpgradeBuilding(BuildingType buildingType) {
        var building = getBuilding(buildingType);
        building?.Upgrade();
    }

    public void DowngradeBuilding(BuildingType buildingType) {
        var building = getBuilding(buildingType);
        building?.Downgrade();
    }

    public void calcStatuses() {
        modifiers.ResetModifiers();

        if (statuses!= null) {
            List<Status> to_rmv = new();
            statuses.OrderByDescending(s => s.Type).ToList();

            foreach (var status in statuses) {
                if (0 != status.Duration--)
                    status.applyEffect(this);
                else to_rmv.Add(status);
            }
            statuses = statuses.Except(to_rmv).ToList();
        }
    }

    public Building getBuilding(BuildingType type) {
        return buildings?.Find(b => b.BuildingType == type);
    }

    public void addStatus(Status status) { 
        statuses.Add(status);
    }

    public void RemoveStatus(Status status) {
        statuses.Remove(status);
    }

    private float RealProduction() {
        float prod;

        if (ResourcesT != Resource.AP) {
            prod = resources_amount
            * modifiers.ProdMod
            * (0.75f + happiness * 0.01f / 2) 
            * (1 + population / 1000) 
            * (1 + 0.5f * getBuilding(BuildingType.Infrastructure).BuildingLevel);

            switch(ResourcesT) {
                case Resource.Gold:
                    prod *= (1 + 0.25f * getBuilding(BuildingType.Mine).BuildingLevel); break;
                case Resource.Iron:
                    prod *= (1 + 0.75f * getBuilding(BuildingType.Mine).BuildingLevel); break;
                default: break;
            }
        }
        else {
            prod = 0.2f + resources_amount * population / 1000;
        }

        return (float)Math.Round(prod, 1);
    }

    public static List<Building> defaultBuildings(Province p) {
        return new List<Building> {
            new(BuildingType.Infrastructure, 0),
            new(BuildingType.Fort, 0),
            new(BuildingType.School, p.Population > 3000 ? 0 : 4),
            new(BuildingType.Mine, p.Resources == "iron" ? 0 : p.Resources == "gold" ? 0 : 4)
        };
	}
}
