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
        del_button.onClick.AddListener(() => { DelNamedGame(); });
        load_button.onClick.AddListener(() => { LoadNamedGame(); });

        if (SceneManager.GetActiveScene().name == "game_map")
        {
            save_button.onClick.AddListener(() => { SaveNamedGame(); });
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
        if (overlay != null) overlay.SetActive(true);
        SetData();
    }

	private void OnDisable() {
        if (overlay != null) overlay.SetActive(false);
	}

	private void Update() {
        if (overlay != null) overlay.SetActive(true);
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
        Saves = save_manager.GetSaveGames();
        dynamic_vscroll_saves_view.totalItemCount = Saves.Length;
        DisplayList();
    }

    private void DisplayList()
    {
        dynamic_vscroll_saves_view.refresh();
    }

    public void SetSaveName(string name) {
        save_name.text = name;
    }

    private void LoadGame()
    {
        if (isGameMap)
        {
            save_manager.LoadGame(save_name.text);
        }
        else
        {
            PlayerPrefs.SetString("saveName", save_name.text);
            SceneManager.LoadScene("game_map");
        }
    }

    public void SaveNamedGame() {
        dialog_box.InvokeConfirmBox("Save Game", "Are you sure you want to save game under " + save_name.text 
            + (save_manager.ExistsSaveGame(save_name.text) ? "?\nAlready existing data will be overwritten!" : ""), 
            ()=> { save_manager.SaveGame(save_name.text); exit.onClick.Invoke(); });
    }

    public void DelNamedGame() {
        dialog_box.InvokeConfirmBox("Delete Save", "Are you sure you want to delete this savefile?\n" + save_name.text, 
            () => { save_manager.DeleteSaveGame(save_name.text); SetData(); }); 
    }
    public void LoadNamedGame() {
        dialog_box.InvokeConfirmBox("Load Save", "Are you sure you want to load this savefile?\n" + save_name.text, 
            () => { LoadGame(); if (isGameMap) exit.onClick.Invoke(); });
    }
}