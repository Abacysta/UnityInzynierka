using Assets.classes;
using Assets.classes.subclasses;
using Assets.classes.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Assets.classes.subclasses.Constants.ProvinceConstants;

public class TechnologyInterpreter
{
    /// <summary>
    /// Able to build boats
    /// </summary>
    public bool CanBoat { get; set; }
    /// <summary>
    /// Can start a festival effect for provinces
    /// </summary>
    public bool CanFestival { get; set; }
    /// <summary>
    /// Can start a taxbreak effect for provinces
    /// </summary>
    public bool CanTaxBreak { get; set; }
    /// <summary>
    /// Can start a rebel suppression effect for provinces
    /// </summary>
    public bool CanRebelSupp { get; set; }
    /// <summary>
    /// Can build infrastructure in provinces
    /// </summary>
    public bool CanInfrastructure { get; set; }
    /// <summary>
    /// Production Efficiency 
    /// </summary>
    public float ProdFactor { get; set; }
    /// <summary>
    /// Taxation Efficiency
    /// </summary>
    public float TaxFactor { get; set; }
    /// <summary>
    /// Population growth
    /// </summary>
    public float PopGrowth { get; set; }
    /// <summary>
    /// Army strength multiplier
    /// </summary>
    public float ArmyPower { get; set; }
    /// <summary>
    /// Army upkeep (cost at the beginning of the turn) multiplier
    /// </summary>
    public float ArmyUpkeep { get; set; }
    /// <summary>
    /// Cost of building new military units
    /// </summary>
    public float ArmyCost { get; set; }
    /// <summary>
    /// % of recruitable population
    /// </summary>
    public float RecPop { get; set; }
    /// <summary>
    /// Penalty to population growth, happiness growth and produced resource in occupied provinces
    /// </summary>
    public float OccPenalty { get; set; }
    /// <summary>
    /// Amount of turns needed to change province status from "occupied" to "owned"
    /// </summary>
    public int OccTime { get; set; }
    /// <summary>
    /// Max level of Mine building
    /// </summary>
    public int LvlMine { get; set; }
    /// <summary>
    /// Max level of Fort building
    /// </summary>
    public int LvlFort { get; set; }
    /// <summary>
    /// If levels above I can be built
    /// </summary>
    public bool MoreSchool { get; set; }
    /// <summary>
    /// How many taxation policies were unlocked
    /// </summary>
    public int LvlTax { get; set; }
    /// <summary>
    /// Fog of War level in tiles seen beyond controlled ones
    /// </summary>
    public int LvlFoW { get; set; }
    /// <summary>
    /// Range of movement for land units.
    /// </summary>
    public int MoveRange { get; set; }
    /// <summary>
    /// Factor for calculation water movement range based on moveRange and Waterfactor.
    /// </summary>
    public float WaterMoveFactor { get; set; }

    public static class BaseModifiers
    {
        public const float ProdFactor = 0.05f;
        public const float TaxFactor = 0.01f;
        public const float PopGrowth = 0.03f;
        public const float ArmyPower = 0.05f;
        public const float ArmyUpkeep = 0.03f;
        public const float ArmyCost = 0.05f;
    }

    public static class EconomicModifiers
    {
        public const float ProdFactor1 = 0.05f;
        public const float ProdFactor2 = 0.05f;
        public const float TaxFactor1 = 0.15f;
        public const float ProdFactor3 = 0.1f;
        public const float TaxFactor2 = 0.05f;
        public const float ProdFactor4 = 0.05f;
    }
    public static class MilitaryModifiers
    {
        public const float ArmyPower1 = 0.1f;
        public const float ArmyCost1 = 0.05f;
        public const float ArmyUpkeep1 = -0.03f;
        public const int OccTime = -1;
        public const float ArmyPower2 = 0.1f;
        public const float ArmyCost2 = 0.1f;
        public const float ArmyUpkeep2 = 0.02f;
        public const float ArmyCost3 = -0.1f;
        public const int MoveRange = 1;
        public const float ArmyUpkeep3 = 0.02f;
        public const float RecPop = 0.05f;
        public const float ArmyCost4 = -0.1f;
        public const float ArmyUpkeep4 = -0.15f;
        public const float OccPenalty = -0.35f;
        public const float ArmyPower3 = 0.15f;
        public const float WaterMoveFactor = 0.5f;
    }

    public static class AdministrativeModifiers
    {
        public const float TaxFactor1 = 0.03f;
        public const float TaxFactor2 = 0.01f;
        public const float RecPop = 0.02f;
        public const float OccPenalty = -0.05f;
    }

