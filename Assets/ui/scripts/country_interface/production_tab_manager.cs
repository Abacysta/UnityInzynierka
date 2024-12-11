using Assets.classes.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class production_tab_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text country_population_text, country_happiness_text;
    [SerializeField] private List<Toggle> toggles;
    [SerializeField] private TMP_Text tax_text, happ_text;
    [SerializeField] private GameObject panel;
    public Map Map { get => map; set => map = value; }
    public List<Toggle> TaxToggles { get => toggles; set => toggles = value; }
    public TMP_Text Tax_text { get => tax_text; set => tax_text = value; }
    public TMP_Text Happ_text { get => happ_text; set => happ_text = value; }
    public GameObject ResourcePanel { get => panel; set => panel = value; }

    void Start()
    {
        InitializeTaxToggles();
    }

    public void InitializeTaxToggles()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            var idx = i;
            toggles[idx].onValueChanged.AddListener(delegate {
                SetTaxType(idx);
            });
            toggles[idx].onValueChanged.AddListener(delegate { UpdateTaxInfo(); });
        }
    }

    void OnEnable()
    {
        Debug.Log(GetTaxType() + "-tax");

        SetCountryPopulationText();
        SetCountryHappinessText();
        SetTaxButtons();
        UpdateTaxInfo();
        UpdateGainInfo();
    }

    public void SetCountryPopulationText()
    {
        int countryPopulation = map.CurrentPlayer.Provinces.Sum(p => p.Population);
        country_population_text.text = countryPopulation.ToString();
    }

    public void SetCountryHappinessText()
    {
        int countryHappiness = (int)map.CurrentPlayer.Provinces.Average(p => p.Happiness);

        country_happiness_text.text = countryHappiness.ToString() + "%";
        country_happiness_text.color =
            countryHappiness < 9 ? new Color32(255, 41, 35, 255) : // red
            countryHappiness < 50 ? new Color32(255, 162, 0, 255) : // orange
            Color.green;
    }

    private void UpdateAllToggleTransparencies()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            Image image = toggles[i].GetComponent<Image>();
            if (toggles[i].isOn) SetTransparency(image, 1.0f);
            else SetTransparency(image, 0.5f);
        }
    }

    void SetTransparency(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private void SetTaxButtons()
    {
        var toSet = GetTaxType();

        for (int i = 0; i < toggles.Count; i++)
        {
            toggles[i].interactable = i < map.CurrentPlayer.techStats.LvlTax + 3;
            if (i == toSet) toggles[i].isOn = true;
        }
    }

    private void UpdateTaxInfo()
    {
        ATax tax = map.CurrentPlayer.Tax;
        var tax_percent = tax.GoldP;
        var tax_happ = tax.HappP;
        tax_text.text = tax_percent * 100 + "%";
        happ_text.text = (tax_happ >= 0 ? "+" : "") + tax_happ + "%";

        happ_text.color = tax_happ > 0 ? Color.green : Color.red;

        UpdateGainInfo();
    }

    private void SetTaxType(int it)
    {
        switch (it)
        {
            case 1:
                map.CurrentPlayer.Tax = new MediumTaxes();
                return;
            case 2:
                map.CurrentPlayer.Tax = new HighTaxes();
                return;
            case 3:
                map.CurrentPlayer.Tax = new WarTaxes();
                return;
            case 4:
                map.CurrentPlayer.Tax = new InvesmentTaxes();
                return;
            default:
                map.CurrentPlayer.Tax = new LowTaxes();
                return;
        }
    }

    public int GetTaxType()
    {
        switch (map.CurrentPlayer.Tax)
        {
            case MediumTaxes:
                return 1;
            case HighTaxes:
                return 2;
            case WarTaxes:
                return 3;
            case InvesmentTaxes:
                return 4;
            default:
                return 0;
        }
    }

    private void UpdateGainInfo()
    {
        var gains = Map.PowerUtilites.GetGain(map, map.CurrentPlayer);

        var gold = panel.transform.Find("gold_text").GetComponent<TMP_Text>();
        var wood = panel.transform.Find("wood_text").GetComponent<TMP_Text>();
        var iron = panel.transform.Find("iron_text").GetComponent<TMP_Text>();
        var sp = panel.transform.Find("science_points_text").GetComponent<TMP_Text>();

        gold.SetText((gains[Resource.Gold] >= 0 ? "+" : "") + gains[Resource.Gold]);
        wood.SetText((gains[Resource.Wood] >= 0 ? "+" : "") + gains[Resource.Wood]);
        iron.SetText((gains[Resource.Iron] >= 0 ? "+" : "") + gains[Resource.Iron]);
        sp.SetText((gains[Resource.SciencePoint] >= 0 ? "+" : "") + gains[Resource.SciencePoint]);
    }
}