using Mosframe;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class dynamic_scroll_view_pop_country_row : UIBehaviour, IDynamicScrollViewItem
{
    [SerializeField] private end_screen_manager end_screen_manager;
    [SerializeField] private Image coat_img;
    [SerializeField] private TMP_Text country_name;
    [SerializeField] private TMP_Text population_value;

    public void onUpdateItem(int index)
    {
        Country country = end_screen_manager.PopCountries[index];
        if (country == null) Debug.LogError("Kraj wzial i zniknal POP");
        country.SetCoatandColor(coat_img);
        country_name.text = country.Name;
        population_value.SetText(country.Provinces.Sum(p => p.Population).ToString());
    }
}