    public TechnologyInterpreter(Dictionary<Technology, int> tech)
    {
        CalculateModifiers(tech);
    }

    public void CalculateModifiers(Dictionary<Technology, int> tech)
    {
        int eco = tech[Technology.Economic],
            mil = tech[Technology.Military],
            adm = tech[Technology.Administrative];

        ProdFactor = 1 + eco * BaseModifiers.ProdFactor; // 1 + eco * 0.05f
        TaxFactor = 1 + adm * BaseModifiers.TaxFactor; // 1 + adm * 0.01f
        PopGrowth = 1 + adm * BaseModifiers.PopGrowth; // 1 + adm * 0.03f
        ArmyPower = 1 + mil * BaseModifiers.ArmyPower; // 1 + mil * 0.05f
        ArmyUpkeep = 1 + mil * BaseModifiers.ArmyUpkeep; // 1 + mil * 0.03f
        ArmyCost = 1 + mil * BaseModifiers.ArmyCost; // 1 + mil * 0.05f

        PopGrowth += 0.02f;
        RecPop = 0.05f;
        OccPenalty = 0.5f;
        OccTime = 3;
        MoreSchool = false;
        LvlMine = 0; LvlFort = 0; LvlTax = 0; LvlFoW = 2; MoveRange = 1;
        WaterMoveFactor = 0.5f;

        //economic
        switch (eco)
        {
            case 1:
                ProdFactor += EconomicModifiers.ProdFactor1; // 0.05f
                break;
            case 2:
                CanBoat = true;
                LvlMine += 1; // Mine I
                goto case 1;
            case 3:
                ProdFactor += EconomicModifiers.ProdFactor2; // 0.05f
                goto case 2;
            case 4:
                LvlTax += 1; // Tax IV
                goto case 3;
            case 5:
                TaxFactor += EconomicModifiers.TaxFactor1; // 0.15f
                goto case 4;
            case 6:
                LvlMine += 1; // Mine II
                goto case 5;
            case 7:
                ProdFactor += EconomicModifiers.ProdFactor3; // 0.1f
                goto case 6;
            case 8:
                TaxFactor += EconomicModifiers.TaxFactor2; // 0.05f
                goto case 7;
            case 9:
                LvlTax += 1; // Tax V
                goto case 8;
            case 10:
                LvlMine += 1; // Mine III
                ProdFactor += EconomicModifiers.ProdFactor4; // 0.05f
                goto case 9;
            default:
                break;
        }

        //military
        switch (mil)
        {
            case 1:
                ArmyPower += MilitaryModifiers.ArmyPower1; // 0.1f
                break;
            case 2:
                LvlFort += 1; // Fort I
                ArmyCost += MilitaryModifiers.ArmyCost1; // 0.05f
                goto case 1;
            case 3:
                ArmyUpkeep += MilitaryModifiers.ArmyUpkeep1; // -0.03f
                OccTime += MilitaryModifiers.OccTime; // -1
                goto case 2;
            case 4:
                ArmyPower += MilitaryModifiers.ArmyPower2; // 0.1f
                ArmyCost += MilitaryModifiers.ArmyCost2; // 0.1f
                goto case 3;
            case 5:
                LvlFort += 1; // Fort II
                ArmyUpkeep += MilitaryModifiers.ArmyUpkeep2; // 0.02f
                goto case 4;
            case 6:
                ArmyCost += MilitaryModifiers.ArmyCost3; // -0.1f
                MoveRange += MilitaryModifiers.MoveRange; // 1
                goto case 5;
            case 7:
                LvlFort += 1; // Fort III
                ArmyUpkeep += MilitaryModifiers.ArmyUpkeep3; // 0.02f
                goto case 6;
            case 8:
                RecPop += MilitaryModifiers.RecPop; // 0.05f
                ArmyCost += MilitaryModifiers.ArmyCost4; // -0.1f
                ArmyUpkeep += MilitaryModifiers.ArmyUpkeep4; // -0.15f
                goto case 7;
            case 9:
                OccPenalty += MilitaryModifiers.OccPenalty; // -0.35f
                goto case 8;
            case 10:
                ArmyPower += MilitaryModifiers.ArmyPower3; // 0.15f
                WaterMoveFactor += MilitaryModifiers.WaterMoveFactor; // 0.5f
                goto case 9;
            default:
                break;
        }

        //administrative
        switch (adm)
        {
            case 1:
                CanInfrastructure = true;
                LvlFoW += 1; // Fog I
                break;
            case 2:
                MoreSchool = true; // School II, III
                goto case 1;
            case 3:
                CanFestival = true;
                TaxFactor += AdministrativeModifiers.TaxFactor1; // 0.03f
                goto case 2;
            case 4:
                CanTaxBreak = true;
                goto case 3;
            case 5:
                LvlFoW += 1; // Fog II
                goto case 4;
            case 6:
                goto case 5;
            case 7:
                goto case 6;
            case 8:
                TaxFactor += AdministrativeModifiers.TaxFactor2; // 0.01f
                RecPop += AdministrativeModifiers.RecPop; // 0.02f
                goto case 7;
            case 9:
                CanRebelSupp = true;
                OccPenalty += AdministrativeModifiers.OccPenalty; // -0.05f
                goto case 8;
            case 10:
                LvlFoW += 2; // Fog III
                goto case 9;
            default:
                break;
        }
    }
}

