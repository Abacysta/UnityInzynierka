using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    public TMP_Text id, res, happ, pop, rec_pop;
    public Map map;
    private int prov;

    private void Start() { 
        id.SetText("null");
        res.SetText("null");
        happ.SetText("null");
        pop.SetText("null");
        rec_pop.SetText("null");
    }

    private void Update() {
        var coordinates = map.Selected_province;
        prov = map.getProvinceIndex(coordinates);
        id.SetText("id:" + map.Provinces[prov].X + '_' + map.Provinces[prov].Y);
        res.SetText("" + map.Provinces[prov].Resources);
        happ.SetText(""+ map.Provinces[prov].Happiness);
        pop.SetText("" + map.Provinces[prov].Population);
        rec_pop.SetText(""+map.Provinces[prov].RecruitablePopulation);
    }

    public void PopulationIncrease(int val) {
        map.Provinces[prov].Population += val;
    }

}
