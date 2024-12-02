using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Assets.classes;
using Assets.Scripts;
using System.Reflection;
using Assets.classes.subclasses;
using static Assets.classes.TurnAction;

public class dialog_box_manager : MonoBehaviour
{
    [SerializeField] Map map;
    [SerializeField] private Image db_country_color_img;

    [SerializeField] private GameObject overlay;

    [SerializeField] private TMP_Text dialog_title;
    [SerializeField] private Button close_button;
    [SerializeField] private Button zoom_button;

    [SerializeField] private TMP_Text dialog_message;
    [SerializeField] private GameObject choice_element;
    [SerializeField] private GameObject cost_element;
    [SerializeField] private TMP_Text quantity_text;
    [SerializeField] private Slider dialog_slider;
    [SerializeField] private TMP_Text slider_max;
    [SerializeField] private GameObject cost_content;
    [SerializeField] private GameObject effect_row_prefab;

    [SerializeField] private Button cancel_button;
    [SerializeField] private Button confirm_button;

    [SerializeField] private AudioSource click_sound;
    [SerializeField] private alerts_manager alerts;
    [SerializeField] private technology_manager technology_manager;

    [SerializeField] private Sprite gold_sprite;
    [SerializeField] private Sprite wood_sprite;
    [SerializeField] private Sprite iron_sprite;
    [SerializeField] private Sprite science_point_sprite;
    [SerializeField] private Sprite ap_sprite;
    [SerializeField] private Sprite happiness_sprite;
    [SerializeField] private Sprite tax_mod_sprite;
    [SerializeField] private Sprite pop_mod_sprite;
    [SerializeField] private Sprite prod_mod_sprite;

    public class dialog_box_precons {
        internal class DialogBox {
            public string title, message;

            public DialogBox(string title, string message) {
                this.title = title;
                this.message = message;
            }

            internal (string, string) toVars() {
                return (title, message);
            }
        }

        internal static DialogBox armyMoveBox = new("Move Army", "Select how many units you want to move.");
        internal static DialogBox recruitBox = new("Recruit Army", "Select how many units you want to recruit.");
        internal static DialogBox disbandBox = new("Disband Army", "Select how many units you want to disband.");
        internal static DialogBox upBuildingBox = new("Build ", "Do you want to build ");
        internal static DialogBox downBuildingBox = new("Raze ", "Do you want to raze ");
        internal static DialogBox techBox = new("Upgrade ", "Do you want to upgrade ");
        internal static DialogBox taxBreakBox = new("Introduce Tax Break", "Do you want to introduce a tax break?");
        internal static DialogBox festivitiesBox = new("Organize Festivities", "Do you want to organize festivities?");
        internal static DialogBox rebelSuppressBox = new("Suppress the Rebellion", "Do you want to suppress the rebellion?");
    };

