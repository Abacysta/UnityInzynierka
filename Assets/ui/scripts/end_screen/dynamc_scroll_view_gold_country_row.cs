using Mosframe;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class dynamc_scroll_view_gold_country_row : UIBehaviour, IDynamicScrollViewItem
{
    [SerializeField] private end_screen_manager end_screen_manager;
    [SerializeField] private Image coat_img;
    [SerializeField] private TMP_Text country_name;
    [SerializeField] private TMP_Text gold_value;

    public void onUpdateItem(int index)
    {
        Country country = end_screen_manager.GoldCountries[index];

        country.setCoatandColor(coat_img);
        country_name.text = country.Name;
        gold_value.text = country.Resources[Resource.Gold].ToString();
    }
}