using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    public void ShowDialogBox(string imageName, string actionTitle, string message, System.Action onConfirm, System.Action onCancel)
    {
        close_button.onClick.RemoveAllListeners();
        confirm_button.onClick.RemoveAllListeners();
        cancel_button.onClick.RemoveAllListeners();

        dialog_slider.onValueChanged.AddListener(OnSliderValueChanged);

        SetCountryImage(imageName);

        dialog_title.text = actionTitle;

        close_button.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
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
            HideDialog();
        });

        overlay.SetActive(true);
        gameObject.SetActive(true);
    }

    public void ShowMoveArmyBox(string imageName, System.Action onConfirm, System.Action onCancel, int maxValue)
    {
        string actionTitle = "Moving the army";
        string message = "Are you sure you want to move the army?";

        choice_area.SetActive(true);
        cost_area.SetActive(true);
        cost_content.text = "";

        dialog_slider.maxValue = maxValue;
        slider_max.text = maxValue.ToString();
        quantity_text.text = dialog_slider.value.ToString();

        ShowDialogBox(imageName, actionTitle, message, onConfirm, onCancel);
    }
    public void ShowRecruitArmyBox(string imageName, System.Action onConfirm, System.Action onCancel, int maxValue)
    {
        string actionTitle = "Recruiting the army";
        string message = "Are you sure you want to recruit the army?";

        choice_area.SetActive(true);
        cost_area.SetActive(true);
        cost_content.text = "";

        dialog_slider.maxValue = maxValue;
        slider_max.text = maxValue.ToString();
        quantity_text.text = dialog_slider.value.ToString();

        ShowDialogBox(imageName, actionTitle, message, onConfirm, onCancel);
    }

    public void ShowUpgradeTechnologyBox(string imageName, System.Action onConfirm, System.Action onCancel)
    {
        string actionTitle = "Technology upgrade";
        string message = "Are you sure you want to upgrade the technology?";

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