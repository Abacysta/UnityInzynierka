using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mosframe;

public class save_list_manager : MonoBehaviour
{
    [SerializeField] private DynamicVScrollView dynamic_vscroll_saves_view;
    [SerializeField] private game_manager game_manager;

    public List<string> Saves { get; set; }

    void Start()
    {
        SetData();
    }

    void OnEnable()
    {
        SetData();
    }

    private void SetData()
    {
        Saves = new List<string>();
        for (int i = 1; i <= 40; i++)
        {
            Saves.Add($"Save {i} - 2024-10-{i:D2}");
        }

        //Saves = game_manager.getSaveGames().ToList();
        dynamic_vscroll_saves_view.totalItemCount = Saves.Count;
    }

    private void DisplayList()
    {
        dynamic_vscroll_saves_view.refresh();
    }
}