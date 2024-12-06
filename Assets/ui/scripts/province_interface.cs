using Assets.classes.subclasses;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    private class StatusDisplay
    {
        private static List<Status> previousStatuses = new();
        private static int previousTurn = 0;

        private static string FormatPercentChangeText(string label, float value)
        {
            string color = value > 0 ? "green" : value < 0 ? "red" : "yellow";
            string formattedValue = value > 0 ? $"+{value * 100}%" : value < 0 ? $"{value * 100}%" : "0";
            return $"� {label}: <color={color}>{formattedValue}</color>";
        }

        private static string FormatValueChangeText(string label, float value)
        {
            string color = value > 0 ? "green" : value < 0 ? "red" : "yellow";
            string formattedValue = value > 0 ? $"+{value}" : value < 0 ? $"{value}" : "0";
            return $"� {label}: <color={color}>{formattedValue}</color>";
        }

        private static string GenerateTooltipText(Status status, Province province, Map map)
        {
            string tooltipText = $"{status.Description}\n\n";

            tooltipText += "<align=left>";

            tooltipText += (status.Duration > 0) ? $"Duration: {status.Duration}\n\n" : "";

            switch (status.Id)
            {
                case 1: // TaxBreak
                    tooltipText += FormatPercentChangeText("Happiness growth", TaxBreak.HappMod);
                    tooltipText += "\n" + FormatValueChangeText("Happiness", TaxBreak.HappStatic);
                    break;
                case 2: // Festivities
                    tooltipText += FormatPercentChangeText("Production", Festivities.ProdMod);
                    tooltipText += "\n" + FormatPercentChangeText("Population growth", Festivities.PopMod);
                    tooltipText += "\n" + FormatValueChangeText("Happiness", Festivities.HappStatic);
                    break;
                case 3: // ProdBoom
                    tooltipText += FormatPercentChangeText("Production", ProdBoom.ProdMod);
                    break;
                case 4: // ProdDown
                    tooltipText += FormatPercentChangeText("Production", ProdDown.ProdMod);
                    break;
                case 5: // Illness
                    tooltipText += FormatPercentChangeText("Population growth", Illness.PopMod);
                    tooltipText += "\n" + FormatValueChangeText("Population", -province.Population / Illness.PopulationDivisor);
                    tooltipText += "\n" + FormatValueChangeText("Happiness", Illness.HappStatic);
                    break;
                case 6: // Disaster
                    tooltipText += FormatPercentChangeText("Population growth", Disaster.PopMod);
                    tooltipText += "\n" + FormatPercentChangeText("Production", Disaster.ProdMod);
                    break;
                case 7: // Occupation
                    tooltipText += $"Occupying country: {map.Countries[province.OccupationInfo.OccupyingCountryId].Name}\n";
                    break;
                case 8: // RecBoom
                    tooltipText += FormatPercentChangeText("Recruitable population", RecBoom.RecPop);
                    tooltipText += "\n" + FormatPercentChangeText("Production", RecBoom.ProdMod);
                    break;
                case 9: // FloodStatus
                    tooltipText += FormatPercentChangeText("Recruitable population", FloodStatus.RecPop);
                    tooltipText += "\n" + FormatPercentChangeText("Production", FloodStatus.ProdMod);
                    break;
                case 10: // FireStatus
                    tooltipText += FormatPercentChangeText("Recruitable population", FireStatus.RecPop);
                    tooltipText += "\n" + FormatPercentChangeText("Production", FireStatus.ProdMod);
                    break;
                case 0: // Tribal
                default:
                    tooltipText += "";
                    break;
            }

            tooltipText += "</align>";

            return tooltipText;
        }

        public static void deleteIcons(GameObject obj)
        {
            foreach (Transform child in obj.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public static void showIcons(GameObject obj, List<Status> statuses, List<Sprite> status_sprites,
            Province province, Map map)
        {
            if (statuses.SequenceEqual(previousStatuses) && previousTurn == map.turnCnt) return;

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

                    help_tooltip_trigger trigger = child.AddComponent<help_tooltip_trigger>();
                    trigger.TooltipText = GenerateTooltipText(status, province, map);
                }

                previousStatuses = new List<Status>(statuses);
                previousTurn = map.turnCnt;
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
    [SerializeField] private GameObject help_tooltip;

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
            btnTransform.Find("add").GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeUpgradeBuilding(SelectedProvince, buildingType));
            btnTransform.Find("remove").GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeDowngradeBuilding(SelectedProvince, buildingType));
        }

        recruitment_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeRecBox(SelectedProvince));
        festivities_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeFestivitiesOrganizationBox(SelectedProvince));
        tax_break_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeTaxBreakIntroductionBox(SelectedProvince));
        rebel_suppress_button.GetComponent<Button>().onClick.AddListener(() => dialog_box.invokeRebelSuppressionBox(SelectedProvince));
    }

    private void Update() {
        var coordinates = SelectedProvince;
        Province p = map.getProvince(coordinates);
        int countryId = map.Countries[p.OwnerId].Id;

        id_.Txt.SetText(coordinates.ToString() +
            (p.OwnerId != 0 ? " " + map.Countries[p.OwnerId].Name +
            (map.Controllers[countryId] == Map.CountryController.Ai ? " (AI)" : "") : ""));

        type_.Txt.SetText(p.IsLand ? "land" : "ocean");

        if (p.IsLand) 
        {
            res_.Txt.SetText("" + p.ResourcesP + "(" + p.ResourceAmount + ")");
            res_.Img.sprite = res_images[((int)p.ResourceType)];
            happ_.Txt.SetText("" + p.Happiness);
            pop_.Txt.SetText("" + p.Population);
            rec_pop_.Txt.SetText("" + p.RecruitablePopulation);

            UpdateUIElementStates(p); 
            UpdateEmblem(p.OwnerId);
            StatusDisplay.showIcons(statuses_list, p.Statuses, status_sprites, p, map);
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

        building_interface.SetActive(p.OwnerId == map.CurrentPlayer.Id);
        recruitment_button.SetActive(p.OwnerId == map.CurrentPlayer.Id);
        festivities_button.SetActive(p.OwnerId == map.CurrentPlayer.Id);
        tax_break_button.SetActive(p.OwnerId == map.CurrentPlayer.Id);
        rebel_suppress_button.SetActive(p.OwnerId == map.CurrentPlayer.Id);

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
        b_1.sprite = b_1_spr[province.Buildings[BuildingType.Infrastructure]];
        b_2.sprite = b_2_spr[province.Buildings[BuildingType.Fort]];
        b_3.sprite = b_3_spr[province.Buildings[BuildingType.School]];
        b_4.sprite = b_4_spr[province.Buildings[BuildingType.Mine]];

        UpdateBuildingButtonsStates(province);
    }

    private void UpdateBuildingButtonsStates(Province province)
    {
        if (map.CurrentPlayer.Id != province.OwnerId) return;

        var buttonMappings = new List<(Transform ButtonTransform, BuildingType Type)>
        {
            (b_1_m, BuildingType.Infrastructure),
            (b_2_m, BuildingType.Fort),
            (b_3_m, BuildingType.School),
            (b_4_m, BuildingType.Mine)
        };

        foreach (var (buttonTransform, buildingType) in buttonMappings)
        {
            int buildingLevel = province.Buildings[buildingType];
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
        emblem.GetComponent<Button>().onClick.AddListener(() => help_tooltip.transform.GetChild(0).gameObject.SetActive(false));
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