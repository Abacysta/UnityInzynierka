using Assets.classes.subclasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Country.TechnologyInterpreter;
using static Assets.classes.TurnAction;

public class technology_manager : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private GameObject tech_tooltip_row;

    [SerializeField] private dialog_box_manager dialog_box;

    [SerializeField] private TMP_Text mil_tooltip_text;
    [SerializeField] private TMP_Text ec_tooltip_text;
    [SerializeField] private TMP_Text adm_tooltip_text;

    [SerializeField] private GameObject mil_tooltip_container;
    [SerializeField] private GameObject ec_tooltip_container;
    [SerializeField] private GameObject adm_tooltip_container;

    [SerializeField] private Button mil_tech_button;
    [SerializeField] private Button ec_tech_button;
    [SerializeField] private Button adm_tech_button;

    [SerializeField] private TMP_Text mil_current_level_text;
    [SerializeField] private TMP_Text ec_current_level_text;
    [SerializeField] private TMP_Text adm_current_level_text;

    [SerializeField] private TMP_Text mil_next_level_text;
    [SerializeField] private TMP_Text ec_next_level_text;
    [SerializeField] private TMP_Text adm_next_level_text;

    [SerializeField] private GameObject mil_next_level_container;
    [SerializeField] private GameObject adm_next_level_container;
    [SerializeField] private GameObject ec_next_level_container;

    [SerializeField] private TMP_Text mil_ap_value;
    [SerializeField] private TMP_Text mil_sp_value;
    [SerializeField] private TMP_Text ec_ap_value;
    [SerializeField] private TMP_Text ec_sp_value;
    [SerializeField] private TMP_Text adm_ap_value;
    [SerializeField] private TMP_Text adm_sp_value;

    [SerializeField] private GameObject mil_cost_content;
    [SerializeField] private GameObject ec_cost_content;
    [SerializeField] private GameObject adm_cost_content;

    [SerializeField] private Sprite army_combat_power_sprite;
    [SerializeField] private Sprite army_cost_sprite;
    [SerializeField] private Sprite army_upkeep_cost_sprite;
    [SerializeField] private Sprite army_move_range_sprite;
    [SerializeField] private Sprite recruitable_population_sprite;
    [SerializeField] private Sprite occupation_penalty_sprite;
    [SerializeField] private Sprite occupation_time_sprite;
    [SerializeField] private Sprite water_move_factor_sprite;

    [SerializeField] private Sprite production_factor_sprite;
    [SerializeField] private Sprite tax_revenue_sprite;
    [SerializeField] private Sprite can_boat_sprite;
    [SerializeField] private Sprite choosing_new_tax_law_sprite;
    [SerializeField] private Sprite fog_of_war_sprite;

    [SerializeField] private List<Sprite> building_the_fort_sprite;
    [SerializeField] private List<Sprite> building_the_mine_sprite;
    [SerializeField] private List<Sprite> building_the_infrastructure_sprite;
    [SerializeField] private List<Sprite> building_the_school_sprite;

    [SerializeField] private Sprite holding_the_festival_sprite;
    [SerializeField] private Sprite introducing_the_tax_break_sprite;
    [SerializeField] private Sprite occupation_production_factor_sprite;
    [SerializeField] private Sprite supressing_the_rebelion_sprite;
    [SerializeField] private Sprite population_growth_sprite;

    public class TechEffect
    {
        public string Name { get; set; }
        public float? NumericValue { get; set; } 
        public int? IntValue { get; set; }
        public bool? BoolValue { get; set; }
        public Sprite Icon { get; set; }
        public bool IsEffectPositive { get; set; }

        public TechEffect(string name, float value, Sprite icon, bool isEffectPositive)
        {
            Name = name;
            NumericValue = value;
            IntValue = null;
            BoolValue = null;
            Icon = icon;
            IsEffectPositive = isEffectPositive;
        }

        public TechEffect(string name, int value, Sprite icon, bool isEffectPositive)
        {
            Name = name;
            NumericValue = null;
            IntValue = value;
            BoolValue = null;
            Icon = icon;
            IsEffectPositive = isEffectPositive;
        }

        public TechEffect(string name, bool value, Sprite icon, bool isEffectPositive)
        {
            Name = name;
            NumericValue = null;
            IntValue = null;
            BoolValue = value;
            Icon = icon;
            IsEffectPositive = isEffectPositive;
        }

        public string GetFormattedValue()
        {
            if (Name.StartsWith("Building"))
            {
                return "YES";
            }
            else if (NumericValue.HasValue)
            {
                float percentageValue = NumericValue.Value * 100;
                string sign = percentageValue >= 0 ? "+" : "";
                return sign + percentageValue.ToString("0.##") + "%";
            }
            else if (IntValue.HasValue)
            {
                string sign = IntValue >= 0 ? "+" : "";
                return sign + IntValue.Value.ToString();
            }
            else if (BoolValue.HasValue)
            {
                return BoolValue.Value ? "YES" : "NO";
            }
            else
            {
                return "N/A";
            }
        }
    }

    public class TechLevel
    {
        private List<TechEffect> effects;

        public TechLevel(List<TechEffect> effects)
        {
            this.effects = effects;
        }

        public List<TechEffect> Effects
        {
            get { return effects; }
            set { effects = value; }
        }
    }

    private List<TechLevel> militaryTree;
    private List<TechLevel> economicTree;
    private List<TechLevel> administrativeTree;

    private List<TechEffect> milBaseEffects;
    private List<TechEffect> ecBaseEffects;
    private List<TechEffect> admBaseEffects;

    void Awake()
    {
        InitializeLevels();
    }

    void Start()
    {
        mil_tech_button.onClick.AddListener(() =>
        {
            dialog_box.invokeTechUpgradeBox(Technology.Military);
        });

        ec_tech_button.onClick.AddListener(() =>
        {
            dialog_box.invokeTechUpgradeBox(Technology.Economic);
        });

        adm_tech_button.onClick.AddListener(() =>
        {
            dialog_box.invokeTechUpgradeBox(Technology.Administrative);
        });
    }

    void OnEnable()
    {
        UpdateData();
    }

    public void UpdateData()
    {
        int militaryLevel = map.CurrentPlayer.Technology_[Technology.Military];
        int economicLevel = map.CurrentPlayer.Technology_[Technology.Economic];
        int administrativeLevel = map.CurrentPlayer.Technology_[Technology.Administrative];

        SetTechnologyData(mil_tooltip_container, mil_tooltip_text, militaryLevel, militaryTree,
            mil_next_level_container, mil_tech_button, mil_current_level_text, mil_next_level_text, 
            mil_ap_value, mil_sp_value, mil_cost_content, Technology.Military);
        SetTechnologyData(ec_tooltip_container, ec_tooltip_text, economicLevel, economicTree,
            ec_next_level_container, ec_tech_button, ec_current_level_text, ec_next_level_text, 
            ec_ap_value, ec_sp_value, ec_cost_content, Technology.Economic);
        SetTechnologyData(adm_tooltip_container, adm_tooltip_text, administrativeLevel, administrativeTree,
            adm_next_level_container, adm_tech_button, adm_current_level_text, adm_next_level_text, 
            adm_ap_value, adm_sp_value, adm_cost_content, Technology.Administrative);
    }

    public void SetTechnologyData(GameObject tooltip, TMP_Text tooltipText, int level, List<TechLevel> techTree, 
        GameObject nextLevelContainer, Button techButton, TMP_Text currentLevelText, TMP_Text nextLevelText, 
        TMP_Text ap_value, TMP_Text sp_value, GameObject cost_content, Technology type)
    {
        Dictionary<Resource, float> cost = CostsCalculator.TurnActionFullCost(ActionType.TechnologyUpgrade, 
            tech: map.CurrentPlayer.Technology_, techType: type);

        // Tooltip
        ClearChildren(tooltip.transform);

        if (level == 0)
        {
            tooltipText.text = "No active effects!";
        }
        else
        {
            tooltipText.text = "Current effects:";
            var levelNodes = techTree.Take(level);
            SumEffects(levelNodes, tooltip);
        }

        // Panel
        currentLevelText.text = level.ToString();

        ClearChildren(nextLevelContainer.transform);

        if (level < 10)
        {
            SetButtonColor(techButton, map.CurrentPlayer.isPayable(cost));

            nextLevelText.text = $"Level {level + 1} effects:";

            foreach (var effect in techTree[level].Effects)
            {
                GameObject effectRow = Instantiate(tech_tooltip_row, nextLevelContainer.transform);
                tech_effect_ui effectUI = effectRow.GetComponent<tech_effect_ui>();
                effectUI.SetEffect(effect);
            }
        }
        else
        {
            SetButtonNonInteractable(techButton);
            nextLevelText.text = "Maximum level reached!";
        }

        ap_value.text = cost.ContainsKey(Resource.AP)
            ? ($"-{Math.Round(cost[Resource.AP], 1)}") : "?";
        sp_value.text = cost.ContainsKey(Resource.SciencePoint)
            ? ($"-{Math.Round(cost[Resource.SciencePoint], 1)}") : "?";
        cost_content.SetActive(level < 10);

        StartCoroutine(RefreshUITemporarily(5f));
    }

    private void SetButtonColor(Button techButton, bool isGreen)
    {
        TMP_Text buttonText = techButton.GetComponentInChildren<TMP_Text>();

        if (isGreen)
        {
            techButton.interactable = true;
            techButton.GetComponent<Image>().color = new Color32(35, 82, 29, 255); // green color
            if (buttonText != null)
            {
                buttonText.text = "Upgrade technology";
            }
        }
        else
        {
            techButton.interactable = false;
            techButton.GetComponent<Image>().color = new Color32(118, 32, 23, 255); // red color
            if (buttonText != null)
            {
                buttonText.text = "Not enough funds";
            }
        }
    }

    private void SetButtonNonInteractable(Button techButton)
    {
        techButton.interactable = false;
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    void SumEffects(IEnumerable<TechLevel> levelNodes, GameObject tooltip)
    {
        var consolidatedEffects = new List<TechEffect>();

        var nonBuildingEffects = levelNodes
            .SelectMany(level => level.Effects)
            .Where(b => !b.Name.StartsWith("Building"))
            .GroupBy(b => b.Name)
            .Select(g =>
            {
                bool hasFloat = g.Any(b => b.NumericValue.HasValue);
                bool hasInt = g.Any(b => b.IntValue.HasValue);

                return hasFloat
                    ? new TechEffect(g.Key, g.Sum(b => b.NumericValue ?? 0f), g.Last().Icon, g.All(b => b.IsEffectPositive))
                    : hasInt
                    ? new TechEffect(g.Key, g.Sum(b => b.IntValue ?? 0), g.Last().Icon, g.All(b => b.IsEffectPositive))
                    : new TechEffect(g.Key, g.All(b => b.BoolValue == true), g.Last().Icon, g.All(b => b.IsEffectPositive));
            })
            .ToList();

        var buildingEffects = levelNodes
            .SelectMany(level => level.Effects)
            .Where(b => b.Name.StartsWith("Building"))
            .GroupBy(b => b.Name)
            .Select(g => new TechEffect(
                g.Key,
                g.Max(b => b.IntValue ?? 0),
                g.OrderByDescending(b => b.IntValue).First().Icon,
                true
            ))
        .ToList();

        consolidatedEffects.AddRange(nonBuildingEffects);
        consolidatedEffects.AddRange(buildingEffects);

        foreach (var effect in consolidatedEffects)
        {
            GameObject effectRow = Instantiate(tech_tooltip_row, tooltip.transform);
            tech_effect_ui effectUI = effectRow.GetComponent<tech_effect_ui>();
            effectUI.SetEffect(effect);
        }
    }

    void InitializeLevels()
    {
        milBaseEffects = new List<TechEffect>
        {
            new("Army combat power", BaseModifiers.ArmyPower, army_combat_power_sprite, true),
            new("Army upkeep cost", BaseModifiers.ArmyUpkeep, army_upkeep_cost_sprite, false),
            new("Army cost", BaseModifiers.ArmyCost, army_cost_sprite, false),
        };

        ecBaseEffects = new List<TechEffect>
        {
            new("Production factor", BaseModifiers.ProdFactor, production_factor_sprite, true),
        };

        admBaseEffects = new List<TechEffect>
        {
            new("Tax revenue", BaseModifiers.TaxFactor, tax_revenue_sprite, true),
            new("Population growth", BaseModifiers.PopGrowth, population_growth_sprite, true),
        };

        militaryTree = new List<TechLevel>
        {
            // Level 1
            new(new List<TechEffect> {
                new("Army combat power", MilitaryModifiers.ArmyPower1, army_combat_power_sprite, true)
            }),
            // Level 2
            new(new List<TechEffect> {
                new("Building the fort", 1, building_the_fort_sprite[0], true),
                new("Army cost", MilitaryModifiers.ArmyCost1, army_cost_sprite, false)
            }),
            // Level 3
            new(new List<TechEffect> {
                new("Army upkeep cost", MilitaryModifiers.ArmyUpkeep1, army_upkeep_cost_sprite, false),
                new("Occupation time", MilitaryModifiers.OccTime1, occupation_time_sprite, false)
            }),
            // Level 4
            new(new List<TechEffect> {
                new("Army combat power", MilitaryModifiers.ArmyPower2, army_combat_power_sprite, true),
                new("Army cost", MilitaryModifiers.ArmyCost2, army_cost_sprite, false)
            }),
            // Level 5
            new(new List<TechEffect> {
                new("Building the fort", 2, building_the_fort_sprite[1], true),
                new("Army upkeep cost", MilitaryModifiers.ArmyUpkeep2, army_upkeep_cost_sprite, false)
            }),
            // Level 6
            new(new List<TechEffect> {
                new("Army cost", MilitaryModifiers.ArmyCost3, army_cost_sprite, false),
                new("Army move range", MilitaryModifiers.MoveRange1, army_move_range_sprite, true)
            }),
            // Level 7
            new(new List<TechEffect> {
                new("Building the fort", 3, building_the_fort_sprite[2], true),
                new("Army upkeep cost", MilitaryModifiers.ArmyUpkeep3, army_upkeep_cost_sprite, false)
            }),
            // Level 8
            new(new List<TechEffect> {
                new("Recruitable population", MilitaryModifiers.RecPop1, recruitable_population_sprite, true),
                new("Army cost", MilitaryModifiers.ArmyCost4, army_cost_sprite, false),
                new("Army upkeep cost", MilitaryModifiers.ArmyUpkeep4, army_upkeep_cost_sprite, false),
            }),
            // Level 9
            new(new List<TechEffect> {
                new("Occupation penalty", MilitaryModifiers.OccPenalty1, occupation_penalty_sprite, false)
            }),
            // Level 10
            new(new List<TechEffect> {
                new("Army combat power", MilitaryModifiers.ArmyPower3, army_combat_power_sprite, true),
                new("Water move factor", MilitaryModifiers.WaterMoveFactor1, water_move_factor_sprite, true)
            }),
        };

        economicTree = new List<TechLevel>
        {
            // Level 1
            new(new List<TechEffect> {
                new("Production factor", EconomicModifiers.ProdFactor1, production_factor_sprite, true)
            }),
            // Level 2
            new(new List<TechEffect> {
                new("Can boat", EconomicModifiers.CanBoats, can_boat_sprite, true),
                new("Building the mine", 1, building_the_mine_sprite[0], true)
            }),
            // Level 3
            new(new List<TechEffect> {
                new("Production factor", EconomicModifiers.ProdFactor2, production_factor_sprite, true)
            }),
            // Level 4
            new(new List<TechEffect> {
                new("Choosing tax law I", true, choosing_new_tax_law_sprite, true)
            }),
            // Level 5
            new(new List<TechEffect> {
                new("Tax revenue", EconomicModifiers.TaxFactor1, tax_revenue_sprite, true)
            }),
            // Level 6
            new(new List<TechEffect> {
                new("Building the mine", 2, building_the_mine_sprite[1], true)
            }),
            // Level 7
            new(new List<TechEffect> {
                new("Production factor", EconomicModifiers.ProdFactor3, production_factor_sprite, true)
            }),
            // Level 8
            new(new List<TechEffect> {
                new("Tax revenue", EconomicModifiers.TaxFactor2, tax_revenue_sprite, true)
            }),
            // Level 9
            new(new List<TechEffect> {
                new("Choosing tax law II", true, choosing_new_tax_law_sprite, true)
            }),
            // Level 10
            new(new List<TechEffect> {
                new("Building the mine", 3, building_the_mine_sprite[2], true),
                new("Production factor", EconomicModifiers.ProdFactor4, production_factor_sprite, true)
            }),
        };

        administrativeTree = new List<TechLevel>
        {
            // Level 1
            new(new List<TechEffect> {
                new("Building the infrastructure", AdministrativeModifiers.CanInfrastructure, building_the_infrastructure_sprite[0], true),
                new("Fog of war", 1, fog_of_war_sprite, true)
            }),
            // Level 2
            new(new List<TechEffect> {
                new("Building the school", 1, building_the_school_sprite[0], true)
            }),
            // Level 3
            new(new List<TechEffect> {
                new("Holding the festival", true, holding_the_festival_sprite, true),
                new("Tax revenue", AdministrativeModifiers.TaxFactor1, tax_revenue_sprite, true)
            }),
            // Level 4
            new(new List<TechEffect> {
                new("Introducing the tax break", true, introducing_the_tax_break_sprite, true)
            }),
            // Level 5
            new(new List<TechEffect> {
                new("Building the infrastructure", 2, building_the_infrastructure_sprite[1], true),
                new("Fog of war", 1, fog_of_war_sprite, true)
            }),
            // Level 6
            new(new List<TechEffect> {
                new("Occupation production factor", AdministrativeModifiers.OccProd1, occupation_production_factor_sprite, true)
            }),
            // Level 7
            new(new List<TechEffect> {}),
            // Level 8
            new(new List<TechEffect> {
                new("Tax revenue", AdministrativeModifiers.TaxFactor2, tax_revenue_sprite, true),
                new("Recruitable population", AdministrativeModifiers.RecPop1, recruitable_population_sprite, true)
            }),
            // Level 9
            new(new List<TechEffect> {
                new("Suppressing the rebellion", AdministrativeModifiers.CanRebelSupp, supressing_the_rebelion_sprite, true),
                new("Army cost", AdministrativeModifiers.OccPenalty1, army_cost_sprite, false)
            }),
            // Level 10
            new(new List<TechEffect> {
                new("Occupation production factor", AdministrativeModifiers.OccProd2, occupation_production_factor_sprite, true),
                new("Building the infrastructure", 3, building_the_infrastructure_sprite[2], true),
                new("Fog of war", 2, fog_of_war_sprite, true)
            }),
        };

        AddBaseEffectsToTree(militaryTree, milBaseEffects);
        AddBaseEffectsToTree(economicTree, ecBaseEffects);
        AddBaseEffectsToTree(administrativeTree, admBaseEffects);
    }

    void AddBaseEffectsToTree(List<TechLevel> techTree, List<TechEffect> baseEffects)
    {
        foreach (var level in techTree)
        {
            foreach (var baseEffect in baseEffects)
            {
                var existingEffect = level.Effects.FirstOrDefault(e => e.Name == baseEffect.Name);
                if (existingEffect != null)
                {
                    if (baseEffect.NumericValue.HasValue && existingEffect.NumericValue.HasValue)
                    {
                        existingEffect.NumericValue += baseEffect.NumericValue.Value;
                    }
                    else if (baseEffect.IntValue.HasValue && existingEffect.IntValue.HasValue)
                    {
                        existingEffect.IntValue += baseEffect.IntValue.Value;
                    }
                    else if (baseEffect.BoolValue.HasValue && existingEffect.BoolValue.HasValue)
                    {
                        existingEffect.BoolValue = baseEffect.BoolValue.Value;
                    }
                }
                else
                {
                    TechEffect newEffect;
                    if (baseEffect.NumericValue.HasValue)
                    {
                        newEffect = new TechEffect(baseEffect.Name, baseEffect.NumericValue.Value, baseEffect.Icon, baseEffect.IsEffectPositive);
                    }
                    else if (baseEffect.IntValue.HasValue)
                    {
                        newEffect = new TechEffect(baseEffect.Name, baseEffect.IntValue.Value, baseEffect.Icon, baseEffect.IsEffectPositive);
                    }
                    else
                    {
                        newEffect = new TechEffect(baseEffect.Name, baseEffect.BoolValue.Value, baseEffect.Icon, baseEffect.IsEffectPositive);
                    }
                    level.Effects.Add(newEffect);
                }
            }
        }
    }

    void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(mil_tooltip_container.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(ec_tooltip_container.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(adm_tooltip_container.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(mil_next_level_container.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(ec_next_level_container.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(adm_next_level_container.GetComponent<RectTransform>());
    }

    private IEnumerator RefreshUITemporarily(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            ForceRebuildLayout();
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}