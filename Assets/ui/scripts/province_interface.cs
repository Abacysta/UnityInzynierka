using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    public TMP_Text id, res, happ, pop, rec_pop, occupator, occupation_count;
    public Map map;
    private int prov;
    [SerializeField] private buildings_interface buildings_Interface;

    private void Start() { 
        id.SetText("null");
        res.SetText("null");
        happ.SetText("null");
        pop.SetText("null");
        rec_pop.SetText("null");
        occupator.SetText("null");
        occupation_count.SetText("null");

        buildings_Interface.Initialize(map);
    }

    private void Update() {
        var coordinates = map.Selected_province;
        prov = map.getProvinceIndex(coordinates);
        id.SetText("id:" + map.Provinces[prov].X + '_' + map.Provinces[prov].Y);
        res.SetText("" + map.Provinces[prov].Resources);
        happ.SetText(""+ map.Provinces[prov].Happiness);
        pop.SetText("" + map.Provinces[prov].Population);
        rec_pop.SetText(""+map.Provinces[prov].RecruitablePopulation);
        occupator.SetText(""+map.Provinces[prov].OccupationInfo.OccupyingCountryId);
        occupation_count.SetText(""+map.Provinces[prov].OccupationInfo.OccupationCount);
    }

    public void PopulationIncrease(int val) {
        map.Provinces[prov].Population += val;
    }
    public void addOccupation()
    {
        map.Provinces[prov].OccupationInfo = new OccupationInfo(true,3,69);
    }
    public void removeOccupation()
    {
        map.Provinces[prov].OccupationInfo.IsOccupied = false;
        map.Provinces[prov].OccupationInfo.OccupationCount = 0;
        map.Provinces[prov].OccupationInfo.OccupyingCountryId = 0;
    }

}
