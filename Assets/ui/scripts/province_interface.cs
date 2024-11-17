using Assets.classes.subclasses;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    private class StatusDisplay
    {
        public static void deleteIcons(GameObject obj)
        {
            foreach (Transform child in obj.transform)
            {
                Destroy(child.gameObject);
            }
        }
        public static void showIcons(GameObject obj, List<Status> statuses, List<Sprite> status_sprites)
        {
            deleteIcons(obj);

            if (statuses != null)
            {
                RectTransform rect = obj.GetComponent<RectTransform>();
                float height = rect.rect.height, currentX = 0f;
                foreach (Status status in statuses)
                {
                    GameObject child = new GameObject("icon_" + status.Id);
                    child.transform.SetParent(obj.transform);
                    Image image = child.AddComponent<Image>();
                    image.sprite = status_sprites[status.Id];
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

        public ProvinceInfoField(GameObject obj)
        {
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
    [SerializeField] private List<Sprite> res_images; //gold, wood, iron, tech, ap
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private country_interface_manager country_interface;
    [SerializeField] private diplomatic_actions_manager diplomatic_interface;
    [SerializeField] private GameObject statuses_list;
    [SerializeField] private List<Sprite> status_sprites;
    [SerializeField] private GameObject recruitment_button;
    [SerializeField] private GameObject emblem;
    [SerializeField] private GameObject festivities_button;
    [SerializeField] private GameObject tax_break_button;
    [SerializeField] private GameObject rebel_suppress_button;

    private ProvinceInfoField id_, type_, res_, happ_, pop_, rec_pop_;
    private int prov;

    public (int, int) SelectedProvince { get; set; }
    public bool Recruitable { get { return map.getProvince(SelectedProvince).RecruitablePopulation > 0 && recruitment_button.activeSelf; } }

    private void Start()
    {
        InitializeInfoFields();
        ResetTextFields();
        InitializeButtons();
    }

    private void InitializeInfoFields()
    {
        id_ = new ProvinceInfoField(id);
        type_ = new ProvinceInfoField(type);
        res_ = new ProvinceInfoField(res);
        happ_ = new ProvinceInfoField(happ);
        pop_ = new ProvinceInfoField(pop);
        rec_pop_ = new ProvinceInfoField(rec_pop);
    }

    private void ResetTextFields()
    {
        id_.Txt.SetText("null");
        type_.Txt.SetText("null");
        res_.Txt.SetText("null");
        happ_.Txt.SetText("null");
        pop_.Txt.SetText("null");
        rec_pop_.Txt.SetText("null");
    }

    private void InitializeButtons()
    {
        var buildingTypes = new List<(Transform, BuildingType)> {
            (b_1_m, BuildingType.Infrastructure),
            (b_2_m, BuildingType.Fort),
            (b_3_m, BuildingType.School),
            (b_4_m, BuildingType.Mine)
        };

        foreach (var (btnTransform, buildingType) in buildingTypes)
        {
            btnTransform.Find("add").GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeUpgradeBuilding(map, SelectedProvince, buildingType));
            btnTransform.Find("remove").GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeDowngradeBuilding(map, SelectedProvince, buildingType));
        }

        recruitment_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeRecBox(map, SelectedProvince));
        festivities_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeFestivitiesOrganizationBox(map, SelectedProvince));
        tax_break_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeTaxBreakIntroductionBox(map, SelectedProvince));
        rebel_suppress_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeRebelSuppressionBox(map, SelectedProvince));
    }

    private void Update() {
        var coordinates = SelectedProvince;
        Province p = map.getProvince(coordinates);

        id_.Txt.SetText (coordinates.ToString() + (p.Owner_id != 0 ? " " + map.Countries[p.Owner_id].Name : ""));
        type_.Txt.SetText($"{p.Type}");

        if (p.Type == "land") 
        {
            res_.Txt.SetText("" + p.ResourcesP + "(" + p.Resources_amount + ")");
            res_.Img.sprite = res_images[((int)p.ResourcesT)];
            happ_.Txt.SetText("" + p.Happiness);
            pop_.Txt.SetText("" + p.Population);
            rec_pop_.Txt.SetText("" + p.RecruitablePopulation);

            UpdateUIElementStates(p); 
            UpdateEmblem(p.Owner_id);
            StatusDisplay.showIcons(statuses_list, p.Statuses, status_sprites);
        }
        else {
            res.SetActive(false);
            happ.SetActive(false);
            pop.SetActive(false);
            rec_pop.SetActive(false);

            building_interface.SetActive(false);
            recruitment_button.SetActive(false);
            festivities_button.SetActive(false);
            tax_break_button.SetActive(false);
            rebel_suppress_button.SetActive(false);
        }
    }

    private void UpdateUIElementStates(Province p)
    {
        res.SetActive(true);
        happ.SetActive(true);
        pop.SetActive(true);
        rec_pop.SetActive(true);

        building_interface.SetActive(p.Owner_id == map.CurrentPlayer.Id);
        recruitment_button.SetActive(p.Owner_id == map.CurrentPlayer.Id);
        festivities_button.SetActive(p.Owner_id == map.CurrentPlayer.Id);
        tax_break_button.SetActive(p.Owner_id == map.CurrentPlayer.Id);
        rebel_suppress_button.SetActive(p.Owner_id == map.CurrentPlayer.Id);

        UpdateBuildings(p);
        recruitment_button.GetComponent<Button>().interactable = p.RecruitablePopulation > 0;
        festivities_button.GetComponent<Button>().interactable = map.CurrentPlayer.techStats.canFestival && !p.Statuses.Any(status => status is Festivities);
        tax_break_button.GetComponent<Button>().interactable = map.CurrentPlayer.techStats.canTaxBreak && !p.Statuses.Any(status => status is TaxBreak);
        rebel_suppress_button.GetComponent<Button>().interactable =
            map.CurrentPlayer.techStats.canRebelSupp &&
            map.Armies.Any(a => a.Position == p.coordinates && a.OwnerId == 0);
    }

    private void UpdateBuildings(Province province)
    {
        var buildings = new List<Building> {
            province.Buildings.Find(b => b.BuildingType == BuildingType.Infrastructure),
            province.Buildings.Find(b => b.BuildingType == BuildingType.Fort),
            province.Buildings.Find(b => b.BuildingType == BuildingType.School),
            province.Buildings.Find(b => b.BuildingType == BuildingType.Mine)
        };

        b_1.sprite = b_1_spr[buildings[0].BuildingLevel];
        b_2.sprite = b_2_spr[buildings[1].BuildingLevel];
        b_3.sprite = b_3_spr[buildings[2].BuildingLevel];
        b_4.sprite = b_4_spr[buildings[3].BuildingLevel];

        UpdateBuildingButtonsStates(buildings, province.Owner_id);
    }

    private void UpdateBuildingButtonsStates(List<Building> buildings, int ownerId)
    {
        if (map.CurrentPlayer.Id != ownerId) return;

        var buttonMappings = new List<(Transform ButtonTransform, BuildingType Type, int BuildingIndex)>
        {
            (b_1_m, BuildingType.Infrastructure, 0),
            (b_2_m, BuildingType.Fort, 1),
            (b_3_m, BuildingType.School, 2),
            (b_4_m, BuildingType.Mine, 3)
        };

        foreach (var (buttonTransform, buildingType, buildingIndex) in buttonMappings)
        {
            int buildingLevel = buildings[buildingIndex].BuildingLevel;
            int maxTechLevel = GetMaxTechLevel(buildingType);

            buttonTransform.Find("add").GetComponent<Button>().interactable = buildingLevel < maxTechLevel;
            buttonTransform.Find("remove").GetComponent<Button>().interactable = buildingLevel > 0 && buildingLevel < 4;
        }
    }

    private int GetMaxTechLevel(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.Infrastructure:
                return map.CurrentPlayer.techStats.canInfrastructure ? 3 : 0;
            case BuildingType.School:
                return map.CurrentPlayer.techStats.moreSchool ? 3 : 1;
            case BuildingType.Fort:
                return map.CurrentPlayer.techStats.lvlFort;
            case BuildingType.Mine:
                return map.CurrentPlayer.techStats.lvlMine;
            default:
                return 3;
        }
    }

    private void UpdateEmblem(int ownerId)
    {
        if (ownerId == 0) emblem.SetActive(false);
        else
        {
            map.Countries[ownerId].setCoatandColor(emblem);
            emblem.SetActive(true);
        }

        emblem.GetComponent<Button>().onClick.RemoveAllListeners();
        if (map.CurrentPlayer.Id == ownerId)
        {
            emblem.GetComponent<Button>().onClick.AddListener(() => country_interface.ShowCountryInterface());
        }
        else
        {
            emblem.GetComponent<Button>().onClick.AddListener(() => diplomatic_interface.ShowDiplomaticActionsInterface(ownerId));
        }
        emblem.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));
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