using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class country_info : MonoBehaviour
{
    public GameObject info_cont;
    public Map map;
    private TMP_Text gold, wood, iron, tech, ap;
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
        gold.SetText("" + map.CurrentPlayer.Resources[Resource.Gold]);
        wood.SetText("" + map.CurrentPlayer.Resources[Resource.Wood]);
        iron.SetText("" + map.CurrentPlayer.Resources[Resource.Iron]);
        tech.SetText("" + map.CurrentPlayer.Resources[Resource.SciencePoint]);
        ap.SetText("" + map.CurrentPlayer.Resources[Resource.AP]);
    }
}
