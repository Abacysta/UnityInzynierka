using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class production_tab_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text country_population_text, country_happiness_text;
    [SerializeField] private List<Toggle> tax_law_buttons_list;

    void Start()
    {
        UpdateAllToggleTransparencies();

        foreach (Toggle toggle in tax_law_buttons_list)
        {
            toggle.onValueChanged.AddListener(delegate { UpdateAllToggleTransparencies(); });
        }
    }

    void OnEnable()
    {
        int countryPopulation = CalculateCountryPopulation();
        int countryHappiness = CalculateCountryHappiness();

        SetCountryPopulationText(countryPopulation);
        SetCountryHappinessText(countryHappiness);
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
        foreach (Toggle toggle in tax_law_buttons_list)
        {
            Image image = toggle.GetComponent<Image>();
            if (toggle.isOn) SetTransparency(image, 1.0f);
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
}