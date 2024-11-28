using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mosframe;

public class province_table_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_provinces_view;
    [SerializeField] private Button sort_by_name_button;
    [SerializeField] private Button sort_by_population_button;
    [SerializeField] private Button sort_by_happiness_button;
    [SerializeField] private Button sort_by_resources_button;

    private List<Province> sortedProvinces = new();
    private string currentSortCriteria = "name";
    private bool isAscending = true;

    public List<Province> SortedProvinces { get => sortedProvinces; set => sortedProvinces = value; }

    void Start()
    {
        sort_by_name_button.onClick.AddListener(() => ToggleSort("name"));
        sort_by_population_button.onClick.AddListener(() => ToggleSort("population"));
        sort_by_happiness_button.onClick.AddListener(() => ToggleSort("happiness"));
        sort_by_resources_button.onClick.AddListener(() => ToggleSort("resources"));

        SetData();
    }

    void OnEnable()
    {
        SetData();
    }

    private void SetData()
    {
        SortedProvinces = map.CurrentPlayer.Provinces.ToList();
        dynamic_vscroll_provinces_view.totalItemCount = SortedProvinces.Count;
        SortData(currentSortCriteria);
    }

    private void DisplayTable()
    {
        dynamic_vscroll_provinces_view.refresh();
    }

    private void ToggleSort(string sortBy)
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

    private void SortData(string sortBy)
    {
        switch (sortBy)
        {
            case "name":
                SortedProvinces = isAscending
                    ? SortedProvinces.OrderBy(p => ExtractNumberFromName(p.Name)).ThenBy(p => p.Name).ToList()
                    : SortedProvinces.OrderByDescending(p => ExtractNumberFromName(p.Name)).ThenByDescending(p => p.Name).ToList();
                break;
            case "population":
                SortedProvinces = isAscending
                    ? SortedProvinces.OrderBy(p => p.Population).ToList()
                    : SortedProvinces.OrderByDescending(p => p.Population).ToList();
                break;
            case "happiness":
                SortedProvinces = isAscending
                    ? SortedProvinces.OrderBy(p => p.Happiness).ToList()
                    : SortedProvinces.OrderByDescending(p => p.Happiness).ToList();
                break;
            case "resources":
                SortedProvinces = isAscending
                    ? SortedProvinces.OrderBy(p => p.ResourceType).ToList()
                    : SortedProvinces.OrderByDescending(p => p.ResourceType).ToList();
                break;
        }

        DisplayTable();
    }

    private int ExtractNumberFromName(string name)
    {
        var number = new string(name.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());

        return int.TryParse(number, out int result) ? result : 0;
    }
}