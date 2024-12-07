using Mosframe;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class end_screen_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_pop_countries_view;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_gold_countries_view;
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text title_text;
    [SerializeField] private TMP_Text kill_reason_text;
    [SerializeField] private TMP_Text pop_text;
    [SerializeField] private TMP_Text happ_text;
    [SerializeField] private TMP_Text claim_text;
    [SerializeField] private Button kill_game_button;

    private List<Country> popCountries = new();
    private List<Country> goldCountries = new();

    public List<Country> PopCountries { get => popCountries; set => popCountries = value; }
    public List<Country> GoldCountries { get => goldCountries; set => goldCountries = value; }
    public Map Map { get => map; set => map = value; }

    private void Start()
    {
        SetPopulationRows();
        SetGoldRows();
    }

    void OnEnable()
    {
        overlay.SetActive(true);
        SetPopulationRows();
        SetGoldRows();
        setEverythingElse();
    }

    private void OnDisable()
    {
        overlay.SetActive(false);
        kill_game_button.onClick.RemoveAllListeners();
    }


    private void setEverythingElse() {
        kill_reason_text.fontSize = title_text.fontSize;
        if (map.TurnCnt >= map.Turnlimit) setTimeoutKill();
        else setDominationKill();
        pop_text.SetText(map.Provinces.Sum(p => p.Population).ToString());
        happ_text.SetText(((int)(map.Provinces.Sum(p => p.Happiness) / map.Provinces.Count)).ToString());
        claim_text.SetText((int)((float)map.Provinces.Where(p => p.OwnerId == 0).Count() / (float)map.Provinces.Count * 100) + "%");
        kill_game_button.onClick.AddListener(() => SceneManager.LoadScene(0));
    }

    private void setTimeoutKill() {
        kill_reason_text.SetText("Timeout");
        kill_reason_text.color = new Color(0, 0.6f, 1);
    }

    private void setDominationKill() {
        kill_reason_text.SetText("Domination");
        kill_reason_text.color = new Color(1, 0.6f, 0);
    }

    private void SetPopulationRows()
    {
        PopCountries = map.Countries.Where(c => c.Id != 0).OrderByDescending(c => c.Provinces.Sum(p => p.Population)).ToList();
        dynamic_vscroll_pop_countries_view.totalItemCount = PopCountries.Count;
        dynamic_vscroll_pop_countries_view.refresh();
    }

    private void SetGoldRows()
    {
        GoldCountries = map.Countries.Where(c => c.Id != 0).OrderByDescending(c => c.Resources[Resource.Gold]).ToList();
        dynamic_vscroll_gold_countries_view.totalItemCount = GoldCountries.Count;
        dynamic_vscroll_gold_countries_view.refresh();
    }
}
