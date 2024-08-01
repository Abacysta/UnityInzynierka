using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class province_interface : MonoBehaviour
{
    public TMP_Text id, res, type, pop;
    public Map map;
    private int prov;
    private void Start() {
        id.SetText("null");
        res.SetText("null");
        type.SetText("null");
        pop.SetText("null");
    }

    private void Update() {
        var coordinates = map.Selected_province;
        prov = map.Provinces.FindIndex(p => p.X == coordinates.Item1 && p.Y == coordinates.Item2);
        id.SetText("id:" + map.Provinces[prov].X + '_' + map.Provinces[prov].Y);
        //icons TBD
        res.SetText("resource:" + map.Provinces[prov].Resources);
        type.SetText(map.Provinces[prov].Type == "land" ? "land" : "sea");
        pop.SetText("population" + map.Provinces[prov].Population);
    }

    public void PopulationIncrease(int val) {
        map.Provinces[prov].Population += val;
    }
}
