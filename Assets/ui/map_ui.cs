using TMPro;
using UnityEngine;

public class map_ui : MonoBehaviour
{
    [SerializeField] private settings_menu settings_menu_scr;
    [SerializeField] private GameObject settings_menu_ui;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private GameObject dialog_box;
    [SerializeField] private dialog_box_manager box_manager;
    [SerializeField] private map_loader loader;

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
        
        if (Input.GetKeyDown(KeyCode.I)) {
            if (loader.CurrentMode != map_loader.MapMode.Terrain) {
                loader.SetTerrain();
            }
        }
            else if (Input.GetKeyDown(KeyCode.O)) {
                if (loader.CurrentMode != map_loader.MapMode.Resource) {
                    loader.SetResources();
                }
            }
            else if (Input.GetKeyDown(KeyCode.P)) {
                if (loader.CurrentMode != map_loader.MapMode.Happiness) {
                    loader.SetHappiness();
                }
            }
            else if (Input.GetKeyDown(KeyCode.J)) {
                if (loader.CurrentMode != map_loader.MapMode.Population) {
                    loader.SetPopulation();
                }
            }
            else if (Input.GetKeyDown(KeyCode.K)) {
                if (loader.CurrentMode != map_loader.MapMode.Political) {
                    loader.SetPolitical();
                }
            }
            else if (Input.GetKeyDown(KeyCode.L)) { 
                if(loader.CurrentMode != map_loader.MapMode.Diplomatic) {
                    loader.SetDiplomatic();
                }
            }
        
    }
}