//[Serializable]
public class Country 
{
    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int priority;
    [SerializeField] private (int, int) capital;
    [SerializeField] private Dictionary<Resource, float> resources;
    [SerializeField] private Dictionary<Technology, int> technologies;
    [SerializeField] private HashSet<Province> provinces;
    [SerializeField] private Color color;

    private ActionContainer actions;
    private int coatOfArms;
    private HashSet<(int, int)> revealedTiles;
    private HashSet<(int, int)> seenTiles;
    private List<Event_> events;
    private Dictionary<int, int> opinions;
    private bool atWar;
    private army_visibility_manager armyVisibilityManager;
    private ATax tax;

    /// <summary>
    /// Container for all stats modified by technology
    /// </summary>
    private TechnologyInterpreter techStats;

    public Country(int id, string name, (int, int) capital, Color color, int coat, Map map) {
        this.id = id;
        this.name = name;
        this.capital = id == 0 ? DEFAULT_CORD : capital;
        this.color = id == 0 ? new Color(0.8392f, 0.7216f, 0.4706f) : color;
        coatOfArms = coat;
        resources = new(TechnicalDefaultResources.defaultValues);
        technologies = new Dictionary<Technology, int> { 
            { Technology.Economic, 0 }, 
            { Technology.Military, 0 }, 
            { Technology.Administrative, 0 } 
        };
        techStats = new TechnologyInterpreter(technologies);
        provinces = new HashSet<Province> { map.GetProvince(capital) };
        revealedTiles = new HashSet<(int, int)>();
        actions = new(map);
        seenTiles = new HashSet<(int, int)>();
        events = new List<Event_> ();
        atWar = false;
        opinions = new Dictionary<int, int> { { 0, 0 } };
        tax = new MediumTaxes();
    }

    public int Id { get { return id; } }
    public string Name { get { return name; } } 
    public int Priority { get { return priority; } set => priority = value; }
    public Color Color { get { return color; } }
    public Dictionary<Resource, float> Resources { get { return resources; } }
    public HashSet<Province> Provinces { get { return provinces; } set => provinces = value; }
    public (int, int) Capital {  get { return capital; } }
    public HashSet<(int,int)> RevealedTiles { get {  return revealedTiles; } }
    public HashSet<(int, int)> SeenTiles { get { return seenTiles;  } }
    public ActionContainer Actions { get { return actions; } }
    public List<Event_> Events { get => events; set => events = value; }
    public Dictionary<int, int> Opinions { get => opinions; set => opinions = value; }
    public bool AtWar { get => atWar; set => atWar = value; }
    public int Coat { get => coatOfArms; }
    public Dictionary<Technology, int> Technologies { get => technologies; set => technologies = value; }
    public ATax Tax { get => tax; set => tax = value; }
    public TechnologyInterpreter TechStats { get => techStats; set => techStats = value; }

    public void AddProvince(Province province)
    {
        provinces.Add(province);
    }

    public void RemoveProvince((int, int) coordinates)
    {
        RemoveProvince(provinces.First(p => p.coordinates == coordinates));
    }

    public void RemoveProvince(Province province)
    {
        if (province.coordinates != capital) provinces.Remove(province);
        else
        {
            provinces.Remove(province);
            if (provinces.Count != 0)
            {
                capital = provinces.ToList().OrderByDescending(p => p.Population).First().coordinates;
            }
            else capital = DEFAULT_CORD;
        }
    }

    public Sprite GetCoat() {
        int res;
        if (coatOfArms == 1 || coatOfArms == 2 || coatOfArms == 3)
            res = coatOfArms;
        else res = 1;
        return UnityEngine.Resources.Load<Sprite>("sprites/coat_" + res);
    }

    public void SetCoatandColor(Image image) {
        image.sprite = GetCoat();
        image.color = this.color;
    }

