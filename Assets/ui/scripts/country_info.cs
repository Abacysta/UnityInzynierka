using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class country_info : MonoBehaviour
{
    public GameObject info_cont;
    public Map map;
    private TMP_Text gold, wood, iron, tech, ap;
    private TMP_Text goldG, woodG, ironG, techG, apG;
    [SerializeField] private GameObject main_button;
    [SerializeField] private country_interface_manager country_interface;
    [SerializeField] private province_interface province_interface;
    [SerializeField] game_manager game_manager;

    void Start()
    {
        gold = info_cont.transform.Find("gold").Find("txt").GetComponent<TMP_Text>();
        wood = info_cont.transform.Find("wood").Find("txt").GetComponent<TMP_Text>();
        iron = info_cont.transform.Find("iron").Find("txt").GetComponent<TMP_Text>();
        tech = info_cont.transform.Find("tech").Find("txt").GetComponent<TMP_Text>();
        ap = info_cont.transform.Find("ap").Find("txt").GetComponent<TMP_Text>();

        goldG = info_cont.transform.Find("gold").Find("gain").GetComponent<TMP_Text>();
        woodG = info_cont.transform.Find("wood").Find("gain").GetComponent<TMP_Text>();
        ironG = info_cont.transform.Find("iron").Find("gain").GetComponent<TMP_Text>();
        techG = info_cont.transform.Find("tech").Find("gain").GetComponent<TMP_Text>();
        apG = info_cont.transform.Find("ap").Find("gain").GetComponent<TMP_Text>();

        displayInfo();
        InvokeRepeating("displayInfo", 0.5f, 0.5f);
    }

    public void displayInfo() {
        var gains = Map.PowerUtilites.getGain(map, map.CurrentPlayer);

        main_button.GetComponent<Image>().color = map.CurrentPlayer.Color;
        main_button.GetComponent<Button>().onClick.AddListener(() => country_interface.ShowCountryInterface());
        main_button.GetComponent<Button>().onClick.AddListener(() => province_interface.hide());

        gold.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.Gold], 1));
        wood.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.Wood],1));
        iron.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.Iron],1));
        tech.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.SciencePoint], 1));
        ap.SetText("" + Math.Round(map.CurrentPlayer.Resources[Resource.AP], 1));

        goldG.SetText((gains[Resource.Gold] >= 0 ? "+" : "") + Math.Round(gains[Resource.Gold], 1));
        woodG.SetText((gains[Resource.Wood] >= 0 ? "+" : "") + Math.Round(gains[Resource.Wood], 1));
        ironG.SetText((gains[Resource.Iron] >= 0 ? "+" : "") + Math.Round(gains[Resource.Iron], 1));
        techG.SetText((gains[Resource.SciencePoint] >= 0 ? "+" : "") + Math.Round(gains[Resource.SciencePoint], 1));
        apG.SetText((gains[Resource.AP] >= 0 ? "+" : "") + Math.Round(gains[Resource.AP], 1));

        map.CurrentPlayer.setCoatandColor(transform.Find("country_button").GetComponent<Image>());
    }
}
