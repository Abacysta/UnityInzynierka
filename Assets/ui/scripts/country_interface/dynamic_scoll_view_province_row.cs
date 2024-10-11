using UnityEngine;
using UnityEngine.UI;
using Mosframe;
using TMPro;
using UnityEngine.EventSystems;

public class dynamic_scoll_view_province_row : UIBehaviour, IDynamicScrollViewItem
{
    [SerializeField] private province_table_manager province_table_manager;
    [SerializeField] private camera_controller camera_controller;
    [SerializeField] private province_click_handler province_click_handler;

    [SerializeField] private TMP_Text name_text;
    [SerializeField] private TMP_Text population_text;
    [SerializeField] private TMP_Text happiness_text;
    [SerializeField] private Image resource_img;

    [SerializeField] private Sprite gold_sprite;
    [SerializeField] private Sprite wood_sprite;
    [SerializeField] private Sprite iron_sprite;
    [SerializeField] private Sprite ap_sprite;

    private (int, int) provinceCoordinates;

    public void onUpdateItem(int index)
    {
        Province province = province_table_manager.SortedProvinces[index];
        provinceCoordinates = (province.X, province.Y);

        name_text.text = province_table_manager.SortedProvinces[index].Name;
        population_text.text = province_table_manager.SortedProvinces[index].Population.ToString();

        happiness_text.text = province_table_manager.SortedProvinces[index].Happiness.ToString() + "%";
        happiness_text.color = GetHappinessColor(province_table_manager.SortedProvinces[index].Happiness);

        resource_img.sprite = GetResourceSprite(province_table_manager.SortedProvinces[index].Resources);
    }

    private Color32 GetHappinessColor(float happiness)
    {
        return happiness < 9 ? new Color32(255, 41, 35, 255) : // red
               happiness < 50 ? new Color32(255, 162, 0, 255) : // orange
               Color.green;
    }

    private Sprite GetResourceSprite(string resourceName)
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

    public void OnProvinceRowClick()
    {
        camera_controller.ZoomCameraOnProvince(provinceCoordinates);
        province_click_handler.DisplayProvinceInterface(provinceCoordinates.Item1, provinceCoordinates.Item2);
    }
}