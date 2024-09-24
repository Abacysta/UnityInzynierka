using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.classes.Event_.DiploEvent;
using static Assets.classes.Relation;
using Assets.map.scripts;

public class diplomatic_actions_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private diplomatic_relations_manager diplomatic_relations_manager;

    // country info
    [SerializeField] private Image country_color_img;
    [SerializeField] private TMP_Text country_name_txt;

    [SerializeField] private TMP_Text provinces_count_text;
    [SerializeField] private TMP_Text population_text;
    [SerializeField] private TMP_Text their_opinion_text;
    [SerializeField] private TMP_Text our_opinion_text;

    // panel
    [SerializeField] private GameObject message_area;
    [SerializeField] private GameObject amount_area;
    [SerializeField] private GameObject country_choice_area;
    [SerializeField] private GameObject effect_area;

    // message
    [SerializeField] private TMP_Text ap_text;
    [SerializeField] private TMP_Text message_text;

    // amount area
    [SerializeField] private Slider subsidy_slider;
    [SerializeField] private TMP_Text slider_max;
    [SerializeField] private TMP_Text amount_text;

    [SerializeField] private TMP_Text duration_text;
    [SerializeField] private Button less_button;
    [SerializeField] private Button more_button;

    // dropdown
    [SerializeField] private Sprite coat_of_arms_background;
    [SerializeField] private TMP_Dropdown country_dropdown;
    [SerializeField] private Image pattern_img;

    // effects
    [SerializeField] private GameObject effect_content;
    [SerializeField] private GameObject effect_row_prefab;

    // buttons
    [SerializeField] private Button war_declare_button;
    [SerializeField] private Button vasal_rebel_button;
    [SerializeField] private Button peace_offer_button;
    [SerializeField] private Button diplomatic_mission_button;
    [SerializeField] private Button insult_button;
    [SerializeField] private Button alliance_offer_button;
    [SerializeField] private Button alliance_break_button;
    [SerializeField] private Button call_to_war_button;
    [SerializeField] private Button subsidize_button;
    [SerializeField] private Button subs_end_button;
    [SerializeField] private Button subs_request_button;
    [SerializeField] private Button mil_acc_offer_button;
    [SerializeField] private Button mil_acc_end_button;
    [SerializeField] private Button vasal_offer_button;

    [SerializeField] private Button send_message_button;

    private Dictionary<Color, int> countryColorToDropdownIndexMap = new();
    private int countryId;
    private Country receiverCountry;
    private Country currentPlayer;
    private int goldValue = 0;
    private int durationValue = 0;
    private const int maxDurationValue = 1000;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        country_dropdown.onValueChanged.AddListener(delegate { UpdateCaption(); });
        
        war_declare_button.onClick.AddListener(SetDeclareWarMessagePanel);
        vasal_rebel_button.onClick.AddListener(SetVassalRebelMessagePanel);
        peace_offer_button.onClick.AddListener(SetOfferPeaceMessagePanel);
        diplomatic_mission_button.onClick.AddListener(SetDiplomaticMissionMessagePanel);
        insult_button.onClick.AddListener(SetInsultMessagePanel);
        alliance_offer_button.onClick.AddListener(SetOfferAllianceMessagePanel);
        alliance_break_button.onClick.AddListener(SetBreakAllianceMessagePanel);
        call_to_war_button.onClick.AddListener(SetCallToWarMessagePanel);
        subsidize_button.onClick.AddListener(SetSubsidizeMessagePanel);
        subs_end_button.onClick.AddListener(SetEndSubsidiesMessagePanel);
        subs_request_button.onClick.AddListener(SetRequestSubsidiesMessagePanel);
        mil_acc_offer_button.onClick.AddListener(SetOfferMilitaryAccessMessagePanel);
        mil_acc_end_button.onClick.AddListener(SetEndMilitaryAccessMessagePanel);
        vasal_offer_button.onClick.AddListener(SetOfferVassalizationMessagePanel);
    }

    public void ShowDiplomaticActionsInterface(int countryId)
    {
        this.countryId = countryId;
        receiverCountry = map.Countries[countryId];
        currentPlayer = map.CurrentPlayer;

        SetCountryInfo();
        SetActionButtonStates();

        pattern_img.enabled = false;
        send_message_button.interactable = false;
        DeactivateAreas();
        gameObject.SetActive(true);
    }

    private void SetCountryInfo()
    {
        country_color_img.color = receiverCountry.Color;
        country_name_txt.text = receiverCountry.Name;

        provinces_count_text.text = receiverCountry.Provinces.Count.ToString();
        population_text.text = receiverCountry.Provinces.Sum(p => p.Population).ToString();

        int theirOpinion = receiverCountry.Opinions.ContainsKey(currentPlayer.Id) ? receiverCountry.Opinions[currentPlayer.Id] : 0;
        int ourOpinion = currentPlayer.Opinions.ContainsKey(countryId) ? currentPlayer.Opinions[countryId] : 0;

        SetOpinionText(their_opinion_text, theirOpinion);
        SetOpinionText(our_opinion_text, ourOpinion);
    }

    private void SetOpinionText(TMP_Text textElement, int opinion)
    {
        textElement.color = opinion == 0 ? Color.yellow : (opinion < 0 ? Color.red : Color.green);
        textElement.text = opinion == 0 ? "0" : (opinion > 0 ? "+" + opinion : opinion.ToString());
    }

    private void SetActionButtonStates()
    {
        bool isVassal = map.Relations
            .OfType<Vassalage>()
            .Any(rel => rel.Sides[0] == currentPlayer && rel.Sides[1] == receiverCountry);

        vasal_rebel_button.interactable = isVassal;
    }

    private void SetSlider()
    {
        int goldAmount = Mathf.FloorToInt(map.CurrentPlayer.Resources[Resource.Gold]);
        
        subsidy_slider.value = 0;
        subsidy_slider.maxValue = goldAmount;
        slider_max.text = goldAmount.ToString();
        amount_text.text = subsidy_slider.value.ToString();

        subsidy_slider.onValueChanged.RemoveAllListeners();
        subsidy_slider.onValueChanged.AddListener((value) =>
        {
            goldValue = Mathf.RoundToInt(value);
            amount_text.text = goldValue.ToString();
            UpdateSendButton();
        });

        less_button.onClick.RemoveAllListeners();
        more_button.onClick.RemoveAllListeners();

        UpdateDurationArea();

        less_button.onClick.AddListener(() =>
        {
            if (durationValue > 0)
            {
                durationValue--;
                UpdateDurationArea();
            }
        });

        more_button.onClick.AddListener(() =>
        {
            if (durationValue < maxDurationValue)
            {
                durationValue++;
                UpdateDurationArea();
            }
        });
    }

    private void UpdateDurationArea()
    {
        duration_text.text = durationValue.ToString();
        UpdateSendButton();
    }

    private void UpdateSendButton()
    {
        send_message_button.interactable = (goldValue > 0 && durationValue > 0);
    }

    private void UpdateSendButton(System.Action onSend)
    {
        send_message_button.onClick.RemoveAllListeners();
        send_message_button.onClick.AddListener(() => {
            onSend?.Invoke();
            gameObject.SetActive(false);
        });

    }

    private void SetDropdownCountries()
    {
        country_dropdown.ClearOptions();
        countryColorToDropdownIndexMap.Clear();

        // zawezic pozniej do tylko tych krajow, z ktorymi currentPlayer ma wojne
        var options = map.Countries
            .Where(c => c.Id != 0 && c.Id != countryId && c.Id != map.currentPlayer)
            .Select((country, index) =>
            {
                countryColorToDropdownIndexMap[country.Color] = index;
                return new TMP_Dropdown.OptionData
                {
                    text = country.Name,
                    image = coat_of_arms_background
                };
            })
            .ToList();

        country_dropdown.AddOptions(options);
        country_dropdown.RefreshShownValue();
    }

    private void SetEffects()
    {

    }

    public void UpdateDropdownImagesColor()
    {
        Transform content = country_dropdown.transform.Find("Dropdown List/country_viewport/country_content");

        for (int i = 0; i < country_dropdown.options.Count; i++)
        {
            Transform item = content.GetChild(i + 1);

            if (item.Find("item_coat_of_arms/item_country_color_img").TryGetComponent<Image>(out var itemImage))
            {
                Color countryColor = countryColorToDropdownIndexMap
                    .FirstOrDefault(entry => entry.Value == i).Key;

                if (countryColor != default) itemImage.color = countryColor;
            }
        }
    }

    private void UpdateCaption()
    {
        send_message_button.interactable = (country_dropdown.value > -1);

        int selectedIndex = country_dropdown.value;
        pattern_img.enabled = true;

        var selectedEntry = countryColorToDropdownIndexMap
            .FirstOrDefault(entry => entry.Value == selectedIndex);

        if (selectedEntry.Key != null)
        {
            country_dropdown.captionImage.color = selectedEntry.Key;
        }
    }

    private void SetDeclareWarMessagePanel()
    {
        message_text.text = "I'm declaring a war on you!";
        ap_text.text = "-1.0";
        SetEffects();

        void onSend()
        {
            Country receiverCountry = map.Countries[countryId];
            receiverCountry.Events.Add(new WarDeclared(map.CurrentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box));
        }

        UpdateSendButton(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetVassalRebelMessagePanel()
    {
        message_text.text = "I don't want to be your vassal anymore. Time to fight for control!";
        ap_text.text = "-1.0";
        ShowPanelWithBasicArea();
    }

    private void SetOfferPeaceMessagePanel()
    {
        message_text.text = "I seek peace. This could be our chance for a better tomorrow.";
        ap_text.text = "-1.0";
        ShowPanelWithBasicArea();
    }

    private void SetDiplomaticMissionMessagePanel()
    {
        message_text.text = "I'm sending a diplomatic mission to you. I want to have good relations with you.";
        ap_text.text = "-1.0";
        ShowPanelWithBasicArea();
    }

    private void SetInsultMessagePanel()
    {
        message_text.text = "You are nothing but a fool!";
        ap_text.text = "-1.0";
        ShowPanelWithBasicArea();
    }

    private void SetOfferAllianceMessagePanel()
    {
        message_text.text = "I'm proposing an alliance. Together, we can be stronger!";
        ap_text.text = "-1.0";
        ShowPanelWithBasicArea();
    }

    private void SetBreakAllianceMessagePanel()
    {
        message_text.text = "I am breaking the alliance with you!";
        ap_text.text = "-0.1";
        ShowPanelWithBasicArea();
    }

    private void SetCallToWarMessagePanel()
    {
        message_text.text = "I'm calling upon you for help in the war. As my ally, we must support each other!";
        ap_text.text = "-0.1";
        SetDropdownCountries();
        ShowPanelWithCountryChoiceArea();
    }

    private void SetSubsidizeMessagePanel()
    {
        message_text.text = "I'm going to subsidize you! Use these resources wisely.";
        ap_text.text = "-1.0";
        SetSlider();

        ShowPanelWithSubsidyArea();
    }

    private void SetEndSubsidiesMessagePanel()
    {
        message_text.text = "I'm ending subsidizing you.";
        ap_text.text = "-0.1";
        ShowPanelWithBasicArea();
    }

    private void SetRequestSubsidiesMessagePanel()
    {
        message_text.text = "I am asking for your support through subsidies.";
        ap_text.text = "-1.0";
        SetSlider();

        ShowPanelWithSubsidyArea();
    }

    private void SetOfferMilitaryAccessMessagePanel()
    {
        message_text.text = "I'm offering you military access. Feel free to pass through my territories.";
        ap_text.text = "-1.0";

        ShowPanelWithBasicArea();
    }

    private void SetEndMilitaryAccessMessagePanel()
    {
        message_text.text = "-0.1"; 
        message_text.text = "Military access has been revoked. Stay vigilant!";
        ShowPanelWithBasicArea();
    }

    private void SetOfferVassalizationMessagePanel()
    {
        message_text.text = "You have little choice but to accept my offer of vassalization.";
        ap_text.text = "-1.0";
        ShowPanelWithBasicArea();
    }

    private void DeactivateAreas()
    {
        message_area.SetActive(false);
        amount_area.SetActive(false);
        country_choice_area.SetActive(false);
        effect_area.SetActive(false);
    }

    private void ShowPanelWithBasicArea()
    {
        send_message_button.interactable = true;
        message_area.SetActive(true);
        amount_area.SetActive(false);
        country_choice_area.SetActive(false);
        effect_area.SetActive(true);
    }

    private void ShowPanelWithCountryChoiceArea()
    {
        send_message_button.interactable = (country_dropdown.value > -1);
        message_area.SetActive(true);
        amount_area.SetActive(false);
        country_choice_area.SetActive(true);
        effect_area.SetActive(true);
    }

    private void ShowPanelWithSubsidyArea()
    {
        send_message_button.interactable = (goldValue > 0 && durationValue > 0);
        message_area.SetActive(true);
        amount_area.SetActive(true);
        country_choice_area.SetActive(false);
        effect_area.SetActive(true);
    }
}