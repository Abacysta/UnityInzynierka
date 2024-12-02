using Assets.classes.subclasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.classes.subclasses.Constants.Province;

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
    [SerializeField] private Resource resourceType;
    [SerializeField] private float resourceAmount;
    [SerializeField] private int population;
    [SerializeField] private int recruitable_population;
    [SerializeField] private int happiness;
    [SerializeField] private bool is_coast;
    [SerializeField] private OccupationInfo occupationInfo;
    [SerializeField] private int owner_id;
    [SerializeField] private Dictionary<BuildingType, int> buildings;
    private ProvinceModifiers modifiers;
    private TerrainType terrain;
    private List<Status> statuses;

    [JsonConstructor]
    public Province(string name, int x, int y, string type, TerrainType terrain, Resource resourceType,
    float resourceAmount, int population,int happiness, bool is_coast)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        this.type = type;
        this.terrain = terrain;
        this.resourceType = resourceType;
        this.resourceAmount = resourceAmount;
        this.population = population;
        this.happiness = happiness;
        this.is_coast = is_coast;

        if (type == "land")
        {
            occupationInfo = new OccupationInfo();
            buildings = defaultBuildings(this);
            modifiers = new ProvinceModifiers();
            statuses = new List<Status>();
            recruitable_population = 0;
            owner_id = 0;
        }
    }
    public Province( string name, int x, int y, string type, TerrainType terrain, Resource resourceType, 
        float resourceAmount, int population, int recruitable_population, int happiness, bool is_coast, int owner_id) {
        this.name = name;
        this.x = x;
        this.y = y;
        this.type = type;
        this.terrain = terrain;
        this.resourceType = resourceType;
        this.resourceAmount = resourceAmount;
        this.population = population;
        this.recruitable_population = recruitable_population;
        this.happiness = happiness;
        this.is_coast = is_coast;
        this.owner_id = owner_id;

        if (type == "land")
        {
            occupationInfo = new OccupationInfo();
            buildings = defaultBuildings(this);
            modifiers = new ProvinceModifiers();
            statuses = new List<Status>();
        }
    }

    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public string Type { get => type; set => type = value; }
    public Resource ResourceType { get => resourceType; set => resourceType = value; }
    public float ResourceAmount { get => (float)System.Math.Round(resourceAmount, 1); set => resourceAmount = value; }
    public int Population { get => population; set => population = value;}
    public int RecruitablePopulation { get => recruitable_population; set => recruitable_population = value; }
    public int Happiness { get => happiness; set => happiness = Mathf.Clamp(value, MIN_HAPP, MAX_HAPP); }
    public bool Is_coast { get => is_coast; set => is_coast = value; }
    public OccupationInfo OccupationInfo{ get => occupationInfo; set => occupationInfo = value; }
    public int Owner_id { get => owner_id; set => owner_id = value; }
    public Dictionary<BuildingType,int> Buildings { get => buildings; set => buildings = value; }
    public (int, int) coordinates { get => (x, y); }
    public float ResourcesP { get => RealProduction(); }
    public List<Status> Statuses { get => statuses; set => statuses = value; }
    internal TerrainType Terrain { get => terrain; set => terrain = value; }
    public ProvinceModifiers Modifiers { get => modifiers; set => modifiers = value; }


    public void UpgradeBuilding(BuildingType buildingType)
    {
        if (buildings.ContainsKey(buildingType) && buildings[buildingType] < 3)
        {
            buildings[buildingType]++;
        }
    }

    public void DowngradeBuilding(BuildingType buildingType)
    {
        if (buildings.ContainsKey(buildingType) && buildings[buildingType] > 0 && buildings[buildingType] < 4)
        {
            buildings[buildingType]--;
        }
    }

    public int GetBuildingLevel(BuildingType type)
    {
        return buildings.ContainsKey(type) ? buildings[type] : 0;
    }
    public void ResetBuilding(BuildingType buildingType)
    {
        if (buildings.ContainsKey(buildingType))
        {
            buildings[buildingType] = 0;
        }
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
    public void addStatus(Status status) { 
        statuses.Add(status);
    }

    public void RemoveStatus(Status status) {
        statuses.Remove(status);
    }

    private float RealProduction() {
        float prod;

        if (ResourceType != Resource.AP) {
            prod = ResourceAmount
            * modifiers.ProdMod
            * (0.75f + happiness * 0.01f / 2) 
            * (1 + population / 1000) 
            * (1 + 0.5f * GetBuildingLevel(BuildingType.Infrastructure));

            switch(ResourceType) {
                case Resource.Gold:
                    prod *= (1 + 0.25f * GetBuildingLevel(BuildingType.Mine)); break;
                case Resource.Iron:
                    prod *= (1 + 0.75f * GetBuildingLevel(BuildingType.Mine)); break;
                default: break;
            }
        }
        else {
            prod = 0.2f + ResourceAmount * population / 1000;
        }

        return (float)Math.Round(prod, 1);
    }

    public static Dictionary<BuildingType, int> defaultBuildings(Province p)
    {
        return new Dictionary<BuildingType, int>
        {
            { BuildingType.Infrastructure, 0 },
            { BuildingType.Fort, 0 },
            { BuildingType.School, p.Population > SCHOOL_MIN_POP ? 0 : 4 },
            { BuildingType.Mine, p.resourceType == Resource.Iron ? 0 : p.resourceType == Resource.Gold ? 0 : 4 }
        };
    }
}
