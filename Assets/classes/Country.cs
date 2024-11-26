using Assets.classes;
using Assets.classes.subclasses;
using Assets.classes.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//[Serializable]
public class Country {
    public class TechnologyInterpreter {
        /// <summary>
        /// Able to build boats
        /// </summary>
        public bool canBoat;
        /// <summary>
        /// Can start a festival effect for provinces
        /// </summary>
        public bool canFestival;
        /// <summary>
        /// Can start a taxbreak effect for provinces
        /// </summary>
        public bool canTaxBreak;
        /// <summary>
        /// Can start a rebel suppression effect for provinces
        /// </summary>
        public bool canRebelSupp;
        /// <summary>
        /// Can build infrastructure in provinces
        /// </summary>
        public bool canInfrastructure;
        /// <summary>
        /// Production Efficiency 
        /// </summary>
        public float prodFactor;
        /// <summary>
        /// Taxation Efficiency
        /// </summary>
        public float taxFactor;
        /// <summary>
        /// Population growth
        /// </summary>
        public float popGrowth;
        /// <summary>
        /// Army strength multiplier
        /// </summary>
        public float armyPower;
        /// <summary>
        /// Army upkeep (cost at the beginning of the turn) multiplier
        /// </summary>
        public float armyUpkeep;
        /// <summary>
        /// Cost of building new military units
        /// </summary>
        public float armyCost;
        /// <summary>
        /// % of recruitable population
        /// </summary>
        public float recPop;
        /// <summary>
        /// Penalty to population growth, happiness growth and produced resource in occupied provinces
        /// </summary>
        public float occPenalty;
        /// <summary>
        /// Amount of turns needed to change province status from "occupied" to "owned"
        /// </summary>
        public int occTime;
        /// <summary>
        /// Max level of Mine building
        /// </summary>
        public int lvlMine;
        /// <summary>
        /// Max level of Fort building
        /// </summary>
        public int lvlFort;
        /// <summary>
        /// If levels above I can be built
        /// </summary>
        public bool moreSchool;
        /// <summary>
        /// How many taxation policies were unlocked
        /// </summary>
        public int lvlTax;
        /// <summary>
        /// Fog of War level in tiles seen beyond controlled ones
        /// </summary>
        public int lvlFoW;
        /// <summary>
        /// Range of movement for land units.
        /// </summary>
        public int moveRange;
        /// <summary>
        /// Factor for calculation water movement range based on moveRange and Waterfactor.
        /// </summary>
        public float waterMoveFactor;

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

        public TechnologyInterpreter(Dictionary<Technology, int> tech) {
            Calculate(tech);
        }

