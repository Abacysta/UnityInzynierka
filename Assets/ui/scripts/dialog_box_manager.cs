using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using JetBrains.Annotations;

public class dialog_box_manager : MonoBehaviour
{
    [SerializeField] private GameObject overlay;

    [SerializeField] private Image country_image;
    [SerializeField] private TMP_Text dialog_title;
    [SerializeField] private Button close_button;

    [SerializeField] private TMP_Text dialog_message;
    [SerializeField] private GameObject choice_area;
    [SerializeField] private GameObject cost_area;
    [SerializeField] private TMP_Text quantity_text;
    [SerializeField] private Slider dialog_slider;
    [SerializeField] private TMP_Text slider_max;
    [SerializeField] private TMP_Text cost_content;

    [SerializeField] private Button cancel_button;
    [SerializeField] private Button confirm_button;

    [SerializeField] private AudioSource click_sound, move_sound;

    public class dialog_box_precons {
        internal class DialogBox {
            public string image, title, message;

            public DialogBox(string image, string title, string message) {
                this.image = image;
                this.title = title;
                this.message = message;
            }

            internal (string, string, string) toVars() {
                return (image, title, message);
            }
        }
        internal static DialogBox army_box = new("gauls", "Move Army", "Select how many units you want to move");
        internal static DialogBox rec_box = new("gauls", "Recruit Army", "Select how many units you want to recruit");
        internal static DialogBox dis_box = new("gauls", "Disband Army", "Select how many units you want to disband");
        internal static DialogBox upBuilding_box = new("gauls", "Build ", "Do you want to build ");
        internal static DialogBox downBuilding_box = new("gauls", "Raze ", "Do you want to raze ");
    };

    public void invokeArmyBox(Map map, Army army, (int, int) destination) {
        (string image, string title, string message) = dialog_box_precons.army_box.toVars();
        Action onConfirm = () => {
            map.setMoveArmy(army, (int)dialog_slider.value, destination);
            move_sound.Play();
        };
        Action onCancel = null;
        ShowSliderBox(image, title, message, onConfirm, onCancel, army.Count);
    }

    public void invokeRecBox(Map map, (int, int) coordinates) {
        (string image, string title, string message) = dialog_box_precons.rec_box.toVars();
        var province = map.getProvince(coordinates);
        Action onConfirm = () => {
            map.recArmy(coordinates, (int)dialog_slider.value);
        };
        Action onCancel= null;
        ShowSliderBox(image, title, message, onConfirm, onCancel, province.RecruitablePopulation);
    }

    public void invokeDisBox(Map map, Army army) {
        (string image, string title, string message) = dialog_box_precons.dis_box.toVars();
        Action onConfirm = () => {
            map.disbandArmy(army, (int)dialog_slider.value);
        };
        Action onCancel= null;
        ShowSliderBox(image, title, message, onConfirm, onCancel, army.count);
    }

    public void invokeUpgradeBuilding(Map map, (int, int) coordinates, BuildingType type) {
        (string image, string title, string message) = dialog_box_precons.upBuilding_box.toVars();
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
        ShowConfirmBox(image, title, message, onConfirm, onCancel);
    }

    public void invokeDowngradeBuilding(Map map, (int, int) coordinates, BuildingType type) {
        (string image, string title, string message) = dialog_box_precons.downBuilding_box.toVars();
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
        ShowConfirmBox(image, title, message, onConfirm, onCancel);
    }

    private void ShowDialogBox(string imageName, string actionTitle, string message, System.Action onConfirm, System.Action onCancel, string txtConfirm=null, string txtCancel=null)
    {
        close_button.onClick.RemoveAllListeners();
        confirm_button.onClick.RemoveAllListeners();
        cancel_button.onClick.RemoveAllListeners();
        var txt_con = confirm_button.GetComponentInChildren<TMP_Text>();
        var txt_can = cancel_button.GetComponentInChildren<TMP_Text>();
        if(txtConfirm != null) txt_con.SetText(txtConfirm);
        else txt_con.SetText("Ok");
        if(txtCancel != null) txt_can.SetText(txtCancel);
        else txt_can.SetText("Cancel");
        dialog_slider.value = dialog_slider.minValue;
        dialog_slider.onValueChanged.AddListener(OnSliderValueChanged);

        SetCountryImage(imageName);

        dialog_title.text = actionTitle;

        close_button.onClick.AddListener(() =>
        {
            HideDialog();
        });

        dialog_message.text = message;

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

        overlay.SetActive(true);
        gameObject.SetActive(true);
    }

    private void ShowSliderBox(string imageName, string actionTitle, string message, System.Action onConfirm, System.Action onCancel, int maxValue)
    {
        choice_area.SetActive(true);
        cost_area.SetActive(true);
        cost_content.text = "";
        dialog_slider.maxValue = maxValue;
        slider_max.text = maxValue.ToString();
        quantity_text.text = dialog_slider.value.ToString();

        ShowDialogBox(imageName, actionTitle, message, onConfirm, onCancel);
    }

    private void ShowConfirmBox(string imageName, string actionTitle, string message, System.Action onConfirm, System.Action onCancel)
    {

        choice_area.SetActive(false);
        cost_area.SetActive(true);
        cost_content.text = "";

        ShowDialogBox(imageName, actionTitle, message, onConfirm, onCancel);
    }

    public void HideDialog()
    {
        gameObject.SetActive(false);
        overlay.SetActive(false);
    }

    public void SetCountryImage(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>(spriteName);

        if (sprite != null)
        {
            country_image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Cannot find image with the name: " + spriteName);
        }
    }

    public void OnSliderValueChanged(float value)
    {
        quantity_text.text = value.ToString();
    }

    public void SetCost(string costText) 
    {
        cost_content.text = costText;
    }
}