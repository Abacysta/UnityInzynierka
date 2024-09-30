using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class country_info : MonoBehaviour
{
    public GameObject info_cont;
    public Map map;
    private TMP_Text gold, wood, iron, tech, ap;
    [SerializeField] private GameObject main_button;
    [SerializeField] private country_interface_manager country_interface;
    [SerializeField] private province_interface province_interface;
    // Start is called before the first frame update
    void Start()
    {
        gold = info_cont.transform.Find("gold").GetComponentInChildren<TMP_Text>();
        wood = info_cont.transform.Find("wood").GetComponentInChildren<TMP_Text>();
        iron = info_cont.transform.Find("iron").GetComponentInChildren<TMP_Text>();
        tech = info_cont.transform.Find("tech").GetComponentInChildren<TMP_Text>();
        ap = info_cont.transform.Find("ap").GetComponentInChildren<TMP_Text>();
        displayInfo();
        InvokeRepeating("displayInfo", 0.5f, 0.15f);
    }

    public void displayInfo() {
        main_button.GetComponent<Image>().color = map.CurrentPlayer.Color;
        main_button.GetComponent<Button>().onClick.AddListener(() => country_interface.ShowCountryInterface());
        main_button.GetComponent<Button>().onClick.AddListener(() => province_interface.hide());
        gold.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.Gold], 1));
        wood.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.Wood],1));
        iron.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.Iron],1));
        tech.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.SciencePoint], 1));
        ap.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.AP], 1));
    }
}