    void Update() {
        if (gameObject.activeSelf) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                gameObject.SetActive(false);
                overlay.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.LeftShift)) {
                cancel_button.onClick.Invoke();
                overlay.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Return)) {
                confirm_button.onClick.Invoke();
                overlay.SetActive(false);
            }
        }
    }

    public void invokeArmyBox(Army army, (int, int) destination) {
        (string title, string message) = dialog_box_precons.armyMoveBox.toVars();

        int affordableValue = map.CurrentPlayer
            .CalculateMaxArmyUnits(CostsCalculator.TurnActionFullCost(ActionType.ArmyMove), army.Count);

        Action onConfirm = () => {
            var act = new Assets.classes.TurnAction.army_move(army.Position, destination, (int)dialog_slider.value, army);
            map.Countries[army.OwnerId].Actions.addAction(act);
        };
        float cost = CostsCalculator.TurnActionApCost(ActionType.ArmyMove);
        ShowSliderBox(title, message, onConfirm, army.Count, CostsCalculator.TurnActionFullCost(ActionType.ArmyMove), affordableValue: affordableValue);
    }

    public void invokeRecBox((int, int) coordinates) {
        (string title, string message) = dialog_box_precons.recruitBox.toVars();
        var province = map.getProvince(coordinates);
        Country.TechnologyInterpreter techStats = map.Countries[province.Owner_id].techStats;

        int affordableValue = map.CurrentPlayer
            .CalculateMaxArmyUnits(CostsCalculator.TurnActionFullCost(ActionType.ArmyRecruitment, techStats), province.RecruitablePopulation);

        Action onConfirm = () => {
            var act = new Assets.classes.TurnAction.army_recruitment(coordinates, (int)dialog_slider.value, techStats);
            map.Countries[province.Owner_id].Actions.addAction(act);
        };

        ShowSliderBox(title, message, onConfirm, province.RecruitablePopulation, 
            CostsCalculator.TurnActionFullCost(ActionType.ArmyRecruitment, techStats), affordableValue: affordableValue);
    }

    public void invokeDisbandArmyBox(Army army)
    {
        (string title, string message) = dialog_box_precons.disbandBox.toVars();

        int affordableValue = map.CurrentPlayer
            .CalculateMaxArmyUnits(CostsCalculator.TurnActionFullCost(ActionType.ArmyDisbandment), army.Count);

        Action onConfirm = () =>
        {
            int unitsToDisband = (int)dialog_slider.value;
            var act = new Assets.classes.TurnAction.army_disbandment(army, unitsToDisband);
            map.Countries[army.OwnerId].Actions.addAction(act);
        };

        ShowSliderBox(title, message, onConfirm, army.Count, 
            CostsCalculator.TurnActionFullCost(ActionType.ArmyDisbandment), affordableValue: affordableValue);
    }

    public void invokeUpgradeBuilding((int, int) coordinates, BuildingType type) {
        (string title, string message) = dialog_box_precons.upBuildingBox.toVars();

        var province = map.getProvince(coordinates);

        switch(type) {
            case BuildingType.Fort:
                title += "Fort lvl";
                message += "Fort lvl";
                break;
            case BuildingType.Infrastructure:
                title += "Infrastructure lvl";
                message += "Infrastructure lvl";
                break;
            case BuildingType.Mine:
                title += "Mine lvl";
                message += "Mine lvl";
                break;
            case BuildingType.School:
                title += "School lvl";
                message += "School lvl";
                break;
            default:
                break;
        }

        int lvl = province.Buildings.ContainsKey(type) ? province.Buildings[type] + 1 : 0;
        title += lvl + "?";
        message += lvl + "?";

        Action onConfirm = () => {
            var act = new building_upgrade(province, type);
            map.CurrentPlayer.Actions.addAction(act);
        };

        var cost = CostsCalculator.TurnActionFullCost(ActionType.BuildingUpgrade, type, lvl);

        ShowConfirmBox(title, message, onConfirm, map.CurrentPlayer.isPayable(cost), cost: cost);
    }

    public void invokeDowngradeBuilding((int, int) coordinates, BuildingType type) {
        (string title, string message) = dialog_box_precons.downBuildingBox.toVars();

        var province = map.getProvince(coordinates);

        switch (type) {
            case BuildingType.Fort:
                title += "Fort";
                message += "Fort";
                break;
            case BuildingType.Infrastructure:
                title += "Infrastructure";
                message += "Infrastructure";
                break;
            case BuildingType.Mine:
                title += "Mine";
                message += "Mine";
                break;
            case BuildingType.School:
                title += "School";
                message += "School";
                break;
            default:
                break;
        }

        int lvl = province.Buildings.ContainsKey(type) ? province.Buildings[type] : 0;
        title += lvl + "?";
        message += lvl + "?";

        Action onConfirm = () => {
            var act = new building_downgrade(province, type);
            map.CurrentPlayer.Actions.addAction(act);
        };

        var cost = CostsCalculator.TurnActionFullCost(ActionType.BuildingDowngrade);

        ShowConfirmBox(title, message, onConfirm, map.CurrentPlayer.isPayable(cost), cost: cost);
    }

    public void invokeTechUpgradeBox(Technology type)
    {
        (string title, string message) = dialog_box_precons.techBox.toVars();

        switch (type)
        {
            case Technology.Military:
                title += "Military Technology";
                message += $"Military Technology to level {map.CurrentPlayer.Technologies[Technology.Military] + 1}?";
                break;
            case Technology.Economic:
                title += "Economic Technology";
                message += $"Economic Technology to level {map.CurrentPlayer.Technologies[Technology.Economic] + 1}?";
                break;
            case Technology.Administrative:
                title += "Administrative Technology";
                message += $"Administrative Technology to level {map.CurrentPlayer.Technologies[Technology.Administrative] + 1}?";
                break;
            default:
                break;
        }

        Action onConfirm = () => {
            var act = new technology_upgrade(map.CurrentPlayer, type);
            map.CurrentPlayer.Actions.addAction(act);
            technology_manager.UpdateData();
        };
        
        ShowConfirmBox(title, message, onConfirm, confirmable: true, 
            cost: CostsCalculator.TurnActionFullCost(ActionType.TechnologyUpgrade,
            tech: map.CurrentPlayer.Technologies, techType: type));
    }

    public void invokeTaxBreakIntroductionBox((int, int) coordinates)
    {
        (string title, string message) = dialog_box_precons.taxBreakBox.toVars();

        var province = map.getProvince(coordinates);

        Action onConfirm = () => {
            var act = new tax_break_introduction(province);
            map.CurrentPlayer.Actions.addAction(act);
        };

        var cost = CostsCalculator.TurnActionFullCost(ActionType.TaxBreakIntroduction);

        List<Effect> effects = new()
        {
            new(tax_mod_sprite, "Tax", $"{TaxBreak.TaxMod}%", true),
            new(happiness_sprite, "Happiness growth", $"{(TaxBreak.HappMod >= 0 ? "+" : "")}{TaxBreak.HappMod * 100}%", true),
            new(happiness_sprite, "Happiness", $"{(TaxBreak.HappStatic >= 0 ? "+" : "")}{TaxBreak.HappStatic}", true)
        };

        ShowConfirmBox(title, message, onConfirm, map.CurrentPlayer.isPayable(cost), cost: cost, effects: effects);
    }

    public void invokeFestivitiesOrganizationBox((int, int) coordinates)
    {
        (string title, string message) = dialog_box_precons.festivitiesBox.toVars();

        var province = map.getProvince(coordinates);

        Action onConfirm = () => {
            var act = new festivities_organization(province);
            map.CurrentPlayer.Actions.addAction(act);
        };

        var cost = CostsCalculator.TurnActionFullCost(ActionType.FestivitiesOrganization);

        List<Effect> effects = new()
        {
            new(prod_mod_sprite, "Production", $"{(Festivities.ProdMod >= 0 ? "+" : "")}{Festivities.ProdMod * 100}%", false),
            new(pop_mod_sprite, "Population growth", $"{(Festivities.PopMod >= 0 ? "+" : "")}{Festivities.PopMod * 100}%", true),
            new(happiness_sprite, "Happiness", $"{(Festivities.HappStatic >= 0 ? "+" : "")}{Festivities.HappStatic}", true)
        };

        ShowConfirmBox(title, message, onConfirm, map.CurrentPlayer.isPayable(cost), cost: cost, effects: effects);
    }

    public void invokeRebelSuppressionBox((int, int) coordinates)
    {
        (string title, string message) = dialog_box_precons.rebelSuppressBox.toVars();

        var province = map.getProvince(coordinates);

        Action onConfirm = () => {
            var act = new rebel_suppresion(province);
            map.CurrentPlayer.Actions.addAction(act);
        };

        var cost = CostsCalculator.TurnActionFullCost(ActionType.RebelSuppresion);

        ShowConfirmBox(title, message, onConfirm, map.CurrentPlayer.isPayable(cost), cost: cost);
    }

    public void invokeConfirmBox(string title, string message, Action onConfirm, Action onCancel = null, Dictionary<Resource, float> cost = null) {
        bool confirmable = true;
        if (cost != null)
        {
            confirmable = map.CurrentPlayer.isPayable(cost);
        }

        ShowConfirmBox(title, message, onConfirm, confirmable, cost: cost);
    }

    public void invokeEventBox(Event_ _event) {
        bool confirmable = true;
        if (_event.Cost != null)
        {
            confirmable = map.CurrentPlayer.isPayable(_event.Cost);
        }

        // Search for a non-static, public, non-inherited method named "reject"
        MethodInfo rejectMethod = _event.GetType().GetMethod("reject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        // If the reject method exists and has a body (braces), it is rejectable
        bool rejectable = rejectMethod != null && rejectMethod.GetMethodBody() != null;

        cost_element.SetActive(_event.Cost != null);

        Action onConfirm = () => {
            _event.accept();
            map.CurrentPlayer.Events.Remove(_event);
            alerts.sortedevents.Remove(_event);
            alerts.reloadAlerts();
        };

        Action onCancel = () => {
            _event.reject();
            map.CurrentPlayer.Events.Remove(_event);
            alerts.sortedevents.Remove(_event);
            alerts.reloadAlerts();
        };

        zoom_button.onClick.AddListener(() => {
            _event.zoom();
            HideDialog();
        });

        ShowConfirmBox("", _event.Message, onConfirm, confirmable, rejectable, zoomable: true, cost: _event.Cost, confirmText: "Confirm", cancelText: "Reject", onCancel: onCancel);
    }

    private void ShowDialogBox(string actionTitle, string message, Action onConfirm,
        bool confirmable = true, bool rejectable = true, string confirmText = "OK", string cancelText = "Cancel", 
        Action onCancel = null, Action onClose = null)
    {
        if (map.currentPlayer > 0 && map.Countries.Count > 0)
        {
            map.CurrentPlayer.setCoatandColor(db_country_color_img);
            db_country_color_img.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            db_country_color_img.transform.parent.gameObject.SetActive(false);
        }

        close_button.onClick.RemoveAllListeners();
        confirm_button.onClick.RemoveAllListeners();
        cancel_button.onClick.RemoveAllListeners();

        confirm_button.GetComponentInChildren<TMP_Text>().SetText(confirmText);
        cancel_button.GetComponentInChildren<TMP_Text>().SetText(cancelText);

        dialog_title.text = actionTitle;
        dialog_message.text = message;

        close_button.onClick.AddListener(() =>
        {
            onClose?.Invoke();
            HideDialog();
        });

        confirm_button.interactable = confirmable;
        confirm_button.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            HideDialog();
        });

        cancel_button.interactable = rejectable;
        cancel_button.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            click_sound.Play();
            HideDialog();
        });

        CenterDialogBox();
        ShowDialog();
    }

    private void ShowSliderBox(string actionTitle, string message, Action onConfirm, int maxValue,
        Dictionary<Resource, float> cost = null, List<Effect> effects = null, int affordableValue = int.MaxValue,
        string confirmText = "OK", string cancelText = "Cancel", Action onCancel = null, Action onClose = null) {
        dialog_slider.value = 0;
        dialog_slider.maxValue = maxValue;
        slider_max.text = maxValue.ToString();
        quantity_text.text = dialog_slider.value.ToString();

        SetCostContent(cost, dialog_slider.value);

        dialog_slider.onValueChanged.RemoveAllListeners();
        dialog_slider.onValueChanged.AddListener((value) =>
        {
            quantity_text.text = value.ToString();
            SetCostContent(cost, dialog_slider.value);
            confirm_button.interactable = value > 0 && value <= affordableValue;
        });

        zoom_button.gameObject.SetActive(false);
        choice_element.SetActive(true);
        cost_element.SetActive(true);

        ShowDialogBox(actionTitle, message, onConfirm, confirmable: false, rejectable: true,
            confirmText, cancelText, onCancel, onClose);
    }

    private void ShowConfirmBox(string actionTitle, string message, Action onConfirm,
        bool confirmable = true, bool rejectable = true, bool zoomable = false, Dictionary<Resource, float> cost = null,
        List<Effect> effects = null, string confirmText = "OK", string cancelText = "Cancel", Action onCancel = null, Action onClose = null)
    {
        SetCostContent(cost, effects: effects, sliderValue: null);

        zoom_button.gameObject.SetActive(zoomable);
        choice_element.SetActive(false);
        cost_element.SetActive(cost!=null);

        ShowDialogBox(actionTitle, message, onConfirm, confirmable, rejectable, confirmText, cancelText, onCancel, onClose);
    }

    public void HideDialog()
    {
        gameObject.SetActive(false);
        overlay.SetActive(false);
    }

    public void ShowDialog()
    {
        gameObject.SetActive(true);
        overlay.SetActive(true);
    }

    private void SetCostContent(Dictionary<Resource, float> cost = null, float? sliderValue = null, List<Effect> effects = null)
    {
        var costRows = new List<Effect>();

        if (cost != null && cost.Count > 0)
        {
            foreach (var item in cost)
            {
                var (resourceName, resourceSprite) = GetResourceDetails(item.Key);
                string costValue = sliderValue.HasValue && item.Key != Resource.AP
                ? (item.Value * sliderValue.Value).ToString("F1")
                : item.Value.ToString("F1");

                costRows.Add(new Effect(resourceSprite, $"{resourceName}:", $"-{costValue}", false));
            }
        }

        if (effects != null && effects.Count > 0)
        {
            costRows.AddRange(effects);
        }

        SetEffects(costRows);
    }

    private void SetEffects(List<Effect> actionEffects)
    {
        foreach (Transform child in cost_content.transform)
        {
            Destroy(child.gameObject);
        }

        if (actionEffects != null && actionEffects.Count > 0)
        {
            foreach (var effect in actionEffects)
            {
                GameObject effectRow = Instantiate(effect_row_prefab, cost_content.transform);
                var effectUI = effectRow.GetComponent<effect_ui>();
                effectUI.SetEffect(effect);
            }
        }
    }

    public void addValue(int value) {
        dialog_slider.value += value;
    }

    public void subValue(int value) { 
        dialog_slider.value -= value;
    }

    public void percentValue(float percent) { 
        dialog_slider.value = dialog_slider.maxValue * percent;
    }

    private (string name, Sprite sprite) GetResourceDetails(Resource resource)
    {
        switch (resource)
        {
            case Resource.Gold:
                return ("Gold", gold_sprite);
            case Resource.Wood:
                return ("Wood", wood_sprite);
            case Resource.Iron:
                return ("Iron", iron_sprite);
            case Resource.SciencePoint:
                return ("Science points", science_point_sprite);
            case Resource.AP:
                return ("Action points", ap_sprite);
            default:
                return (resource.ToString(), null);
        }
    }

    private void CenterDialogBox()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.offsetMax = new Vector2(0f, 0f);
        rectTransform.offsetMin = new Vector2(0f, 0f);
    }
}