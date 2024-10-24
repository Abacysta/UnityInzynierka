using Mosframe;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class dynamic_scoll_view_save_row : UIBehaviour, IDynamicScrollViewItem
{
    [SerializeField] private TMP_Text save_row_text;
    [SerializeField] private save_list_manager save_list_manager;

    public void onUpdateItem(int index)
    {
        string save = save_list_manager.Saves[index];
        save_row_text.text = save;
    }

    public void OnSaveRowClick()
    {
        Debug.Log(save_row_text.text);
        save_list_manager.setSaveName(save_row_text.text);
    }
}