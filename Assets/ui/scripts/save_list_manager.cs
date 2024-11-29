using System.Linq;
using UnityEngine;
using Mosframe;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class save_list_manager : MonoBehaviour
{
    [SerializeField] private DynamicVScrollView dynamic_vscroll_saves_view;
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_InputField save_name;
    [SerializeField] private Button save_button, load_button, del_button;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private Button exit;
    [SerializeField] private save_manager save_manager;

    private Boolean isGameMap = false;

    public string[] Saves { get; set; }

    void Start()
    {
        del_button.onClick.AddListener(() => { delNamedGame(); });
        load_button.onClick.AddListener(() => { loadNamedGame(); });

        if (SceneManager.GetActiveScene().name == "game_map")
        {
            save_button.onClick.AddListener(() => { saveNamedGame(); });
            isGameMap = true;
        }
        else
        {
            save_button.gameObject.SetActive(false);
        }
        SetData();
    }

    void OnEnable()
    {
        if (isGameMap) overlay.SetActive(true);
        SetData();
    }

	private void OnDisable() {
        if (isGameMap) overlay.SetActive(false);
	}

	private void Update() {
        if (isGameMap) overlay.SetActive(true);
		if (save_name != null && Saves.Contains(save_name.text)) {
            load_button.gameObject.SetActive(true);
            del_button.gameObject.SetActive(true);
        } else {
            load_button.gameObject.SetActive(false);
            del_button.gameObject.SetActive(false);
        }
        if (save_name.text != null & save_name.text != "" && isGameMap) {
            save_button.gameObject.SetActive(true);
        }
        else save_button.gameObject.SetActive(false);
	}

	public void SetData()
    {
        Saves = save_manager.getSaveGames();
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
        dialog_box.invokeConfirmBox("Save Game", "Are you sure you want to save game under " + save_name.text 
            + (save_manager.existsSaveGame(save_name.text) ? "?\nAlready existing data will be overwritten!" : ""), 
            ()=> { save_manager.saveGame(save_name.text); exit.onClick.Invoke(); }, null, null);
    }

    public void delNamedGame() {
        dialog_box.invokeConfirmBox("Delete Save", "Are you sure you want to delete this savefile?\n" + save_name.text, 
            () => { save_manager.deleteSaveGame(save_name.text); SetData(); }, null, null); 
    }
    public void loadNamedGame() {
        dialog_box.invokeConfirmBox("Load Save", "Are you sure you want to load this savefile?\n" + save_name.text, 
            () => { save_manager.loadGame(save_name.text, isGameMap); if (isGameMap) exit.onClick.Invoke(); }, null, null);
    }
}