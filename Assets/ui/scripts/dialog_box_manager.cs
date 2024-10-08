using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Assets.classes;
using Assets.Scripts;

public class dialog_box_manager : MonoBehaviour
{
    [SerializeField] Map map;
    [SerializeField] private Image db_country_color_img;

    [SerializeField] private GameObject overlay;

    [SerializeField] private TMP_Text dialog_title;
    [SerializeField] private Button close_button;

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

    public class dialog_box_precons {
        internal class DialogBox {
            public string title, message;

            public DialogBox(string title, string message, Dictionary<Resource, float> cost = null) {
                this.title = title;
                this.message = message;
            }

            internal (string, string) toVars() {
                return (title, message);
            }
        }
        internal static DialogBox army_box = new("Move Army", "Select how many units you want to move");
        internal static DialogBox rec_box = new("Recruit Army", "Select how many units you want to recruit");
        internal static DialogBox dis_box = new("Disband Army", "Select how many units you want to disband");
        internal static DialogBox upBuilding_box = new("Build ", "Do you want to build ");
        internal static DialogBox downBuilding_box = new("Raze ", "Do you want to raze ");
        internal static DialogBox tech_box = new("Upgrade ", "Do you want to upgrade ");
    };

    private void Update() {
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
            //if (Input.GetKeyDown(KeyCode.Space)) {
            //    gameObject.SetActive(false);
            //    overlay.SetActive(false);
                
            //}
        }
    }

    public void invokeArmyBox(Map map, Army army, (int, int) destination) {
        (string title, string message) = dialog_box_precons.army_box.toVars();
        
        Action onConfirm = () => {
            //map.setMoveArmy(army, (int)dialog_slider.value, destination);
            var act = new Assets.classes.actionContainer.TurnAction.army_move(army.Position, destination, (int)dialog_slider.value, army);
            map.Countries[army.OwnerId].Actions.addAction(act);
        };
        Action onCancel = null;
        ShowSliderBox(title, message, onConfirm, onCancel, army.Count);
    }

    void OnEnable()
    {
        SetCoatOfArmsColor();    
    }

    public void invokeRecBox(Map map, (int, int) coordinates) {
        (string title, string message) = dialog_box_precons.rec_box.toVars();
        var province = map.getProvince(coordinates);
        Action onConfirm = () => {
            //map.recArmy(coordinates, (int)dialog_slider.value);
            var act = new Assets.classes.actionContainer.TurnAction.army_recruitment(coordinates, (int)dialog_slider.value);
            map.Countries[province.Owner_id].Actions.addAction(act);
        };
        Action onCancel= null;
        ShowSliderBox(title, message, onConfirm, onCancel, province.RecruitablePopulation);
    }

    public void invokeDisBox(Map map, Army army) {
        (string title, string message) = dialog_box_precons.dis_box.toVars();
        
        Action onConfirm = () => {
            var act = new Assets.classes.actionContainer.TurnAction.army_disbandment(army, (int)dialog_slider.value);
            map.Countries[army.OwnerId].Actions.addAction(act);
        };
        Action onCancel= null;
        ShowSliderBox(title, message, onConfirm, onCancel, army.Count);
    }
    

    public void invokeUpgradeBuilding(Map map, (int, int) coordinates, BuildingType type) {
        (string title, string message) = dialog_box_precons.upBuilding_box.toVars();
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
        var lvl = province.Buildings.Find(b => b.BuildingType == type).BuildingLevel + 1;
        title += lvl + "?";
        message += lvl + "?";
        Action onConfirm = () => {
            map.upgradeBuilding(coordinates, type);
        };
        Action onCancel = null;
        ShowConfirmBox(title, message, onConfirm, onCancel);
    }

    public void invokeDowngradeBuilding(Map map, (int, int) coordinates, BuildingType type) {
        (string title, string message) = dialog_box_precons.downBuilding_box.toVars();
        switch(type) {
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
        Action onConfirm = () => {
            map.downgradeBuilding(coordinates, type);
        };
        Action onCancel = null;
        ShowConfirmBox(title, message, onConfirm, onCancel);
    }

    public void invokeTechUpgradeBox(Technology type)
    {
        (string title, string message) = dialog_box_precons.tech_box.toVars();

        Dictionary<Resource, float> cost = null;

        switch (type)
        {
            case Technology.Military:
                title += "Military Technology";
                message += $"Military Technology to level {map.CurrentPlayer.Technology_[Technology.Military] + 1}?";
                cost = map.CurrentPlayer.techStats.milCost;
                break;

            case Technology.Economic:
                title += "Economic Technology";
                message += $"Economic Technology to level {map.CurrentPlayer.Technology_[Technology.Economic] + 1}?";
                cost = map.CurrentPlayer.techStats.ecCost;
                break;

            case Technology.Administrative:
                title += "Administrative Technology";
                message += $"Administrative Technology to level {map.CurrentPlayer.Technology_[Technology.Administrative] + 1}?";
                cost = map.CurrentPlayer.techStats.admCost;
                break;
            default:
                break;
        }

        float costAP = cost.ContainsKey(Resource.AP) ? cost[Resource.AP] : 0;
        float costSP = cost.ContainsKey(Resource.SciencePoint) ? cost[Resource.SciencePoint] : 0;

        Action onConfirm = () => {
            map.CurrentPlayer.Technology_[type]++;
            map.CurrentPlayer.Resources[Resource.AP] -= costAP;
            map.CurrentPlayer.Resources[Resource.SciencePoint] -= costSP;
            technology_manager.UpdateData();
        };
        Action onCancel = null;
        ShowConfirmBox(title, message, onConfirm, onCancel);
    }

    public void invokeConfirmBox(string title, string message, Action onConfirm, Action onCancel, Dictionary<Resource, float> cost) {
        bool confirmable = map.CurrentPlayer.canPay(cost);
        cost_element.SetActive(cost != null);
        ShowConfirmBox(title, message, onConfirm, onCancel, confirmable, cost);
    }
    public void invokeDisbandArmyBox(Map map, Army army)
    {
        (string title, string message) = dialog_box_precons.dis_box.toVars();

        Action onConfirm = () =>
        {
            int unitsToDisband = (int)dialog_slider.value;

            var act = new Assets.classes.actionContainer.TurnAction.army_disbandment(army, unitsToDisband);
            map.Countries[army.OwnerId].Actions.addAction(act);
        };

        Action onCancel = null;

        ShowSliderBox(title,message,onConfirm, onCancel, army.Count);
    }

    public void invokeEventBox(Event_ _event) {
        bool confirmable = map.CurrentPlayer.canPay(_event.Cost);
        if (_event.Cost == null) cost_element.SetActive(false);
        else cost_element.SetActive(true);
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
        ShowConfirmBox("", _event.msg, onConfirm, onCancel, confirmable, _event.Cost);
    }

    private void ShowDialogBox(string actionTitle, string message, System.Action onConfirm, System.Action onCancel, bool confirmable = true, string txtConfirm = null, string txtCancel = null)
    {
        map.CurrentPlayer.setCoatandColor(db_country_color_img);
        close_button.onClick.RemoveAllListeners();
        confirm_button.onClick.RemoveAllListeners();
        cancel_button.onClick.RemoveAllListeners();
        var txt_con = confirm_button.GetComponentInChildren<TMP_Text>();
        var txt_can = cancel_button.GetComponentInChildren<TMP_Text>();
        if(txtConfirm != null) txt_con.SetText(txtConfirm);
        else txt_con.SetText("Ok");
        if(txtCancel != null) txt_can.SetText(txtCancel);
        else txt_can.SetText("Cancel");
        dialog_title.text = actionTitle;

        close_button.onClick.AddListener(() =>
        {
            HideDialog();
        });

        dialog_message.text = message;
        confirm_button.interactable = confirmable;
        confirm_button.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            HideDialog();
        });

        cancel_button.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            click_sound.Play();
            HideDialog();
        });

        CenterDialogBox();
        overlay.SetActive(true);
        gameObject.SetActive(true);
    }

    private void ShowSliderBox(string actionTitle, string message, System.Action onConfirm, System.Action onCancel, 
        int maxValue, Dictionary<Resource, float> cost = null, List<Effect> effects = null)
    {
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
            confirm_button.interactable = (value > 0);
        });
 
        choice_element.SetActive(true);
        cost_element.SetActive(true);
        ShowDialogBox(actionTitle, message, onConfirm, onCancel, false);
    }

    private void ShowConfirmBox(string actionTitle, string message, System.Action onConfirm, System.Action onCancel, 
        bool confirmable = true, Dictionary<Resource, float> cost = null, List<Effect> effects = null)
    {
        SetCostContent(cost, effects: effects);
        choice_element.SetActive(false);
        cost_element.SetActive(cost!=null);
        //cost_element.SetActive(true);
        ShowDialogBox(actionTitle, message, onConfirm, onCancel, confirmable);
    }

    public void HideDialog()
    {
        gameObject.SetActive(false);
        overlay.SetActive(false);
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
                    ? (item.Value * sliderValue.Value).ToString()
                    : item.Value.ToString();

                costRows.Add(new Effect(resourceSprite, $"{resourceName}:", $"-{costValue}", false));
            }
        }

        if (effects != null && effects.Count > 0)
        {
            costRows.AddRange(effects);
        }

        if (costRows.Count > 0)
        {
            SetEffects(costRows);
        }
    }

    private void SetEffects(List<Effect> actionEffects)
    {
        foreach (Transform child in cost_content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var effect in actionEffects)
        {
            GameObject effectRow = Instantiate(effect_row_prefab, cost_content.transform);
            var effectUI = effectRow.GetComponent<effect_ui>();
            effectUI.SetEffect(effect);
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

    public void SetCoatOfArmsColor()
    {
        db_country_color_img.color = map.CurrentPlayer.Color;
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