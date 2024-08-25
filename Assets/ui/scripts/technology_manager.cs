using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class technology_manager : MonoBehaviour
{
    List<string> militaryTree = new()
    {
        "Level 1: Increases unit combat strength by 5%",
        "Level 2: Unlocks Fort",
        "Level 3: Reduces unit maintenance cost by 10%",
        "Level 4: Increases unit combat strength by 10%",
        "Level 5: Unlocks Fort Upgrade I",
        "Level 6: Reduces unit construction cost by 10%",
        "Level 7: Unlocks Fort Upgrade II",
        "Level 8: Increases recruitable population percentage",
        "Level 9: Reduces penalty from newly conquered provinces",
        "Level 10: Increases unit combat strength by 20%"
    };

    List<string> economicTree = new()
    {
        "Level 1: Increases resource growth by 10%",
        "Level 2: Allows ship construction and increases resource growth by 10%",
        "Level 3: Increases resource growth by 10%",
        "Level 4: Allows choice of tax law",
        "Level 5: Increases tax revenue by 15%",
        "Level 6: Unlocks Mine Upgrade I",
        "Level 7: Increases resource growth by 10%",
        "Level 8: Increases tax revenue by 10%",
        "Level 9: Allows choice of tax law",
        "Level 10: Unlocks Mine Upgrade II"
    };

    List<string> administrativeTree = new()
    {
        "Level 1: Unlocks infrastructure",
        "Level 2: Unlocks school",
        "Level 3: Unlocks province action - festival",
        "Level 4: Unlocks province action - tax relief",
        "Level 5: Reduces penalty from temporary effects by 10%",
        "Level 6: Allows production in occupied provinces (1/5 of base value)",
        "Level 7: Reduces penalty from temporary effects by 10%",
        "Level 8: Increases bonuses from temporary effects by 20%",
        "Level 9: Reduces chances of rebellion",
        "Level 10: Allows production in occupied provinces (1/2 of base value)"
    };

    [SerializeField] private Map map;

    [SerializeField] private dialog_box_manager dialog_box;

    [SerializeField] private Button mil_tech_button;
    [SerializeField] private Button ec_tech_button;
    [SerializeField] private Button adm_tech_button;

    [SerializeField] private TMP_Text mil_current_level_text;
    [SerializeField] private TMP_Text ec_current_level_text;
    [SerializeField] private TMP_Text adm_current_level_text;

    [SerializeField] private TMP_Text mil_next_level_content;
    [SerializeField] private TMP_Text ec_next_level_content;
    [SerializeField] private TMP_Text adm_next_level_content;

    [SerializeField] private TMP_Text mil_science_points_text;
    [SerializeField] private TMP_Text ec_science_points_text;
    [SerializeField] private TMP_Text adm_science_points_text;

    [SerializeField] private TMP_Text mil_current_lvl_tooltip;
    [SerializeField] private TMP_Text ec_current_lvl_tooltip;
    [SerializeField] private TMP_Text adm_current_lvl_tooltip;

    int militaryLevel = 3;
    int economicLevel = 5;
    int administrativeLevel = 10;

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


        // Kod do testow, ale moze jakies czesci bedzie mozna wykorzystac

        UpdateTooltipAndContent(militaryLevel, militaryTree, mil_current_lvl_tooltip, mil_next_level_content, mil_tech_button, mil_current_level_text);
        UpdateTooltipAndContent(economicLevel, economicTree, ec_current_lvl_tooltip, ec_next_level_content, ec_tech_button, ec_current_level_text);
        UpdateTooltipAndContent(administrativeLevel, administrativeTree, adm_current_lvl_tooltip, adm_next_level_content, adm_tech_button, adm_current_level_text);

        mil_tech_button.onClick.AddListener(() =>
        {
            militaryLevel++;
            UpdateTooltipAndContent(militaryLevel, militaryTree, mil_current_lvl_tooltip, mil_next_level_content, mil_tech_button, mil_current_level_text);
        });

        ec_tech_button.onClick.AddListener(() =>
        {
            economicLevel++;
            UpdateTooltipAndContent(economicLevel, economicTree, ec_current_lvl_tooltip, ec_next_level_content, ec_tech_button, ec_current_level_text);
        });

        adm_tech_button.onClick.AddListener(() =>
        {
            administrativeLevel++;
            UpdateTooltipAndContent(administrativeLevel, administrativeTree, adm_current_lvl_tooltip, adm_next_level_content, adm_tech_button, adm_current_level_text);
        });

        DisableButton(ec_tech_button);
    }

    void UpdateTooltipAndContent(int level, List<string> techTree, TMP_Text tooltipText, TMP_Text nextLevelContent, Button techButton, TMP_Text currentLevelText)
    {
        currentLevelText.text = level.ToString();

        tooltipText.text = string.Join("\n", techTree.Take(level));

        if (level < techTree.Count)
        {
            nextLevelContent.text = techTree[level];
        }

        if (level >= techTree.Count)
        {
            nextLevelContent.text = "Maximum level reached";
            techButton.gameObject.SetActive(false);
        }
    }

    void DisableButton(Button techButton)
    {
        techButton.interactable = false;
        techButton.GetComponent<Image>().color = new Color32(176, 41, 23, 255); // red color
        TMP_Text buttonText = techButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = "Not enough funds";
        }
    }
}