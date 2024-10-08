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
    [SerializeField] private game_manager game_manager;
    [SerializeField] private TMP_Text country_population_text, country_happiness_text;
    [SerializeField] private List<Toggle> toggles;
    [SerializeField] private TMP_Text tax_text, happ_text;
    [SerializeField] private GameObject panel;

    void Start()
    {
        for (int i = 0; i < toggles.Count; i++) {
            var idx = i;
            toggles[idx].onValueChanged.AddListener(delegate {
                setTaxType(idx);
            });
            toggles[idx].onValueChanged.AddListener(delegate { updateTaxInfo(); });
        }
    }

    void OnEnable()
    {
        Debug.Log(getTaxType() + "-tax");
        
        int countryPopulation = CalculateCountryPopulation();
        int countryHappiness = CalculateCountryHappiness();

        SetCountryPopulationText(countryPopulation);
        SetCountryHappinessText(countryHappiness);
        setTaxButtons();
        updateTaxInfo();
        updateGainInfo();
    }

    private int CalculateCountryPopulation()
    {
        return map.CurrentPlayer.Provinces.Sum(p => p.Population);
    }

    private int CalculateCountryHappiness()
    {
        return (int)map.CurrentPlayer.Provinces.Average(p => p.Happiness);
    }

    public void SetCountryPopulationText(int countryPopulation)
    {
        country_population_text.text = countryPopulation.ToString();
    }

    public void SetCountryHappinessText(int countryHappiness)
    {
        country_happiness_text.text = countryHappiness.ToString() + "%";
    }

    private void UpdateAllToggleTransparencies()
    {
        for(int i = 0; i < toggles.Count; i++) { 
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
    private void setTaxButtons() {
        var toSet = getTaxType();
        for(int i =0; i < toggles.Count; i++) {
            toggles[i].interactable = i < map.CurrentPlayer.techStats.lvlTax + 3;
            if (i == toSet) toggles[i].isOn = true;
        }
    }

    private void updateTaxInfo() {
        ITax tax = map.CurrentPlayer.Tax;
        var tax_percent = tax.GoldP;
        var tax_happ = tax.HappP;
        tax_text.text = tax_percent*100+"%";
        happ_text.text = tax_happ + "%";
        if (tax_happ > 0) happ_text.color = Color.green;
        else happ_text.color=Color.red;
        updateGainInfo();
    }

    private void setTaxType(int it) {
        switch (it) {
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

    private int getTaxType() {
        switch (map.CurrentPlayer.Tax) {
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

    private void updateGainInfo() {
        var gains = game_manager.getGain(map.CurrentPlayer);
        var gold = panel.transform.Find("gold_text").GetComponent<TMP_Text>();
        var wood = panel.transform.Find("wood_text").GetComponent<TMP_Text>();
        var iron = panel.transform.Find("iron_text").GetComponent<TMP_Text>();
        var sp = panel.transform.Find("science_points_text").GetComponent<TMP_Text>();
        gold.SetText((gains[Resource.Gold] > 0 ? "+" : "") + gains[Resource.Gold]);
        wood.SetText((gains[Resource.Wood] > 0 ? "+" : "") + gains[Resource.Wood]);
        iron.SetText((gains[Resource.Iron] > 0 ? "+" : "") + gains[Resource.Iron]);
        sp.SetText((gains[Resource.SciencePoint] > 0 ? "+" : "") + gains[Resource.SciencePoint]);
    }
}