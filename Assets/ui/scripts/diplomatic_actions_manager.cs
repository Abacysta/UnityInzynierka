using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.map.scripts;
using Assets.classes;
using static Assets.classes.Relation;

public class Effect
{
    public Sprite Icon { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    public bool IsEffectPositive { get; set; }

    public Effect(Sprite icon, string name, string value, bool isEffectPositive)
    {
        Icon = icon;
        Name = name;
        Value = value;
        IsEffectPositive = isEffectPositive;
    }
}

public class diplomatic_actions_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private diplomatic_relations_manager diplomatic_relations_manager;
    [SerializeField] private country_relations_table_manager country_relations_table_manager;

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

    // effect sprites
    [SerializeField] private Sprite happiness_sprite;

    private Dictionary<Color, int> countryColorToDropdownIndexMap = new();
    private int countryId;
    private Country receiverCountry;
    private Country currentPlayer;
    private int goldValue = 0;
    private int durationValue = 0;
    private const int maxDurationValue = 1000;
    private float apCost = 0f;

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
        textElement.color = opinion == 0 ? Color.yellow : (opinion < 0 ? new Color32(255, 83, 83, 255) : Color.green);
        textElement.text = opinion == 0 ? "0" : (opinion > 0 ? "+" + opinion : opinion.ToString());
    }

    private void SetActionButtonStates()
    {
        bool HasRelation(RelationType type)
        {
            return map.Relations.Any(rel => rel.type == type &&
                rel.Sides.Contains(currentPlayer) && rel.Sides.Contains(receiverCountry));
        }

        bool HasOrderedRelation(RelationType type, bool currentPlayerIsSide0)
        {
            if (currentPlayerIsSide0)
            {
                // Strona stratniejsza w relacji (side 0)
                return map.Relations.Any(rel => rel.type == type &&
                    rel.Sides[0] == currentPlayer && rel.Sides[1] == receiverCountry);
            }
            else
            {
                // Strona korzystniejsza w relacji  (side 1)
                return map.Relations.Any(rel => rel.type == type &&
                    rel.Sides[1] == currentPlayer && rel.Sides[0] == receiverCountry);
            }
        }

        bool ArentBothAtSameWar()
        {
            return map.Relations.OfType<War>().Any(war =>
                war.Sides.Contains(currentPlayer) &&
                !war.participants1.Contains(receiverCountry) && !war.participants2.Contains(receiverCountry));
        }

        // jesli currentPlayer jest wasalem nadawcy
        vasal_rebel_button.interactable =
            HasOrderedRelation(RelationType.Vassalage, currentPlayerIsSide0: true);

        // jesli currentPlayer nie ma z nadawca wojny, rozejmu, dostepu wojskowego i wasalstwa
        // DODAC POZNIEJ, ZE JESLI BOT, TO MUSI BYC TEZ OPINIA WYSTARCZAJACO NISKA
        war_declare_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce) &&
            !HasRelation(RelationType.MilitaryAccess) &&
            !HasRelation(RelationType.Vassalage);

        // jesli currentPlayer ma z nadawca wojne
        peace_offer_button.interactable = HasRelation(RelationType.War);

        // jesli currentPlayer nie ma z nadawca wojny i rozejmu
        diplomatic_mission_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce);

        // jesli currentPlayer nie ma z nadawca wojny i rozejmu
        insult_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce);

        // jesli currentPlayer nie ma z nadawca wojny, rozejmu, wasalstwa
        // DODAC POZNIEJ, ZE JESLI BOT, TO MUSI BYC TEZ OPINIA WYSTARCZAJACO WYSOKA
        alliance_offer_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce) &&
            !HasRelation(RelationType.Vassalage);

        // jesli currentPlayer ma z nadawca sojusz
        alliance_break_button.interactable = HasRelation(RelationType.Alliance);

        // jesli currentPlayer jest z nadawca w sojuszu i ma z kims wojne,
        // ale sojusznik nie uczestniczy w tej wojnie na razie
        call_to_war_button.interactable =
            HasRelation(RelationType.Alliance) &&
            ArentBothAtSameWar();

        // jesli currentPlayer nie ma z nadawca wojny i rozejmu oraz
        // currentPlayer jeszcze nie subsydiuje nadawcy
        subsidize_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce) &&
            !HasOrderedRelation(RelationType.Subsidies, currentPlayerIsSide0: true);

        // jesli currentPlayer subsydiuje nadawce
        subs_end_button.interactable =
            HasOrderedRelation(RelationType.Subsidies, currentPlayerIsSide0: true);

        // jesli currentPlayer nie ma z nadawca wojny i rozejmu oraz
        // currentPlayer jeszcze nie jest subsydiowany przez nadawce
        subs_request_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce) &&
            !HasOrderedRelation(RelationType.Subsidies, currentPlayerIsSide0: false);

        // jesli currentPlayer nie ma z nadawca wojny i rozejmu
        // i nie ma dostepu wojskowego poprzez kraj nadawcy
        mil_acc_offer_button.interactable =
            !HasRelation(RelationType.War) &&
            !HasRelation(RelationType.Truce) &&
            !HasOrderedRelation(RelationType.MilitaryAccess, currentPlayerIsSide0: false);

        // jesli currentPlayer daje juz dostep wojskowy nadawcy
        mil_acc_end_button.interactable =
            HasOrderedRelation(RelationType.MilitaryAccess, currentPlayerIsSide0: true);

        // jesli currentPlayer nie ma relacji wasalstwo z nadawca
        vasal_offer_button.interactable =
            !HasRelation(RelationType.Vassalage);
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
            UpdateSendButtonInteractionForSliderAction();
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
        UpdateSendButtonInteractionForSliderAction();
    }

    private void UpdateSendButtonInteractionForSliderAction()
    {
        send_message_button.interactable = (goldValue > 0 && durationValue > 0 && currentPlayer.Resources[Resource.AP] >= apCost);
    }

    private void UpdateSendButtonInteractionForDropdownAction()
    {
        send_message_button.interactable = (country_dropdown.value > -1 && currentPlayer.Resources[Resource.AP] >= apCost);
    }

    private void UpdateSendButtonInteractionForBasicAction()
    {
        send_message_button.interactable = currentPlayer.Resources[Resource.AP] >= apCost;
    }

    private void SetSendButtonAction(System.Action onSend)
    {
        send_message_button.onClick.RemoveAllListeners();
        send_message_button.onClick.AddListener(() => {
            onSend?.Invoke();
            gameObject.SetActive(false);
            country_relations_table_manager.SetData();
        });

    }

    private void SetDropdownCountries()
    {
        country_dropdown.ClearOptions();
        countryColorToDropdownIndexMap.Clear();

        var options = map.Countries
            .Where(c => c.Id != currentPlayer.Id &&
                map.Relations.Any(relation =>
                    relation.type == RelationType.War &&
                    relation.Sides.Contains(currentPlayer) &&
                    relation.Sides.Contains(c)))
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

    private void SetEffects(List<Effect> action_effects)
    {
        foreach (Transform child in effect_content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var effect in action_effects)
        {
            GameObject effectRow = Instantiate(effect_row_prefab, effect_content.transform);
            effect_ui effectUI = effectRow.GetComponent<effect_ui>();
            effectUI.SetEffect(effect);
        }
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
        UpdateSendButtonInteractionForDropdownAction();

        int selectedIndex = country_dropdown.value;
        pattern_img.enabled = true;

        var selectedEntry = countryColorToDropdownIndexMap
            .FirstOrDefault(entry => entry.Value == selectedIndex);

        if (selectedEntry.Key != null)
        {
            country_dropdown.captionImage.color = selectedEntry.Key;
        }
    }

    Relation HasRelation(RelationType type)
    {
        return map.Relations
            .FirstOrDefault(relation => relation.type == type &&
                relation.Sides.Contains(currentPlayer) &&
                relation.Sides.Contains(receiverCountry));
    }

    private void SetDeclareWarMessagePanel()
    {
        message_text.text = "I'm declaring a war on you!";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> action_effects = new()
        {
            new(happiness_sprite, "Happiness 1x", "-15%", false),
            new(happiness_sprite, "Happiness Every Turn", "-2%", false),
            new(happiness_sprite, "Happiness (Enemy)", "-1%", false)
        };

        SetEffects(action_effects);

        void onSend()
        {
            var action = new actionContainer.TurnAction.start_war(receiverCountry, currentPlayer, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetVassalRebelMessagePanel()
    {
        message_text.text = "I don't want to be your vassal anymore. Time to fight for control!";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> action_effects = new()
        {
            new(happiness_sprite, "Happiness 1x", "5%", true),
            new(happiness_sprite, "Happiness (Enemy)", "-5%", false)
        };

        SetEffects(action_effects);

        void onSend()
        {
            Vassalage vassalage = (Vassalage)HasRelation(RelationType.Vassalage);
            var action = new actionContainer.TurnAction.vassal_rebel(vassalage, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetOfferPeaceMessagePanel()
    {
        message_text.text = "I seek peace. This could be our chance for a better tomorrow.";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {
            War war = (War)HasRelation(RelationType.War);
            var action = new actionContainer.TurnAction.end_war(currentPlayer, war, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetDiplomaticMissionMessagePanel()
    {
        message_text.text = "I'm sending a diplomatic mission to you. I want to have good relations with you.";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {

        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetInsultMessagePanel()
    {
        message_text.text = "You are nothing but a fool!";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {

        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetOfferAllianceMessagePanel()
    {
        message_text.text = "I'm proposing an alliance. Together, we can be stronger!";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {
            var action = new actionContainer.TurnAction.alliance_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetBreakAllianceMessagePanel()
    {
        message_text.text = "I am breaking the alliance with you!";
        apCost = 0.1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {
            Alliance alliance = (Alliance)HasRelation(RelationType.Alliance);
            var action = new actionContainer.TurnAction.alliance_end(currentPlayer, alliance, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetCallToWarMessagePanel()
    {
        message_text.text = "I'm calling upon you for help in the war. As my ally, we must support each other!";
        apCost = 0.1f;
        ap_text.text = $"-{apCost:0.0}";
        SetDropdownCountries();

        void onSend()
        {

        }

        UpdateSendButtonInteractionForDropdownAction();
        SetSendButtonAction(onSend);
        ShowPanelWithCountryChoiceArea();
    }

    private void SetSubsidizeMessagePanel()
    {
        message_text.text = "I'm going to subsidize you! Use these resources wisely.";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";
        SetSlider();

        void onSend()
        {
            var action = new actionContainer.TurnAction.subs_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box,
                goldValue, durationValue);
            action.execute(map);
        }

        UpdateSendButtonInteractionForSliderAction();
        SetSendButtonAction(onSend);
        ShowPanelWithSubsidyArea();
    }

    private void SetEndSubsidiesMessagePanel()
    {
        message_text.text = "I'm ending subsidizing you.";
        apCost = 0.1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {

        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetRequestSubsidiesMessagePanel()
    {
        message_text.text = "I am asking for your support through subsidies.";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";
        SetSlider();

        void onSend()
        {

        }

        UpdateSendButtonInteractionForSliderAction();
        SetSendButtonAction(onSend);
        ShowPanelWithSubsidyArea();
    }

    private void SetOfferMilitaryAccessMessagePanel()
    {
        message_text.text = "I'm offering you military access. Feel free to pass through my territories.";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {
            var action = new actionContainer.TurnAction.access_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetEndMilitaryAccessMessagePanel()
    {
        message_text.text = "Military access has been revoked. Stay vigilant!";
        apCost = 0.1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {

        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetOfferVassalizationMessagePanel()
    {
        message_text.text = "You have little choice but to accept my offer of vassalization.";
        apCost = 1f;
        ap_text.text = $"-{apCost:0.0}";

        void onSend()
        {
            var action = new actionContainer.TurnAction.vassal_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box);
            action.execute(map);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
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
        message_area.SetActive(true);
        amount_area.SetActive(false);
        country_choice_area.SetActive(true);
        effect_area.SetActive(true);
    }

    private void ShowPanelWithSubsidyArea()
    {
        message_area.SetActive(true);
        amount_area.SetActive(true);
        country_choice_area.SetActive(false);
        effect_area.SetActive(true);
    }
}