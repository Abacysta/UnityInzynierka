using Assets.classes;
using Assets.classes.subclasses;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;


public class Country {
    public class TechnologyInterpreter {
        /// <summary>
        /// Able to build boats
        /// </summary>
        public bool canBoats;
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
        /// Penalty to population growth and happiness growth in occupied provinces
        /// </summary>
        public float occPenalty;
        /// <summary>
        /// Multiplier of production in occupied provinces. Always calculated last
        /// </summary>
        public float occProd;
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
        /// Max level of School building
        /// </summary>
        public int lvlSchool;
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


        public TechnologyInterpreter(Dictionary<Technology, int> tech) {
            Calculate(tech);
        }

        public void Calculate(Dictionary<Technology, int> tech) {
            int eco = tech[Technology.Economic],
                mil = tech[Technology.Military],
                adm = tech[Technology.Administrative];

            prodFactor = 1 + eco * 0.05f;
            taxFactor = 1 + adm * 0.01f;
            popGrowth = 1 + adm * 0.03f;
            armyPower = 1 + mil * 0.05f;
            armyUpkeep = 1 + mil * 0.03f;
            armyCost = 1 + mil * 0.05f;
            popGrowth = 0.05f;
            recPop = 0.05f;
            occPenalty = 0.5f;
            occProd = 0;
            occTime = 3;
            lvlMine = 0; lvlFort = 0; lvlSchool = 0; lvlTax = 0; lvlFoW = 2; moveRange = 1;
            waterMoveFactor = 0.5f;
            //economic
            switch(eco) {
                case 1:
                    prodFactor += 0.05f;
                    break;
                case 2:
                    canBoats = true;
                    lvlMine += 1;
                    goto case 1;
                case 3:
                    prodFactor += 0.05f;
                    goto case 2;
                case 4:
                    lvlTax += 1;
                    goto case 3;
                case 5:
                    taxFactor += 0.15f;
                    goto case 4;
                case 6:
                    lvlMine += 1;
                    goto case 5;
                case 7:
                    prodFactor += 0.1f;
                    goto case 6;
                case 8:
                    taxFactor += 0.05f;
                    goto case 7;
                case 9:
                    lvlTax += 1;
                    goto case 8;
                case 10:
                    lvlMine += 1;
                    prodFactor += 0.05f;
                    goto case 9;
                default:
                    break;
            }
            //military
            switch(mil) {
                case 1:
                    armyPower += 0.1f;
                    break;
                case 2:
                    lvlFort += 1;
                    armyCost += 0.05f;
                    goto case 1;
                case 3:
                    armyUpkeep -= 0.03f;
                    occTime -= 1;
                    goto case 2;
                case 4:
                    armyPower += 0.1f;
                    armyCost += 0.1f;
                    goto case 3;
                case 5:
                    lvlFort += 1;
                    armyUpkeep += 0.02f;
                    goto case 4;
                case 6:
                    armyCost -= 0.1f;
                    moveRange += 1;
                    goto case 5;
                case 7:
                    lvlFort += 1;
                    armyUpkeep += 0.02f;
                    goto case 6;
                case 8:
                    recPop += 0.05f;
                    armyCost -= 0.10f;
                    armyUpkeep -= 0.15f;
                    goto case 7;
                case 9:
                    occPenalty -= 0.35f;
                    goto case 8;
                case 10:
                    armyPower += 0.15f;
                    waterMoveFactor += 0.5f;
                    goto case 9;
                default:
                    break;
            }
            //administrative
            switch(adm) {
                case 1:
                    canInfrastructure = true;
                    lvlFoW += 1;
                    break;
                case 2:
                    lvlSchool += 1;
                    goto case 1;
                case 3:
                    canFestival = true;
                    taxFactor += 0.03f;
                    goto case 2;
                case 4:
                    canTaxBreak = true;
                    goto case 3;
                case 5:
                    lvlFoW += 1;
                    goto case 4;
                case 6:
                    occProd += 0.1f;
                    goto case 5;
                case 7:
                    goto case 6;
                case 8:
                    taxFactor += 0.01f;
                    recPop += 0.02f;
                    goto case 7;
                case 9:
                    canRebelSupp = true;
                    occPenalty -= 0.05f;
                    goto case 8;
                case 10:
                    occProd += 0.4f;
                    lvlFoW += 2;
                    goto case 9;
                default:
                    break;
            }
            //pos and neg effect factor is a retarded idea
        }
    }

    [SerializeField] private int id;
    [SerializeField] private string name;
    [SerializeField] private int prio;
    [SerializeField] private (int, int) capital;
    [SerializeField] private Dictionary<Resource, float> resources;
    [SerializeField] private Dictionary<Technology, int> technology;
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
    public Country(int id, string name, (int, int) capital, Color color, int coat, Map map) {
        this.id = id;
        this.name = name;
        this.capital = id == 0 ? (-1, -1) : capital;
        this.color = id == 0 ? Color.white : color;
        this.coat = coat;
        this.resources = new(technicalDefaultResources.defaultValues);
        this.technology = new Dictionary<Technology, int> { { Technology.Economic, 0 }, { Technology.Military, 0 }, { Technology.Administrative, 0 } };
        this.techStats = new TechnologyInterpreter(this.technology);
        this.provinces = new HashSet<Province> { map.getProvince(capital) };
        revealedTiles = new HashSet<(int, int)>();
        this.actions = new(map);
        seenTiles = new HashSet<(int, int)>();
        events = new List<Event_> ();
        this.atWar = false;
        this.opinions = new Dictionary<int, int> { { 0, 0 } };
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
    public Dictionary<int, int> Opinions { get => opinions; set => opinions = value; }
    public bool AtWar { get => atWar; set => atWar = value; }

    public int Coat { get => coat; }

    public Sprite getCoat() {
        int res;
        if (coat == 1 || coat == 2 || coat == 3)
            res = coat;
        else res = 1;
        return UnityEngine.Resources.Load<Sprite>("sprites/coat_" + res);
    }
    public bool assignProvince(Province province) {
        if(province.Owner_id != 0 || province.Owner_id == id) return false;
        provinces.Add(province);
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
    public bool canPay(Dictionary<Resource, float> cost) {
        bool can = true;
        if(cost != null) foreach(var key in cost.Keys) {
                if(resources[key] < cost[key]) {
                    can = false; break;
                }
            }
        return can;
    }
    public void SetArmyVisibilityManager(army_visibility_manager manager)
    {
        this.armyVisibilityManager = manager;
    }
}


