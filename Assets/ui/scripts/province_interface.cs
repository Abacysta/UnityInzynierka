using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    private class provinceInfoField:MonoBehaviour {
        [SerializeField]
        public TMP_Text txt;
        [SerializeField]
        public Image img;

        public provinceInfoField(GameObject obj) {
            txt = obj.GetComponentInChildren<TMP_Text>();
            img = obj.GetComponentInChildren<Image>();
        }
    }


    public GameObject id, type, res, happ, pop, rec_pop, building_interface;
    private provinceInfoField id_, type_ , res_, happ_, pop_, rec_pop_;
    public Map map;
    private int prov;
    public Image b_1, b_2, b_3, b_4;
    public Sprite[] b_1_spr, b_2_spr, b_3_spr, b_4_spr;
    public Transform b_1_m, b_2_m, b_3_m, b_4_m;

    //[SerializeField] private buildings_interface buildings_Interface;

    private void Start() {
        id_ = new provinceInfoField(id);
        type_ = new provinceInfoField(type);
        res_ = new provinceInfoField(res);
        happ_ = new provinceInfoField(happ);
        pop_ = new provinceInfoField(pop);
        rec_pop_ = new provinceInfoField (rec_pop);
        
        id_.txt.SetText("null");
        type_.txt.SetText("null");
        res_.txt.SetText("null");
        happ_.txt.SetText("null");
        pop_.txt.SetText("null");
        rec_pop_.txt.SetText("null");

        foreach(var bt in new List<(Transform, BuildingType)> { (b_1_m, BuildingType.Infrastructure), (b_2_m, BuildingType.Fort), (b_3_m, BuildingType.School), (b_4_m, BuildingType.Mine) }) {
            bt.Item1.Find("add").GetComponent<Button>().onClick.AddListener(() => map.upgradeBuilding(map.Selected_province, bt.Item2));
            bt.Item1.Find("remove").GetComponent<Button>().onClick.AddListener(() => map.downgradeBuilding(map.Selected_province, bt.Item2));
        }
        //buildings_Interface.Initialize(map);
    }

    private void Update() {
        var coordinates = map.Selected_province;
        Province p = map.getProvince(coordinates);
        id_.txt.SetText (coordinates.ToString()+ (p.Owner_id!=0 ? " " + map.Countries[p.Owner_id].Name : ""));
        type_.txt.SetText (""+p.Type);

        if(p.Type == "land") {
            res.SetActive(true);
            happ.SetActive(true);
            pop.SetActive(true);
            rec_pop.SetActive(true);
            building_interface.SetActive(true);

            res_.txt.SetText("" + p.Resources);
            happ_.txt.SetText("" + p.Happiness);
            pop_.txt.SetText("" + p.Population);
            rec_pop_.txt.SetText("" + p.RecruitablePopulation);

            var bld = new List<Building> { p.Buildings.Find(b => b.BuildingType == BuildingType.Infrastructure), p.Buildings.Find(b => b.BuildingType == BuildingType.Fort), p.Buildings.Find(b => b.BuildingType == BuildingType.School), p.Buildings.Find(b => b.BuildingType == BuildingType.Mine) };
            
            b_1.sprite = b_1_spr[bld[0].BuildingLevel];
            b_2.sprite = b_2_spr[bld[1].BuildingLevel];
            b_3.sprite = b_3_spr[bld[2].BuildingLevel];
            b_4.sprite = b_4_spr[bld[3].BuildingLevel];

            foreach(var bt in new List<(Transform, int)> { (b_1_m, 0), (b_2_m, 1), (b_3_m, 2), (b_4_m, 3)}) {
                bt.Item1.Find("add").GetComponent<Button>().interactable = bld[bt.Item2].BuildingLevel < 3 ? true : false;
                bt.Item1.Find("remove").GetComponent<Button>().interactable = bld[bt.Item2].BuildingLevel > 0 && bld[bt.Item2].BuildingLevel < 4? true : false;
            }

        }
        else {
            res.SetActive(false);
            happ.SetActive (false );
            pop.SetActive(false );
            rec_pop .SetActive(false );
            building_interface.SetActive(false) ;
        }

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


