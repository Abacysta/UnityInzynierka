using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_table_manager : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private GameObject province_row;
    [SerializeField] private Transform content;

    [SerializeField] private Button sort_by_name_button;
    [SerializeField] private Button sort_by_population_button;
    [SerializeField] private Button sort_by_happiness_button;
    [SerializeField] private Button sort_by_resources_button;

    [SerializeField] private Sprite gold_sprite;
    [SerializeField] private Sprite wood_sprite;
    [SerializeField] private Sprite iron_sprite;
    [SerializeField] private Sprite ap_sprite;

    private List<Province> sortedProvinces;
    private string currentSortCriteria = "population";
    private bool isAscending = true;

    void Start()
    {
        sort_by_name_button.onClick.AddListener(() => ToggleSort("name"));
        sort_by_population_button.onClick.AddListener(() => ToggleSort("population"));
        sort_by_happiness_button.onClick.AddListener(() => ToggleSort("happiness"));
        sort_by_resources_button.onClick.AddListener(() => ToggleSort("resources"));

        sortedProvinces = new List<Province>(map.CurrentPlayer.Provinces);
        SortData("population");
    }

    void OnEnable()
    {
        sortedProvinces = new List<Province>(map.CurrentPlayer.Provinces);
        SortData(currentSortCriteria);
    }

    void DisplayTable()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (var province in sortedProvinces)
        {
            GameObject rowObj = Instantiate(province_row, content);

            rowObj.transform.Find("name_text").GetComponent<TMP_Text>().text = province.Name;
            rowObj.transform.Find("population_text").GetComponent<TMP_Text>().text = province.Population.ToString();
            TMP_Text happinessText = rowObj.transform.Find("happiness_text").GetComponent<TMP_Text>();

            happinessText.text = province.Happiness.ToString() + "%";
            happinessText.color = province.Happiness < 9 ? new Color32(255, 41, 35, 255) : // red
                                    province.Happiness < 50 ? new Color32(255, 162, 0, 255) : // orange
                                    Color.green;

            Image resourceImage = rowObj.transform.Find("resource/resource_img").GetComponent<Image>();
            resourceImage.sprite = GetResourceSprite(province.Resources);
        }
    }

    void ToggleSort(string sortBy)
    {
        if (currentSortCriteria == sortBy)
        {
            isAscending = !isAscending;
        }
        else
        {
            currentSortCriteria = sortBy;
            isAscending = true;
        }

        SortData(currentSortCriteria);
    }

    void SortData(string sortBy)
    {
        switch (sortBy)
        {
            case "name":
                sortedProvinces = isAscending
                    ? sortedProvinces.OrderBy(p => ExtractNumberFromName(p.Name)).ThenBy(p => p.Name).ToList()
                    : sortedProvinces.OrderByDescending(p => ExtractNumberFromName(p.Name)).ThenByDescending(p => p.Name).ToList();
                break;
            case "population":
                sortedProvinces = isAscending
                    ? sortedProvinces.OrderBy(p => p.Population).ToList()
                    : sortedProvinces.OrderByDescending(p => p.Population).ToList();
                break;
            case "happiness":
                sortedProvinces = isAscending
                    ? sortedProvinces.OrderBy(p => p.Happiness).ToList()
                    : sortedProvinces.OrderByDescending(p => p.Happiness).ToList();
                break;
            case "resources":
                sortedProvinces = isAscending
                    ? sortedProvinces.OrderBy(p => p.Resources).ToList()
                    : sortedProvinces.OrderByDescending(p => p.Resources).ToList();
                break;
        }

        DisplayTable();
    }

    int ExtractNumberFromName(string name)
    {
        var number = new string(name.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());

        return int.TryParse(number, out int result) ? result : 0;
    }

    Sprite GetResourceSprite(string resourceName)
    {
        switch (resourceName)
        {
            case "gold":
                return gold_sprite;
            case "wood":
                return wood_sprite;
            case "iron":
                return iron_sprite;
            case "empty":
                return ap_sprite;
            default:
                return null;
        }
    }
}