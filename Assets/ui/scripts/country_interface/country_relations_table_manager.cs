using Mosframe;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class country_relations_table_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_countries_view;
    [SerializeField] private Button sort_by_country_name_button;
    [SerializeField] private Button sort_by_their_opinion_button;
    [SerializeField] private Button sort_by_our_opinion_button;

    private List<Country> sortedCountries;
    private string currentSortCriteria = "country_name";
    private bool isAscending = true;

    public List<Country> SortedCountries { get => sortedCountries; set => sortedCountries = value; }
    public Map Map { get => map; set => map = value; }

    private void InitializeTestData()
    {
        SortedCountries = new List<Country>
        {
            new Country(2, "Berbers", (2, 2), Color.cyan, Map),
            new Country(3, "Egyptians", (3, 3), Color.blue, Map),
            new Country(4, "Vikings", (4, 4), Color.green, Map),
            new Country(5, "Huns", (5, 5), Color.yellow, Map)
        };

        var opinions = new Dictionary<int, int>
        {
            { 1, 0 }, { 3, 2 }, { 4, -2 }, { 5, 1 }
        };
        SortedCountries[0].Opinions = new Dictionary<int, int>(opinions);

        opinions = new Dictionary<int, int>
        {
            { 1, 1 }, { 2, -3 }, { 4, 2 }, { 5, 0 } 
        };
        SortedCountries[1].Opinions = new Dictionary<int, int>(opinions);

        opinions = new Dictionary<int, int>
        {
            { 1, -1 }, { 2, 2 }, { 3, 0 }, { 5, 3 }
        };
        SortedCountries[2].Opinions = new Dictionary<int, int>(opinions);

        opinions = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, -1 }, { 3, 2 }, { 4, 3 }
        };
        SortedCountries[3].Opinions = new Dictionary<int, int>(opinions);
    }

    void Start()
    {
        sort_by_country_name_button.onClick.AddListener(() => ToggleSort("country_name"));
        sort_by_their_opinion_button.onClick.AddListener(() => ToggleSort("their_opinion"));
        sort_by_our_opinion_button.onClick.AddListener(() => ToggleSort("our_opinion"));

        SetData();
    }

    void OnEnable()
    {
        SetData();
    }

    private void SetData()
    {
        if (map == null)
        {
            Debug.LogError("Map is not assigned.");
            return;
        }

        if (dynamic_vscroll_countries_view == null)
        {
            Debug.LogError("DynamicVScrollView is not assigned.");
            return;
        }

        //sortedCountries = new List<Country>(map.Countries);
        InitializeTestData();
        dynamic_vscroll_countries_view.totalItemCount = SortedCountries.Count;
        SortData(currentSortCriteria);
    }

    private void DisplayTable()
    {
        dynamic_vscroll_countries_view.refresh();
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
        if (Map.currentPlayer == null)
        {
            Debug.LogError("Current player is not set in the Map.");
            return;
        }

        switch (sortBy)
        {
            case "country_name":
                SortedCountries = isAscending
                    ? SortedCountries.OrderBy(p => ExtractNumberFromName(p.Name)).ThenBy(p => p.Name).ToList()
                    : SortedCountries.OrderByDescending(p => ExtractNumberFromName(p.Name)).ThenByDescending(p => p.Name).ToList();
                break;
            case "their_opinion":
                SortedCountries = isAscending
                    ? SortedCountries.OrderBy(p => p.Opinions.ContainsKey(Map.currentPlayer) ? p.Opinions[Map.currentPlayer] : 0).ToList()
                    : SortedCountries.OrderByDescending(p => p.Opinions.ContainsKey(Map.currentPlayer) ? p.Opinions[Map.currentPlayer] : 0).ToList();
                break;
            case "our_opinion":
                SortedCountries = isAscending
                    ? SortedCountries.OrderBy(p => Map.CurrentPlayer.Opinions.ContainsKey(p.Id) ? Map.CurrentPlayer.Opinions[p.Id] : 0).ToList()
                    : SortedCountries.OrderByDescending(p => Map.CurrentPlayer.Opinions.ContainsKey(p.Id) ? Map.CurrentPlayer.Opinions[p.Id] : 0).ToList();
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