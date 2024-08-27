using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class technology_manager : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private GameObject mil_tree_prefab;
    [SerializeField] private GameObject ec_tree_prefab;
    [SerializeField] private GameObject adm_tree_prefab;

    [SerializeField] private dialog_box_manager dialog_box;

    [SerializeField] private GameObject mil_tooltip_column_1;
    [SerializeField] private GameObject mil_tooltip_column_2;
    [SerializeField] private GameObject ec_tooltip_column_1;
    [SerializeField] private GameObject ec_tooltip_column_2;
    [SerializeField] private GameObject adm_tooltip_column_1;
    [SerializeField] private GameObject adm_tooltip_column_2;

    [SerializeField] private Button mil_tech_button;
    [SerializeField] private Button ec_tech_button;
    [SerializeField] private Button adm_tech_button;

    [SerializeField] private TMP_Text mil_current_level_text;
    [SerializeField] private TMP_Text ec_current_level_text;
    [SerializeField] private TMP_Text adm_current_level_text;

    [SerializeField] private GameObject mil_next_level_panel;
    [SerializeField] private GameObject adm_next_level_panel;
    [SerializeField] private GameObject ec_next_level_panel;

    [SerializeField] private TMP_Text mil_cost_text;
    [SerializeField] private TMP_Text ec_cost_text;
    [SerializeField] private TMP_Text adm_cost_text;

    private List<GameObject> militaryTree = new();
    private List<GameObject> economicTree = new();
    private List<GameObject> administrativeTree = new();

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

        AddChildrenToList(mil_tree_prefab, militaryTree);
        AddChildrenToList(ec_tree_prefab, economicTree);
        AddChildrenToList(adm_tree_prefab, administrativeTree);

        UpgradeTechnology(mil_tooltip_column_1, mil_tooltip_column_2,
            militaryLevel, militaryTree, mil_next_level_panel, mil_tech_button, mil_current_level_text);
        UpgradeTechnology(ec_tooltip_column_1, ec_tooltip_column_2,
            economicLevel, economicTree, ec_next_level_panel, ec_tech_button, ec_current_level_text);
        UpgradeTechnology(adm_tooltip_column_1, adm_tooltip_column_2,
            administrativeLevel, administrativeTree, adm_next_level_panel, adm_tech_button, adm_current_level_text);

        mil_tech_button.onClick.AddListener(() =>
        {
            militaryLevel++;
            UpgradeTechnology(mil_tooltip_column_1, mil_tooltip_column_2,
                militaryLevel, militaryTree, mil_next_level_panel, mil_tech_button, mil_current_level_text);
        });

        ec_tech_button.onClick.AddListener(() =>
        {
            economicLevel++;
            UpgradeTechnology(ec_tooltip_column_1, ec_tooltip_column_2,
                economicLevel, economicTree, ec_next_level_panel, ec_tech_button, ec_current_level_text);
        });

        adm_tech_button.onClick.AddListener(() =>
        {
            administrativeLevel++;
            UpgradeTechnology(adm_tooltip_column_1, adm_tooltip_column_2,
                administrativeLevel, administrativeTree, adm_next_level_panel, adm_tech_button, adm_current_level_text);
        });

        SetButtonColorToRed(ec_tech_button);
    }

    void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(mil_tooltip_column_1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(mil_tooltip_column_2.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(ec_tooltip_column_1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(ec_tooltip_column_2.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(adm_tooltip_column_1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(adm_tooltip_column_2.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(mil_next_level_panel.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(ec_next_level_panel.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(adm_next_level_panel.GetComponent<RectTransform>());
    }

    public void UpgradeTechnology(GameObject tooltip_column_1, GameObject tooltip_column_2, int level,
        List<GameObject> techTree, GameObject nextLevelPanel, Button techButton, TMP_Text currentLevelText)
    {
        // Tech tooltip
        ClearChildren(tooltip_column_1.transform);
        ClearChildren(tooltip_column_2.transform);

        var levelNodes = techTree.Take(level).ToList();
        for (int i = 0; i < levelNodes.Count; i++)
        {
            var column = (i < 5) ? tooltip_column_1.transform : tooltip_column_2.transform;
            Instantiate(levelNodes[i], column);
        }

        // Current level text
        currentLevelText.text = level.ToString();

        // Tooltip and next level panel
        if (level <= 10)
        {
            foreach (Transform child in nextLevelPanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            Instantiate(techTree[level], nextLevelPanel.transform);
        }

        if (level >= 10)
        {
            DeactivateButton(techButton);
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

    private void DeactivateButton(Button techButton)
    {
        techButton.gameObject.SetActive(false);
    }

    private void AddChildrenToList(GameObject prefab, List<GameObject> list)
    {
        foreach (Transform child in prefab.transform)
        {
            list.Add(child.gameObject);
        }
    }
    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}