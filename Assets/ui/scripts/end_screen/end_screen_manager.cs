using Mosframe;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class end_screen_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_pop_countries_view;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_gold_countries_view;
    [SerializeField] private GameObject overlay;

    private List<Country> popCountries = new();
    private List<Country> goldCountries = new();

    public List<Country> PopCountries { get => popCountries; set => popCountries = value; }
    public List<Country> GoldCountries { get => goldCountries; set => goldCountries = value; }
    public Map Map { get => map; set => map = value; }

    private void Start()
    {
        SetPopulationRows();
        SetGoldRows();
    }

    void OnEnable()
    {
        overlay.SetActive(true);
        SetPopulationRows();
        SetGoldRows();
    }

    private void OnDisable()
    {
        overlay.SetActive(false);
    }

    private void SetPopulationRows()
    {
        PopCountries = map.Countries.Where(c => c.Id != 0).OrderByDescending(c => c.Provinces.Sum(p => p.Population)).ToList();
        dynamic_vscroll_pop_countries_view.totalItemCount = PopCountries.Count;
        dynamic_vscroll_pop_countries_view.refresh();
    }

    private void SetGoldRows()
    {
        GoldCountries = map.Countries.Where(c => c.Id != 0).OrderByDescending(c => c.Resources[Resource.Gold]).ToList();
        dynamic_vscroll_gold_countries_view.totalItemCount = GoldCountries.Count;
        dynamic_vscroll_gold_countries_view.refresh();
    }
}
