using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    private class ProvinceInfoField 
    {
        private TMP_Text txt;
        private Image img;

        public ProvinceInfoField(GameObject obj) {
            this.txt = obj.GetComponentInChildren<TMP_Text>();
            this.img = obj.GetComponentInChildren<Image>();
        }

        public TMP_Text Txt { get => txt; set => txt = value; }
        public Image Img { get => img; set => img = value; }
    }

    [SerializeField] private GameObject id, type, res, happ, pop, rec_pop, building_interface;
    [SerializeField] private Map map;
    [SerializeField] private Image b_1, b_2, b_3, b_4;
    [SerializeField] private Sprite[] b_1_spr, b_2_spr, b_3_spr, b_4_spr;
    [SerializeField] private Transform b_1_m, b_2_m, b_3_m, b_4_m;
    [SerializeField] private dialog_box_manager dialog_box;
    //[SerializeField] private buildings_interface buildings_Interface;

    private ProvinceInfoField id_, type_, res_, happ_, pop_, rec_pop_;
    private int prov;

    private void Start() {
        id_ = new ProvinceInfoField(id);
        type_ = new ProvinceInfoField(type);
        res_ = new ProvinceInfoField(res);
        happ_ = new ProvinceInfoField(happ);
        pop_ = new ProvinceInfoField(pop);
        rec_pop_ = new ProvinceInfoField (rec_pop);
        
        id_.Txt.SetText("null");
        type_.Txt.SetText("null");
        res_.Txt.SetText("null");
        happ_.Txt.SetText("null");
        pop_.Txt.SetText("null");
        rec_pop_.Txt.SetText("null");

        foreach(var bt in new List<(Transform, BuildingType)> { 
                                    (b_1_m, BuildingType.Infrastructure), 
                                    (b_2_m, BuildingType.Fort), 
                                    (b_3_m, BuildingType.School), 
                                    (b_4_m, BuildingType.Mine) 
                                }) {
            bt.Item1.Find("add").GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeUpgradeBuilding(map, map.Selected_province, bt.Item2));
            bt.Item1.Find("remove").GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeDowngradeBuilding(map, map.Selected_province, bt.Item2));
        }
        //buildings_Interface.Initialize(map);
    }

    private void Update() {
        var coordinates = map.Selected_province;
        Province p = map.getProvince(coordinates);
        id_.Txt.SetText (coordinates.ToString()+ (p.Owner_id!=0 ? " " + map.Countries[p.Owner_id].Name : ""));
        type_.Txt.SetText (""+p.Type);

        if(p.Type == "land") {
            res.SetActive(true);
            happ.SetActive(true);
            pop.SetActive(true);
            rec_pop.SetActive(true);
            building_interface.SetActive(true);

            res_.Txt.SetText("" + p.Resources);
            happ_.Txt.SetText("" + p.Happiness);
            pop_.Txt.SetText("" + p.Population);
            rec_pop_.Txt.SetText("" + p.RecruitablePopulation);

            var bld = new List<Building> { 
                p.Buildings.Find(b => b.BuildingType == BuildingType.Infrastructure), 
                p.Buildings.Find(b => b.BuildingType == BuildingType.Fort), 
                p.Buildings.Find(b => b.BuildingType == BuildingType.School), 
                p.Buildings.Find(b => b.BuildingType == BuildingType.Mine) 
            };
            
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


