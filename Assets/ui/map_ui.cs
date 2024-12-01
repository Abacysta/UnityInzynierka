using Assets.ui.scripts;
using Newtonsoft.Json.Bson;
using System;
using TMPro;
using UnityEngine;

public class map_ui : MonoBehaviour
{
    [SerializeField] private settings_menu settings_menu_scr;
    [SerializeField] private GameObject settings_menu_ui;
    [SerializeField] private province_interface province_interface;
    [SerializeField] private GameObject dialog_box;
    [SerializeField] private dialog_box_manager box_manager;
    [SerializeField] private filter_modes loader;
    [SerializeField] private game_manager game_manager;
    [SerializeField] private country_interface_manager country_interface;
    [SerializeField] private start_screen start_screen;
    [SerializeField] private GameObject overlay;
	[SerializeField] private diplomatic_actions_manager diplomatic_actions_manager;
	[SerializeField] private save_manager save_manager;

    private void Start()
    {
		settings_menu_scr.SetInitialAudioSettings();
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(dialog_box.activeSelf) {
                box_manager.HideDialog();
            }
            else if (country_interface.gameObject.activeSelf) {
                country_interface.HideCountryInterface();
            }
            else if(province_interface.gameObject.activeSelf) {
                province_interface.gameObject.SetActive(false);
            }
            else {
                settings_menu_ui.SetActive(settings_menu_ui.activeSelf);
            }
        }
        if (!overlay.activeSelf) {
			if (Input.GetKeyDown(KeyCode.I)) {
				if (loader.CurrentMode != filter_modes.MapMode.Terrain) {
					loader.SetTerrain();
				}
			}
			if (Input.GetKeyDown(KeyCode.O)) {
				if (loader.CurrentMode != filter_modes.MapMode.Resource) {
					loader.SetResources();
				}
			}
			if (Input.GetKeyDown(KeyCode.P)) {
				if (loader.CurrentMode != filter_modes.MapMode.Happiness) {
					loader.SetHappiness();
				}
			}
			if (Input.GetKeyDown(KeyCode.J)) {
				if (loader.CurrentMode != filter_modes.MapMode.Population) {
					loader.SetPopulation();
				}
			}
			if (Input.GetKeyDown(KeyCode.K)) {
				if (loader.CurrentMode != filter_modes.MapMode.Political) {
					loader.SetPolitical();
				}
			}
			if (Input.GetKeyDown(KeyCode.L)) {
				if (loader.CurrentMode != filter_modes.MapMode.Diplomatic) {
					loader.SetDiplomatic();
				}
			}
			if (Input.GetKeyDown(KeyCode.Backspace)) {
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					game_manager.UndoAll();
				}
				else {
					game_manager.undoLast();
				}
			}
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
					game_manager.TurnSimulation();
				}
				else {
					game_manager.LocalTurnSimulation();
				}
			}
			if (!isBlocked) {
				if (Input.GetKeyDown(KeyCode.Q)) {
					if (country_interface.gameObject.activeSelf && country_interface.ActiveTab == 0) {
						country_interface.HideCountryInterface();
					}
					else if (!dialog_box.activeSelf) {
						country_interface.ShowCountryInterface();
						country_interface.ActivateTab(0);
					}
				}
				if (Input.GetKeyDown(KeyCode.W)) {
					if (country_interface.gameObject.activeSelf && country_interface.ActiveTab == 1) {
						country_interface.HideCountryInterface();
					}
					else if (!dialog_box.activeSelf) {
						country_interface.ShowCountryInterface();
						country_interface.ActivateTab(1);
					}
				}
				if (Input.GetKeyDown(KeyCode.E)) {
					if (country_interface.gameObject.activeSelf && country_interface.ActiveTab == 2) {
						country_interface.HideCountryInterface();
					}
					else if (!dialog_box.activeSelf) {
						country_interface.ShowCountryInterface();
						country_interface.ActivateTab(2);
					}
				}
				if (Input.GetKeyDown(KeyCode.R)) {
					if (country_interface.gameObject.activeSelf && country_interface.ActiveTab == 3) {
						country_interface.HideCountryInterface();
					}
					else if (!dialog_box.activeSelf) {
						country_interface.ShowCountryInterface();
						country_interface.ActivateTab(3);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.F)) {
				if (province_interface.gameObject.activeSelf && province_interface.Recruitable) {
					province_interface.recruit();
				}
			}
			//ctrl actions
			if (Input.GetKeyDown(KeyCode.S) && game_manager.turnCnt > 0) {
				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
					save_manager.saveGame("quicksave");
				}
			}
		}
    }

    public bool isBlocked { get { return !(settings_menu_ui.activeSelf || dialog_box.activeSelf || country_interface.gameObject.activeSelf || start_screen.gameObject.activeSelf); } }

    public void DeactivateInterfaces()
    {
        country_interface.gameObject.SetActive(false);
        province_interface.gameObject.SetActive(false);
        diplomatic_actions_manager.gameObject.SetActive(false);
    }
}