    public void SetCoatandColor(GameObject obj) {
        SetCoatandColor(obj.GetComponent<Image>());
    }

    public bool AssignProvince(Province province) {
        if (!province.IsLand || province.OwnerId != 0 || province.OwnerId == id) return false;
        provinces.Add(province);
        if (province.OwnerId == 0) province.RemoveStatus(province.Statuses.Find(s => s is Tribal));
        province.OwnerId = this.id;

        return true;
    }
    public void UnassignProvince(Province province) {
        province.OwnerId = 0;
        provinces.Remove(province);
    }

    public void ModifyResource((Resource, float) values) {
        resources[values.Item1] += values.Item2;
    }

    public void ModifyResource(Resource resource, float value) {
        ModifyResource((resource, value));
    }

    public void ModifyResources(Dictionary<Resource, float> values) {
        ModifyResources(values, true);
    }

    public void ModifyResources(Dictionary<Resource, float> values, bool mode) {
        if(values != null) foreach(var kvp in values) {
            Resources[kvp.Key] += mode ? kvp.Value : -kvp.Value;
        }
    }

    public void SetResource((Resource, float) values) {
        resources[values.Item1] = values.Item2;
    }

    public void SetResource(Resource resource, float value) { 
        SetResource((resource, value));    
    }

    public void NullifyResources() {
        resources = null;
    }

    public void NullifyActions() {
        actions = null;
    }

    public void ChangeCapital(Province province) {
        if (provinces.Contains(province)) {
            capital = province.coordinates;
        }
    }

    public void RevealTile((int,int) coordinates)
    {
        revealedTiles.Add(coordinates);
    }

    public bool IsTileRevealed((int,int) coordinates)
    {
        return revealedTiles.Contains(coordinates);
    }

    public void ClearRevealedTiles()
    {
        revealedTiles.Clear();
    }

    public int CalculateMaxArmyUnits(Dictionary<Resource, float> cost, int armyCount)
    {
        if (resources[Resource.AP] < cost[Resource.AP]) 
        {
            return 0;
        }

        int maxUnits = armyCount;

        foreach (var resourceCost in cost)
        {
            if (resourceCost.Key == Resource.AP)
                continue;

            if (resources.TryGetValue(resourceCost.Key, out float availableResource))
            {
                int possibleUnits = (int)(availableResource / resourceCost.Value);
                maxUnits = Math.Min(maxUnits, possibleUnits);
            }
        }
        return maxUnits;
    }

    public void SetArmyVisibilityManager(army_visibility_manager manager)
    {
        this.armyVisibilityManager = manager;
    }

    public bool CanAfford(Dictionary<Resource, float> cost) {
        bool payFlag = true;
        foreach (var c in cost) { 
            if(c.Value > resources[c.Key])
                payFlag = false;
            if (!payFlag)
                break;
        }
        return payFlag;
    }

    public bool CanAfford(Resource type, float amount) {
        return resources[type] >= amount;
    }

    public Province GetCapital() {
        return provinces.First(p=>p.coordinates == capital);
    }

    public void SetOpinion(int countryId, int value) {
        opinions[countryId] = Mathf.Clamp(value, MIN_OPINION, MAX_OPINION);
    }

    public Dictionary<Resource, float> GetResourcesGain(Map map)
    {
        var prod = new Dictionary<Resource, float> {
            { Resource.Gold, 0 },
            { Resource.Wood, 0 },
            { Resource.Iron, 0 },
            { Resource.SciencePoint, 0 },
            { Resource.AP, 0 }
        };

        foreach (var prov in provinces)
        {
            prod[prov.ResourceType] += prov.ResourcesP;
            prod[Resource.AP] += 0.1f;

            if (prov.Buildings.ContainsKey(BuildingType.School) &&
                prov.GetBuildingLevel(BuildingType.School) < 4)
            {
                prod[Resource.SciencePoint] += prov.GetBuildingLevel(BuildingType.School) * 3;
            }
        }

        foreach (var type in prod.ToList())
        {
            if (type.Key != Resource.AP)
                prod[type.Key] *= TechStats.ProdFactor;
        }

        foreach (var army in map.GetCountryArmies(map.CurrentPlayer))
        {
            prod[Resource.Gold] -= (army.Count / 10 + 1) * TechStats.ArmyUpkeep;
        }

        foreach (var type in prod.ToList())
        {
            prod[type.Key] = (float)Math.Round(prod[type.Key], 1);
        }

        prod[Resource.AP] += 2.5f;

        return prod;
    }
}


