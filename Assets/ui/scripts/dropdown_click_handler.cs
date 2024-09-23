using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class dropdown_click_handler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private diplomatic_actions_manager diplomatic_actions_manager;

    public void OnPointerClick(PointerEventData eventData)
    {
        diplomatic_actions_manager.UpdateDropdownImagesColor();
    }
}
