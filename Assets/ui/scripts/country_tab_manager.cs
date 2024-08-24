using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class country_tab_manager : MonoBehaviour
{
    [SerializeField] private GameObject[] tab_buttons;
    [SerializeField] private GameObject[] tab_panels;

    [SerializeField] Color activeColor = new Color32(92, 92, 92, 255);
    [SerializeField] Color inactiveColor = new Color32(77, 77, 77, 255);

    private int activeTab = 0;

    void Start()
    {
        ActivateTab(activeTab);

        for (int i = 0; i < tab_buttons.Length; i++)
        {
            int index = i;
            tab_buttons[i].GetComponent<Button>().onClick.AddListener(() => ActivateTab(index));
        }
    }

    public void ActivateTab(int index)
    {
        for (int i = 0; i < tab_buttons.Length; i++)
        {
            tab_panels[i].SetActive(false);
            tab_buttons[i].GetComponent<Image>().color = inactiveColor;
        }

        tab_panels[index].SetActive(true);
        tab_buttons[index].GetComponent<Image>().color = activeColor;

        activeTab = index;
        Canvas.ForceUpdateCanvases();
    }
}
