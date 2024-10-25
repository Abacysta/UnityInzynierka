using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mosframe;
using System;
using TMPro;
using UnityEngine.UI;

public class save_list_manager : MonoBehaviour
{
    [SerializeField] private DynamicVScrollView dynamic_vscroll_saves_view;
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_InputField save_name;
    [SerializeField] private game_manager game_manager;
    [SerializeField] private Button save_button, load_button, del_button;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private Button exit;

    public string[] Saves { get; set; }

    void Start()
    {
        SetData();
        del_button.onClick.AddListener(() => { delNamedGame(); });
        save_button.onClick.AddListener(() => { saveNamedGame();  });
        load_button.onClick.AddListener(()=>{ loadNamedGame();  });
    }

    void OnEnable()
    {
        overlay.SetActive(true);
        SetData();
    }

	private void OnDisable() {
		overlay.SetActive(false);
	}

	private void Update() {
        overlay.SetActive(true);
		if (save_name != null && Saves.Contains(save_name.text)) {
            load_button.gameObject.SetActive(true);
            del_button.gameObject.SetActive(true);
        } else {
            load_button.gameObject.SetActive(false);
            del_button.gameObject.SetActive(false);
        }
        if (save_name.text != null & save_name.text != "") {
            save_button.gameObject.SetActive(true);
        }
        else save_button.gameObject.SetActive(false);
	}

	public void SetData()
    {
        //Saves = new string[3] { "Save1", "Saev2", "Save3" };
        Saves = game_manager.getSaveGames();


        //Saves = game_manager.getSaveGames().ToList();
        dynamic_vscroll_saves_view.totalItemCount = Saves.Length;
        DisplayList();
    }

    private void DisplayList()
    {
        dynamic_vscroll_saves_view.refresh();
    }

    public void setSaveName(string name) {
        save_name.text = name;
    }

    public void saveNamedGame() {
        dialog_box.invokeConfirmBox("Save Game", "Are you sure you want to save game under " + save_name.text + (game_manager.existsSaveGame(save_name.text) ? "?\nAlready existing data will be overwritten!" : ""), ()=> { game_manager.saveGame(save_name.text); exit.onClick.Invoke(); }, null, null);
    }

    public void delNamedGame() {
        dialog_box.invokeConfirmBox("Delete Save", "Are you sure you want to delete this savefile?\n" + save_name.text, () => { game_manager.deleteSaveGame(save_name.text); SetData(); }, null, null); 
    }
    public void loadNamedGame() {
        dialog_box.invokeConfirmBox("Load Save", "Are you sure you want to load this savefile?\n" + save_name.text, () => { game_manager.loadGame(save_name.text); exit.onClick.Invoke(); }, null, null);
    }
}