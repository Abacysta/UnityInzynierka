using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class provinces_table_manager : MonoBehaviour
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
        sortedProvinces = new List<Province>(map.Provinces);

        sort_by_name_button.onClick.AddListener(() => ToggleSort("name"));
        sort_by_population_button.onClick.AddListener(() => ToggleSort("population"));
        sort_by_happiness_button.onClick.AddListener(() => ToggleSort("happiness"));
        sort_by_resources_button.onClick.AddListener(() => ToggleSort("resources"));

        SortData("population");
    }

    void OnEnable()
    {
        if (sortedProvinces == null) return;
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
            if (province.Owner_id == 1)
            {
                GameObject rowObj = Instantiate(province_row, content);

                rowObj.transform.Find("name_text").GetComponent<TMP_Text>().text = province.Name;
                rowObj.transform.Find("population_text").GetComponent<TMP_Text>().text = province.Population.ToString();
                rowObj.transform.Find("happiness_text").GetComponent<TMP_Text>().text = province.Happiness.ToString() + "%";

                Image resourceImage = rowObj.transform.Find("resource/resource_img").GetComponent<Image>();
                resourceImage.sprite = GetResourceSprite(province.Resources);
            }
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
                    ? sortedProvinces.OrderBy(p => p.Name).ToList()
                    : sortedProvinces.OrderByDescending(p => p.Name).ToList();
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
