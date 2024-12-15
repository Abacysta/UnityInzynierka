using Assets.classes.subclasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.classes.subclasses.Constants.ProvinceConstants;

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
        Tundra,
        Forest,
        Lowlands,
        Desert,
        Ocean
    }

    [SerializeField] private string name;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private bool isLand;
    [SerializeField] private Resource resourceType;
    [SerializeField] private float resourceAmount;
    [SerializeField] private int population;
    [SerializeField] private int recruitablePopulation;
    [SerializeField] private int happiness;
    [SerializeField] private bool isCoast;
    [SerializeField] private OccupationInfo occupationInfo;
    [SerializeField] private int ownerId;
    [SerializeField] private Dictionary<BuildingType, int> buildings;
    [SerializeField] private ProvinceModifiers modifiers;
    [SerializeField] private TerrainType terrain;
    [SerializeField] private List<Status> statuses;

    [JsonConstructor]
    public Province(string name, int x, int y, bool isLand, TerrainType terrain, Resource resourceType,
        float resourceAmount, int population, int happiness, bool isCoast)
    {
        this.name = name;
        this.x = x;
        this.y = y;
        this.isLand = isLand;
        this.terrain = terrain;
        this.resourceType = resourceType;
        this.resourceAmount = resourceAmount;
        this.population = population;
        this.happiness = happiness;
        this.isCoast = isCoast;

        if (isLand)
        {
            occupationInfo = new OccupationInfo();
            buildings = GetDefaultBuildings(this);
            modifiers = new ProvinceModifiers();
            statuses = new List<Status>();
            recruitablePopulation = 0;
            ownerId = 0;
        }
    }
    public Province(string name, int x, int y, bool isLand, TerrainType terrain, Resource resourceType, 
        float resourceAmount, int population, int recruitable_population, int happiness, bool isCoast, int ownerId) {
        this.name = name;
        this.x = x;
        this.y = y;
        this.isLand = isLand;
        this.terrain = terrain;
        this.resourceType = resourceType;
        this.resourceAmount = resourceAmount;
        this.population = population;
        this.recruitablePopulation = recruitable_population;
        this.happiness = happiness;
        this.isCoast = isCoast;
        this.ownerId = ownerId;

        if (isLand)
        {
            occupationInfo = new OccupationInfo();
            buildings = GetDefaultBuildings(this);
            modifiers = new ProvinceModifiers();
            statuses = new List<Status>();
        }
    }

    public string Name { get => name; set => name = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public bool IsLand { get => isLand; set => isLand = value; }
    public Resource ResourceType { get => resourceType; set => resourceType = value; }
    public float ResourceAmount { get => (float)System.Math.Round(resourceAmount, 1); set => resourceAmount = value; }
    public int Population { get => population; set => population = value;}
    public int RecruitablePopulation { get => recruitablePopulation; set => recruitablePopulation = value; }
    public int Happiness { get => happiness; set => happiness = Mathf.Clamp(value, MIN_HAPP, MAX_HAPP); }
    public bool IsCoast { get => isCoast; set => isCoast = value; }
    public OccupationInfo OccupationInfo{ get => occupationInfo; set => occupationInfo = value; }
    public int OwnerId { get => ownerId; set => ownerId = value; }
    public Dictionary<BuildingType,int> Buildings { get => buildings; set => buildings = value; }
    public (int, int) coordinates { get => (x, y); }
    public float ResourcesP { get => CalculateRealProduction(); }
    public List<Status> Statuses { get => statuses; set => statuses = value; }
    internal TerrainType Terrain { get => terrain; set => terrain = value; }
    public ProvinceModifiers Modifiers { get => modifiers; set => modifiers = value; }


    public void UpgradeBuilding(BuildingType buildingType)
    {
        if (buildings != null && buildings.ContainsKey(buildingType) && buildings[buildingType] < 3)
        {
            buildings[buildingType]++;
        }
    }

    public void DowngradeBuilding(BuildingType buildingType)
    {
        if (buildings != null && buildings.ContainsKey(buildingType) && buildings[buildingType] > 0 && buildings[buildingType] < 4)
        {
            buildings[buildingType]--;
        }
    }

    public int GetBuildingLevel(BuildingType type)
    {
        return buildings != null && buildings.ContainsKey(type) ? buildings[type] : 0;
    }
    public void ResetBuilding(BuildingType buildingType)
    {
        if (buildings != null && buildings.ContainsKey(buildingType))
        {
            buildings[buildingType] = 0;
        }
    }
    public void CalcStatuses() {
        modifiers.ResetModifiers();

        if (statuses != null) {
            List<Status> to_rmv = new();
            statuses.OrderByDescending(s => s.Type).ToList();

            foreach (var status in statuses) {
                if (0 != status.Duration--)
                    status.ApplyEffect(this);
                else to_rmv.Add(status);
            }
            statuses = statuses.Except(to_rmv).ToList();
        }
    }
    public void AddStatus(Status status) {
        statuses?.Add(status);
    }

    public void RemoveStatus(Status status) {
        statuses?.Remove(status);
    }

    private float CalculateRealProduction() {
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

    public static Dictionary<BuildingType, int> GetDefaultBuildings(Province p)
    {
        return new Dictionary<BuildingType, int>
        {
            { BuildingType.Infrastructure, 0 },
            { BuildingType.Fort, 0 },
            { BuildingType.School, p.Population > SCHOOL_MIN_POP ? 0 : 4 },
            { BuildingType.Mine, p.resourceType == Resource.Iron ? 0 : p.resourceType == Resource.Gold ? 0 : 4 }
        };
    }

    public void GrowPopulation(Map map)
    {
        if (!isLand) return;

        var provinceOwnerTechStats = map.Countries[ownerId].TechStats;

        if (occupationInfo != null && occupationInfo.IsOccupied)
        {
            var occupierTechStats = map.Countries[occupationInfo.OccupyingCountryId].TechStats;
            population = (int)Math.Floor(population *
                ((provinceOwnerTechStats.PopGrowth * modifiers.PopMod) - occupierTechStats.OccPenalty));
        }
        else
        {
            population = (int)Math.Floor(population * provinceOwnerTechStats.PopGrowth * modifiers.PopMod);
        }

        population += (int)Math.Floor(modifiers.PopStatic);
    }

    public void GrowHappiness(Map map, int value)
    {
        if (!isLand) return;

        if (occupationInfo.IsOccupied)
        {
            var occupierTechStats = map.Countries[occupationInfo.OccupyingCountryId].TechStats;
            happiness += (int)Math.Floor(value * (modifiers.HappMod - occupierTechStats.OccPenalty));
        }
        else happiness += (int)Math.Floor(value * modifiers.HappMod);

        happiness += (int)Math.Floor(modifiers.HappStatic);
    }

    public void CalcRecruitablePopulation(Map map)
    {
        if (!isLand) return;

        var techStats = map.Countries[ownerId].TechStats;

        recruitablePopulation = (int)Math.Floor(population * techStats.RecPop * modifiers.RecPop);
    }
}