        public void Calculate(Dictionary<Technology, int> tech) {
            int eco = tech[Technology.Economic],
                mil = tech[Technology.Military],
                adm = tech[Technology.Administrative];

            prodFactor = 1 + eco * BaseModifiers.ProdFactor; // 1 + eco * 0.05f
            taxFactor = 1 + adm * BaseModifiers.TaxFactor; // 1 + adm * 0.01f
            popGrowth = 1 + adm * BaseModifiers.PopGrowth; // 1 + adm * 0.03f
            armyPower = 1 + mil * BaseModifiers.ArmyPower; // 1 + mil * 0.05f
            armyUpkeep = 1 + mil * BaseModifiers.ArmyUpkeep; // 1 + mil * 0.03f
            armyCost = 1 + mil * BaseModifiers.ArmyCost; // 1 + mil * 0.05f

            popGrowth += 0.05f;
            recPop = 0.05f;
            occPenalty = 0.5f;
            occTime = 3;
            moreSchool = false;
            lvlMine = 0; lvlFort = 0; lvlTax = 0; lvlFoW = 2; moveRange = 1;
            waterMoveFactor = 0.5f;

            //economic
            switch (eco)
            {
                case 1:
                    prodFactor += EconomicModifiers.ProdFactor1; // 0.05f
                    break;
                case 2:
                    canBoat = true;
                    lvlMine += 1; // Mine I
                    goto case 1;
                case 3:
                    prodFactor += EconomicModifiers.ProdFactor2; // 0.05f
                    goto case 2;
                case 4:
                    lvlTax += 1; // Tax IV
                    goto case 3;
                case 5:
                    taxFactor += EconomicModifiers.TaxFactor1; // 0.15f
                    goto case 4;
                case 6:
                    lvlMine += 1; // Mine II
                    goto case 5;
                case 7:
                    prodFactor += EconomicModifiers.ProdFactor3; // 0.1f
                    goto case 6;
                case 8:
                    taxFactor += EconomicModifiers.TaxFactor2; // 0.05f
                    goto case 7;
                case 9:
                    lvlTax += 1; // Tax V
                    goto case 8;
                case 10:
                    lvlMine +=1; // Mine III
                    prodFactor += EconomicModifiers.ProdFactor4; // 0.05f
                    goto case 9;
                default:
                    break;
            }

            //military
            switch (mil) {
                case 1:
                    armyPower += MilitaryModifiers.ArmyPower1; // 0.1f
                    break;
                case 2:
                    lvlFort += 1; // Fort I
                    armyCost += MilitaryModifiers.ArmyCost1; // 0.05f
                    goto case 1;
                case 3:
                    armyUpkeep += MilitaryModifiers.ArmyUpkeep1; // -0.03f
                    occTime += MilitaryModifiers.OccTime; // -1
                    goto case 2;
                case 4:
                    armyPower += MilitaryModifiers.ArmyPower2; // 0.1f
                    armyCost += MilitaryModifiers.ArmyCost2; // 0.1f
                    goto case 3;
                case 5:
                    lvlFort += 1; // Fort II
                    armyUpkeep += MilitaryModifiers.ArmyUpkeep2; // 0.02f
                    goto case 4;
                case 6:
                    armyCost += MilitaryModifiers.ArmyCost3; // -0.1f
                    moveRange += MilitaryModifiers.MoveRange; // 1
                    goto case 5;
                case 7:
                    lvlFort += 1; // Fort III
                    armyUpkeep += MilitaryModifiers.ArmyUpkeep3; // 0.02f
                    goto case 6;
                case 8:
                    recPop += MilitaryModifiers.RecPop; // 0.05f
                    armyCost += MilitaryModifiers.ArmyCost4; // -0.1f
                    armyUpkeep += MilitaryModifiers.ArmyUpkeep4; // -0.15f
                    goto case 7;
                case 9:
                    occPenalty += MilitaryModifiers.OccPenalty; // -0.35f
                    goto case 8;
                case 10:
                    armyPower += MilitaryModifiers.ArmyPower3; // 0.15f
                    waterMoveFactor += MilitaryModifiers.WaterMoveFactor; // 0.5f
                    goto case 9;
                default:
                    break;
            }

            //administrative
            switch (adm) {
                case 1:
                    canInfrastructure = true;
                    lvlFoW += 1; // Fog I
                    break;
                case 2:
                    moreSchool = true; // School II, III
                    goto case 1;
                case 3:
                    canFestival = true;
                    taxFactor += AdministrativeModifiers.TaxFactor1; // 0.03f
                    goto case 2;
                case 4:
                    canTaxBreak = true;
                    goto case 3;
                case 5:
                    lvlFoW += 1; // Fog II
                    goto case 4;
                case 6:
                    goto case 5;
                case 7:
                    goto case 6;
                case 8:
                    taxFactor += AdministrativeModifiers.TaxFactor2; // 0.01f
                    recPop += AdministrativeModifiers.RecPop; // 0.02f
                    goto case 7;
                case 9:
                    canRebelSupp = true;
                    occPenalty += AdministrativeModifiers.OccPenalty; // -0.05f
                    goto case 8;
                case 10:
                    lvlFoW += 2; // Fog III
                    goto case 9;
                default:
                    break;
            }
        }
    }

    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int prio;
    [SerializeField] private (int, int) capital;
    [SerializeField] private Dictionary<Resource, float> resources;
    [SerializeField] private Dictionary<Technology, int> technologies;
    private actionContainer actions;
    /// <summary>
    /// Container for all stats modified by technology
    /// </summary>
    public TechnologyInterpreter techStats;
    [SerializeField] private HashSet<Province> provinces;
    [SerializeField] private Color color;
    private int coat;
    private HashSet<(int, int)> revealedTiles;
    private HashSet<(int, int)> seenTiles;
    private List<Event_> events;
    private Dictionary<int, int> opinions;
    private bool atWar;
    private army_visibility_manager armyVisibilityManager;
    private ITax tax;
    public Country(int id, string name, (int, int) capital, Color color, int coat, Map map) {
        this.id = id;
        this.name = name;
        this.capital = id == 0 ? (-1, -1) : capital;
        this.color = id == 0 ? new Color(0.8392f, 0.7216f, 0.4706f) : color;
        this.coat = coat;
        this.resources = new(technicalDefaultResources.defaultValues);
        this.technologies = new Dictionary<Technology, int> { { Technology.Economic, 0 }, { Technology.Military, 0 }, { Technology.Administrative, 0 } };
        this.techStats = new TechnologyInterpreter(this.technologies);
        this.provinces = new HashSet<Province> { map.getProvince(capital) };
        revealedTiles = new HashSet<(int, int)>();
        this.actions = new(map);
        seenTiles = new HashSet<(int, int)>();
        events = new List<Event_> ();
        this.atWar = false;
        this.opinions = new Dictionary<int, int> { { 0, 0 } };
        this.tax = new MediumTaxes();
    }

