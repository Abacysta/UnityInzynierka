using TMPro;
using UnityEngine;

public class map_ui : MonoBehaviour
{
    [SerializeField] private settings_menu settings_menu_scr;
    [SerializeField] private GameObject settings_menu_ui;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private GameObject dialog_box;
    [SerializeField] private dialog_box_manager box_manager;

    void Start()
    {
        
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(dialog_box.activeSelf) {
                box_manager.HideDialog();
            }
            else if(province_interface.activeSelf) {
                province_interface.SetActive(false);
            }
            else {
                settings_menu_ui.SetActive(settings_menu_ui.activeSelf);
            }
        }
    }
}
