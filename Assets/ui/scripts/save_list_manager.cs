using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mosframe;
using System;
using TMPro;

public class save_list_manager : MonoBehaviour
{
    [SerializeField] private DynamicVScrollView dynamic_vscroll_saves_view;
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_InputField save_name;
    [SerializeField] private game_manager game_manager;

    public string[] Saves { get; set; }

    void Start()
    {
        SetData();
    }

    void OnEnable()
    {
        overlay.SetActive(true);
        SetData();
    }

	private void OnDisable() {
		overlay.SetActive(false);
	}

	private void SetData()
    {
        Saves = new string[3] { "Save1", "Saev2", "Save3" };
        //Saves = game_manager.getSaveGames();


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
}