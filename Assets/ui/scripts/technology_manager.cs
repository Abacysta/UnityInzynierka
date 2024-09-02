using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private TMP_Text mil_cost_text;
    [SerializeField] private TMP_Text ec_cost_text;
    [SerializeField] private TMP_Text adm_cost_text;

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

    [SerializeField] private List<Sprite> building_the_fort_sprite;
    [SerializeField] private List<Sprite> building_the_mine_sprite;
    [SerializeField] private List<Sprite> building_the_infrastructure_sprite;
    [SerializeField] private List<Sprite> building_the_school_sprite;

    [SerializeField] private Sprite holding_the_festival_sprite;
    [SerializeField] private Sprite introducing_the_tax_break_sprite;
    [SerializeField] private Sprite occupation_production_factor_sprite;
    [SerializeField] private Sprite penalties_from_temporary_statuses_sprite;
    [SerializeField] private Sprite supressing_the_rebelion_sprite;
    [SerializeField] private Sprite population_sprite;

    public class TechEffect
    {
        public string Name { get; private set; }
        public float? NumericValue { get; private set; } 
        public int? IntValue { get; private set; }
        public bool? BoolValue { get; private set; }
        public Sprite Icon { get; private set; }
        public bool IsEffectPositive { get; private set; }

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
            if (Name.StartsWith("Building") || Name.Contains("tax law"))
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
            private set { effects = value; }
        }
    }

    private List<TechLevel> militaryTree;
    private List<TechLevel> economicTree;
    private List<TechLevel> administrativeTree;

    private List<TechEffect> baseEffects;

    int militaryLevel = 0;
    int economicLevel = 0;
    int administrativeLevel = 0;

    void Awake()
    {
        InitializeLevels();
    }

    void Start()
    {
        /*
         
        int militaryLevel = map.Countries[0].Technology[Technology.Military];
        int economicLevel = map.Countries[0].Technology[Technology.Economic];
        int administrativeLevel = map.Countries[0].Technology[Technology.Administrative]; 
         
        military_tech_button.onClick.AddListener(() => dialog_box.invokeTechUpgradeBox());
        economic_tech_button.onClick.AddListener(() => dialog_box.invokeTechUpgradeBox());
        administrative_tech_button.onClick.AddListener(() => dialog_box.invokeTechUpgradeBox());
        */

        mil_tech_button.onClick.AddListener(() =>
        {
            militaryLevel++;
            SetTechnologyData(mil_tooltip_container, mil_tooltip_text, militaryLevel, militaryTree,
                mil_next_level_container, mil_tech_button, mil_current_level_text, mil_next_level_text);
        });

        ec_tech_button.onClick.AddListener(() =>
        {
            economicLevel++;
            SetTechnologyData(ec_tooltip_container, ec_tooltip_text, economicLevel, economicTree,
                ec_next_level_container, ec_tech_button, ec_current_level_text, ec_next_level_text);
        });

        adm_tech_button.onClick.AddListener(() =>
        {
            administrativeLevel++;
            SetTechnologyData(adm_tooltip_container, adm_tooltip_text, administrativeLevel, administrativeTree,
                adm_next_level_container, adm_tech_button, adm_current_level_text, adm_next_level_text);
        });
    }

    void OnEnable()
    {
        SetButtonColorToGreen(mil_tech_button);
        SetButtonColorToGreen(ec_tech_button);
        SetButtonColorToGreen(adm_tech_button);

        SetButtonColorToGreen(mil_tech_button);
        SetButtonColorToGreen(ec_tech_button);
        SetButtonColorToGreen(adm_tech_button);

        SetTechnologyData(mil_tooltip_container, mil_tooltip_text, militaryLevel, militaryTree,
            mil_next_level_container, mil_tech_button, mil_current_level_text, mil_next_level_text);
        SetTechnologyData(ec_tooltip_container, ec_tooltip_text, economicLevel, economicTree,
            ec_next_level_container, ec_tech_button, ec_current_level_text, ec_next_level_text);
        SetTechnologyData(adm_tooltip_container, adm_tooltip_text, administrativeLevel, administrativeTree,
            adm_next_level_container, adm_tech_button, adm_current_level_text, adm_next_level_text);
    }

    void Update()
    {
        ForceRebuildLayout();
    }

    public void SetTechnologyData(GameObject tooltip, TMP_Text tooltipText, int level, List<TechLevel> techTree, GameObject nextLevelContainer, 
        Button techButton, TMP_Text currentLevelText, TMP_Text nextLevelText)
    {
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
            nextLevelText.text = $"Level {level + 1}:";

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
    }

    private void SetButtonColorToRed(Button techButton)
    {
        techButton.interactable = false;
        techButton.GetComponent<Image>().color = new Color32(176, 41, 23, 255); // red color
        TMP_Text buttonText = techButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = "Not enough funds";
        }
    }
    private void SetButtonColorToGreen(Button techButton)
    {
        techButton.interactable = true;
        TMP_Text buttonText = techButton.GetComponentInChildren<TMP_Text>();
        techButton.GetComponent<Image>().color = new Color32(35, 82, 29, 255); // green color

        if (buttonText != null)
        {
            buttonText.text = "Upgrade technology";
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
                float sumFloat = g.Sum(b => b.NumericValue ?? 0f);
                int sumInt = g.Sum(b => b.IntValue ?? 0);

                return hasFloat
                    ? new TechEffect(g.Key, sumFloat, g.First().Icon, g.All(b => b.IsEffectPositive))
                    : new TechEffect(g.Key, sumInt, g.First().Icon, g.All(b => b.IsEffectPositive));
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
        baseEffects = new List<TechEffect>
        {
            new("Production factor", 0.05f, production_factor_sprite, true),
            new("Tax revenue", 0.01f, tax_revenue_sprite, true),
            new("Population", 0.03f, population_sprite, true),
            new("Army combat power", 0.05f, army_combat_power_sprite, true),
            new("Army upkeep cost", 0.03f, army_upkeep_cost_sprite, true),
            new("Army cost", 0.05f, army_cost_sprite, true),
        };

        militaryTree = new List<TechLevel>
        {
            // Level 1
            new(new List<TechEffect> {
                new("Army combat power", 0.01f, army_combat_power_sprite, true)
            }),
            // Level 2
            new(new List<TechEffect> {
                new("Building the fort", 1, building_the_fort_sprite[0], true),
                new("Army cost", 0.05f, army_cost_sprite, false)
            }),
            // Level 3
            new(new List<TechEffect> {
                new("Army upkeep cost", -0.03f, army_upkeep_cost_sprite, false),
                new("Occupation time", -1, occupation_time_sprite, false)
            }),
            // Level 4
            new(new List<TechEffect> {
                new("Army combat power", 0.1f, army_combat_power_sprite, true),
                new("Army cost", 0.1f, army_cost_sprite, false)
            }),
            // Level 5
            new(new List<TechEffect> {
                new("Building the fort", 2, building_the_fort_sprite[1], true),
                new("Army upkeep cost", 0.02f, army_upkeep_cost_sprite, false)
            }),
            // Level 6
            new(new List<TechEffect> {
                new("Army cost", -0.1f, army_cost_sprite, false),
                new("Army move range", 1, army_move_range_sprite, true)
            }),
            // Level 7
            new(new List<TechEffect> {
                new("Building the fort", 3, building_the_fort_sprite[2], true),
                new("Army upkeep cost", 0.02f, army_upkeep_cost_sprite, false)
            }),
            // Level 8
            new(new List<TechEffect> {
                new("Recruitable population", 0.05f, recruitable_population_sprite, true),
                new("Army cost", -0.1f, army_cost_sprite, false),
                new("Army upkeep cost", -0.15f, army_upkeep_cost_sprite, false),
            }),
            // Level 9
            new(new List<TechEffect> {
                new("Occupation penalty", -0.35f, occupation_penalty_sprite, false)
            }),
            // Level 10
            new(new List<TechEffect> {
                new("Army combat power", 0.15f, army_combat_power_sprite, true),
                new("Water move factor", 0.5f, water_move_factor_sprite, true)
            }),
        };

        economicTree = new List<TechLevel>
        {
            // Level 1
            new(new List<TechEffect> {
                new("Production factor", 0.05f, production_factor_sprite, true)
            }),
            // Level 2
            new(new List<TechEffect> {
                new("Can boat", true, can_boat_sprite, true),
                new("Building the mine", 1, building_the_mine_sprite[0], true)
            }),
            // Level 3
            new(new List<TechEffect> {
                new("Production factor", 0.05f, production_factor_sprite, true)
            }),
            // Level 4
            new(new List<TechEffect> {
                new("Choosing tax law", 1, choosing_new_tax_law_sprite, true)
            }),
            // Level 5
            new(new List<TechEffect> {
                new("Tax revenue", 0.15f, tax_revenue_sprite, true)
            }),
            // Level 6
            new(new List<TechEffect> {
                new("Building the mine", 2, building_the_mine_sprite[1], true)
            }),
            // Level 7
            new(new List<TechEffect> {
                new("Production factor", 0.1f, production_factor_sprite, true)
            }),
            // Level 8
            new(new List<TechEffect> {
                new("Tax revenue", 0.05f, tax_revenue_sprite, true)
            }),
            // Level 9
            new(new List<TechEffect> {
                new("Choosing tax law", 2, choosing_new_tax_law_sprite, true)
            }),
            // Level 10
            new(new List<TechEffect> {
                new("Building the mine", 3, building_the_mine_sprite[2], true),
                new("Production factor", 0.05f, production_factor_sprite, true)
            }),
        };

        administrativeTree = new List<TechLevel>
        {
            // Level 1
            new(new List<TechEffect> {
                new("Building the infrastructure", 1, building_the_infrastructure_sprite[0], true)
            }),
            // Level 2
            new(new List<TechEffect> {
                new("Building the school", 1, building_the_school_sprite[0], true)
            }),
            // Level 3
            new(new List<TechEffect> {
                new("Holding the festival", true, holding_the_festival_sprite, true),
                new("Tax revenue", 0.03f, tax_revenue_sprite, true)
            }),
            // Level 4
            new(new List<TechEffect> {
                new("Introducing the tax break", true, introducing_the_tax_break_sprite, true)
            }),
            // Level 5
            new(new List<TechEffect> {
                new("Building the infrastructure", 2, building_the_infrastructure_sprite[1], true)
            }),
            // Level 6
            new(new List<TechEffect> {
                new("Occupation production factor", 0.1f, occupation_production_factor_sprite, true)
            }),
            // Level 7
            new(new List<TechEffect> {
                new("Penalties from temporary statuses", -0.1f, null, false)
            }),
            // Level 8
            new(new List<TechEffect> {
                new("Tax revenue", 0.01f, tax_revenue_sprite, true),
                new("Recruitable population", 0.02f, recruitable_population_sprite, true)
            }),
            // Level 9
            new(new List<TechEffect> {
                new("Suppressing the rebellion", true, supressing_the_rebelion_sprite, true),
                new("Army cost", -0.05f, army_cost_sprite, false)
            }),
            // Level 10
            new(new List<TechEffect> {
                new("Occupation production factor", 0.4f, occupation_production_factor_sprite, true),
                new("Building the infrastructure", 3, building_the_infrastructure_sprite[2], true)
            }),
        };
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
}