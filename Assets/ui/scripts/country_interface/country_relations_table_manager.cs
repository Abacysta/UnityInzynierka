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
        sortedCountries = new List<Country>(map.Countries.Where(c => c.Id != 0 && c.Id != map.currentPlayer));
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