    public void addProvince(Province province) {
        provinces.Add(province);
    }
    public void removeProvince((int, int) coordinates) {
        if(coordinates != capital) provinces.RemoveWhere(p => p.coordinates == coordinates);
    }
    public void removeProvince(Province province) {
        if(province.coordinates != capital) provinces.Remove(province);
    }
    public int Id { get { return id; } }
    public string Name { get { return name; } } 
    public int Priority { get { return prio; } set => prio = value; }
    public Color Color { get { return color; } }
    public Dictionary<Resource, float> Resources { get { return resources; } }
    public HashSet<Province> Provinces { get { return provinces; } }
    public (int, int) Capital {  get { return capital; } }
    public HashSet<(int,int)> RevealedTiles { get {  return revealedTiles; } }
    public HashSet<(int, int)> SeenTiles { get { return seenTiles;  } }
    public actionContainer Actions { get { return actions; } }

    public List<Event_> Events { get => events; set => events = value; }
    public Dictionary<int, int> Opinions { 
        get
        {
            var keys = opinions.Keys.ToList();
            foreach (var key in keys)
            {
                opinions[key] = Mathf.Clamp(opinions[key], -200, 200);
            }
            return opinions;
        }
        set => opinions = value;
    }
    public bool AtWar { get => atWar; set => atWar = value; }

    public int Coat { get => coat; }
    public Dictionary<Technology, int> Technologies { get => technologies; set => technologies = value; }
    public ITax Tax { get => tax; set => tax = value; }

    public Sprite getCoat() {
        int res;
        if (coat == 1 || coat == 2 || coat == 3)
            res = coat;
        else res = 1;
        return UnityEngine.Resources.Load<Sprite>("sprites/coat_" + res);
    }

    public void setCoatandColor(Image image) {
        image.sprite = getCoat();
        image.color = this.color;
    }

    public void setCoatandColor(GameObject obj) {
        setCoatandColor(obj.GetComponent<Image>());
    }

    public bool assignProvince(Province province) {
        if(province.Owner_id != 0 || province.Owner_id == id) return false;
        provinces.Add(province);
        if (province.Owner_id == 0) province.RemoveStatus(province.Statuses.Find(s => s is Tribal));
        province.Owner_id = this.id;
        return true;
    }
    public void unassignProvince(Province province) {
        province.Owner_id = 0;
        provinces.Remove(province);
    }

    public void modifyResource((Resource, float) values) {
        //Debug.Log("modified " + values.Item1.ToString() + " by " + values.Item2.ToString() + " for " + this.name);
        this.resources[values.Item1] += values.Item2;
    }

    public void modifyResource(Resource resource, float value) {
        modifyResource((resource, value));
    }

    public void modifyResources(Dictionary<Resource, float> values) {
        modifyResources(values, true);
    }

    public void modifyResources(Dictionary<Resource, float> values, bool mode) {
        if(values != null) foreach(var kvp in values) {
                Resources[kvp.Key] += mode ? kvp.Value : -kvp.Value;
            }
    }

    public void setResource((Resource, float) values) {
        //Debug.Log("set " + values.Item1.ToString() + " by " + values.Item2.ToString() + " for " + this.name);
        this.resources[values.Item1] = values.Item2;
    }

    public void setResource(Resource resource, float value) { 
        setResource((resource, value));    
    }

    public void nullifyResources() {
        resources = null;
    }

    public void nullifyActions() {
        actions = null;
    }
    public void changeCapital(Province province) {
        if(provinces.Contains(province)) {
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
    public bool isPayable(Dictionary<Resource, float> cost) {
        bool payFlag = true;
        foreach (var c in cost) { 
            if(c.Value > resources[c.Key])
                payFlag = false;
            if (!payFlag)
                break;
        }
        return payFlag;
    }
    public bool isPayable(Resource type, float amount) {
        return resources[type] >= amount;
    }
    public Province getCapital() {
        return provinces.First(p=>p.coordinates == capital);
    }
}


