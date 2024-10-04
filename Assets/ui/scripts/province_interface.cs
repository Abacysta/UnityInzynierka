using Assets.classes.subclasses;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    private class EffectDisplay {
        public static void deleteIcons(GameObject obj) {
            foreach(Transform child in obj.transform) {
                Destroy(child.gameObject);
            }
        }
        public static void showIcons(GameObject obj, List<Status> statuses, List<Sprite> status_sprites) { 
            deleteIcons(obj);

            if(statuses!=null){
                RectTransform rect = obj.GetComponent<RectTransform>();
                float height = rect.rect.height, currentX = 0f;
                foreach(Status status in statuses) {
                    GameObject child = new GameObject("icon_" + status.id);
                    child.transform.SetParent(obj.transform);
                    Image image = child.AddComponent<Image>();
                    image.sprite = status_sprites[status.id];
                    RectTransform rectT = child.GetComponent<RectTransform>();
                    rectT.sizeDelta = new Vector2(height, height);
                    rectT.anchorMin = new Vector2(0, 0.5f);
                    rectT.anchorMax = new Vector2(0, 0.5f);
                    rectT.pivot = new Vector2(0, 0.5f);

                    rectT.anchoredPosition = new Vector2(currentX, 0);
                    currentX += height;
                } 
            }
        }
    }
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
    [SerializeField] private List<Sprite> res_images;//gold,wood,iron,tech,ap
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private country_interface_manager country_interface;
    [SerializeField] private diplomatic_actions_manager diplomatic_interface;
    [SerializeField] private GameObject statuses_list;
    [SerializeField] private List<Sprite> status_sprites;
    [SerializeField] private GameObject recruitment_button;
    [SerializeField] private GameObject emblem;
    //[SerializeField] private buildings_interface buildings_Interface;

    private ProvinceInfoField id_, type_, res_, happ_, pop_, rec_pop_;
    private int prov;

    public bool Recruitable{ get { return map.getProvince(map.Selected_province).RecruitablePopulation > 0 && recruitment_button.activeSelf; } }
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
        recruitment_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeRecBox(map, map.Selected_province));
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
            res_.Txt.SetText("" + p.ResourcesP + "(" + p.Resources_amount + ")");
            res_.Img.sprite = res_images[((int)p.ResourcesT)];
            happ_.Txt.SetText("" + p.Happiness);
            pop_.Txt.SetText("" + p.Population);
            rec_pop_.Txt.SetText("" + p.RecruitablePopulation);

            building_interface.SetActive(p.Owner_id == map.CurrentPlayer.Id);
            recruitment_button.SetActive(p.Owner_id == map.CurrentPlayer.Id);
            recruitment_button.GetComponent<Button>().interactable = p.RecruitablePopulation > 0;

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
            if (p.Owner_id == 0) emblem.SetActive(false);
            else emblem.SetActive(true);
            if(map.CurrentPlayer.Id == p.Owner_id) foreach(var bt in new List<(Transform, int)> { (b_1_m, 0), (b_2_m, 1), (b_3_m, 2), (b_4_m, 3)}) {
                bt.Item1.Find("add").GetComponent<Button>().interactable = bld[bt.Item2].BuildingLevel < 3 ? true : false;
                bt.Item1.Find("remove").GetComponent<Button>().interactable = bld[bt.Item2].BuildingLevel > 0 && bld[bt.Item2].BuildingLevel < 4? true : false;
            }
            emblem.GetComponent<Image>().color = map.Countries[p.Owner_id].Color;
            if(map.CurrentPlayer.Id == p.Owner_id) {
                emblem.GetComponent<Button>().onClick.AddListener(() => country_interface.ShowCountryInterface());
            }
            else {
                emblem.GetComponent<Button>().onClick.AddListener(() => diplomatic_interface.ShowDiplomaticActionsInterface(p.Owner_id));
            }
            emblem.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));
            EffectDisplay.showIcons(statuses_list, p.Statuses, status_sprites);

        }
        else {
            res.SetActive(false);
            happ.SetActive (false );
            pop.SetActive(false );
            rec_pop .SetActive(false );
            building_interface.SetActive(false) ;
            recruitment_button.SetActive(false);
        }

    }

    public void PopulationIncrease(int val) {
        map.Provinces[prov].Population += val;
    }

    public void hide() {
        gameObject.SetActive(false);
    }

    public void recruit() {
        recruitment_button.GetComponent<Button>().onClick.Invoke();
    }
}


