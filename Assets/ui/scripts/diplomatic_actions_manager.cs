using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.map.scripts;
using Assets.classes;
using static Assets.classes.Relation;
using UnityEngineInternal.XR.WSA;

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
    [SerializeField] private camera_controller camera_controller;
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

    // effects
    [SerializeField] private GameObject effect_content;
    [SerializeField] private GameObject effect_row_prefab;

    // buttons
    [SerializeField] private Button war_declare_button;
    [SerializeField] private Button vassal_rebel_button;
    [SerializeField] private Button integrate_vassal_button;
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
    [SerializeField] private Button vassal_offer_button;

    [SerializeField] private Button send_message_button;

    // effect sprites
    [SerializeField] private Sprite happiness_sprite;

    private readonly Dictionary<int, int> countryIdToDropdownIndexMap = new();
    private int countryId;
    private Country receiverCountry;
    private Country currentPlayer;
    private int goldValue = 0;
    private int durationValue = 0;
    private const int maxDurationValue = 1000;
    private float apCost = 0f;
    private readonly Dictionary<int, ReceiverCountryButtonStates> receiverCountryButtonStates = new();

    private class ReceiverCountryButtonStates
    {
        public bool WarDeclareButtonState { get; set; } = true;
        public bool VassaRebelButtonState { get; set; } = true;
        public bool IntegrateVassalButtonState { get; set; } = true;
        public bool PeaceOfferButtonState { get; set; } = true;
        public bool DiplomaticMissionButtonState { get; set; } = true;
        public bool InsultButtonState { get; set; } = true;
        public bool AllianceOfferButtonState { get; set; } = true;
        public bool AllianceBreakButtonState { get; set; } = true;
        public bool CallToWarButtonState { get; set; } = true;
        public bool SubsidizeButtonState { get; set; } = true;
        public bool SubsEndButtonState { get; set; } = true;
        public bool SubsRequestButtonState { get; set; } = true;
        public bool MilAccOfferButtonState { get; set; } = true;
        public bool MilAccEndButtonState { get; set; } = true;
        public bool VassalOfferButtonState { get; set; } = true;
        public List<int> CountriesToSkip { get; set; } = new List<int>();
    }

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        country_dropdown.onValueChanged.AddListener(delegate { UpdateCaption(); });
        
        war_declare_button.onClick.AddListener(SetDeclareWarMessagePanel);
        vassal_rebel_button.onClick.AddListener(SetVassalRebelMessagePanel);
        integrate_vassal_button.onClick.AddListener(SetIntegrateVassalMessagePanel);
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
        vassal_offer_button.onClick.AddListener(SetOfferVassalizationMessagePanel);
    }

    public void ShowDiplomaticActionsInterface(int countryId)
    {
        this.countryId = countryId;
        receiverCountry = map.Countries[countryId];
        currentPlayer = map.CurrentPlayer;

        if (!receiverCountryButtonStates.ContainsKey(countryId))
        {
            receiverCountryButtonStates[countryId] = new();
        }

        SetCountryInfo();
        SetActionButtonStates();

        send_message_button.interactable = false;
        DeactivateAreas();
        gameObject.SetActive(true);
    }

    private void SetCountryInfo()
    {
        receiverCountry.setCoatandColor(country_color_img);
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
        bool HasCurrentPlayerRelationWithReceiver(RelationType type)
        {
            return map.Relations.Any(rel => rel.type == type &&
                rel.Sides.Contains(currentPlayer) && rel.Sides.Contains(receiverCountry));
        }

        bool HasCurrentPlayerRelationWithAnyone(RelationType type)
        {
            return map.Relations.Any(rel => rel.type == type && rel.Sides.Contains(currentPlayer));
        }

        bool HasCurrentPlayerOrderedRelationWithReceiver(RelationType type, bool currentPlayerIsWorseSide)
        {
            if (currentPlayerIsWorseSide)
            {
                // Strona stratniejsza w relacji (side 1)
                return map.Relations.Any(rel => rel.type == type &&
                    rel.Sides[0] == receiverCountry && rel.Sides[1] == currentPlayer);
            }
            else
            {
                // Strona korzystniejsza w relacji (side 0)
                return map.Relations.Any(rel => rel.type == type &&
                    rel.Sides[0] == currentPlayer && rel.Sides[1] == receiverCountry);
            }
        }

        bool HasCurrentPlayerOrderedRelationWithAnyone(RelationType type, bool currentPlayerIsWorseSide)
        {
            if (currentPlayerIsWorseSide)
            {
                // Strona stratniejsza w relacji (side 1)
                return map.Relations.Any(rel => rel.type == type && rel.Sides[1] == currentPlayer);
            }
            else
            {
                // Strona korzystniejsza w relacji (side 0)
                return map.Relations.Any(rel => rel.type == type && rel.Sides[0] == currentPlayer);
            }
        }


        bool HasReceiverOrderedRelationWithAnyone(RelationType type, bool receiverCountryIsWorseSide)
        {
            if (receiverCountryIsWorseSide)
            {
                // Strona stratniejsza w relacji (side 1)
                return map.Relations.Any(rel => rel.type == type && rel.Sides[1] == receiverCountry);
            }
            else
            {
                // Strona korzystniejsza w relacji (side 0)
                return map.Relations.Any(rel => rel.type == type && rel.Sides[0] == receiverCountry);
            }
        }

        bool ArentBothAtSameWar()
        {
            return map.Relations.OfType<War>().Any(war =>
                war.Sides.Contains(currentPlayer) &&
                !war.participants1.Contains(receiverCountry) && !war.participants2.Contains(receiverCountry));
        }

        ReceiverCountryButtonStates buttonStates = receiverCountryButtonStates[countryId];

        // interactable jesli currentPlayer nie ma z odbiorca wojny, sojuszu, rozejmu, dostepu wojskowego, wasalstwa
        // i currentPlayer nie jest wasalem jakiekolwiek kraju
        // DODAC POZNIEJ, ZE interactable jesli BOT, TO MUSI BYC TEZ OPINIA WYSTARCZAJACO NISKA
        war_declare_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.MilitaryAccess) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            buttonStates.WarDeclareButtonState &&
            currentPlayer.Opinions[receiverCountry.Id] < 0;

        // interactable jesli currentPlayer jest wasalem odbiorcy
        vassal_rebel_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            buttonStates.VassalOfferButtonState;

        // interactable jesli currentPlayer jest seniorem odbiorcy
        integrate_vassal_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Vassalage, currentPlayerIsWorseSide: false) &&
            buttonStates.IntegrateVassalButtonState;

        // interactable jesli currentPlayer ma z odbiorca wojne
        peace_offer_button.interactable = HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            buttonStates.PeaceOfferButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny, sojuszu, rozejmu
        // i ani currentPlayer, ani odbiorca nie s¹ wasalami kogokolwiek
        diplomatic_mission_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsWorseSide: true) &&
            buttonStates.DiplomaticMissionButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny, sojuszu i rozejmu
        // i ani currentPlayer, ani odbiorca nie s¹ wasalami kogokolwiek
        insult_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsWorseSide: true) &&
            buttonStates.InsultButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny, sojuszu, rozejmu, wasalstwa
        // i currentPlayer nie jest wasalem jakiekolwiek kraju
        //srodac srozniej mam gdzies ze bot, opinia tak czy siak powinna byc odpowiednia
        // DODAC POZNIEJ, ZE interactable jesli BOT, TO MUSI BYC TEZ OPINIA WYSTARCZAJACO WYSOKA
        alliance_offer_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            buttonStates.AllianceOfferButtonState &&
            receiverCountry.Opinions[currentPlayer.Id] > 2;

        // interactable jesli currentPlayer ma z odbiorca sojusz
        alliance_break_button.interactable = 
            HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            buttonStates.AllianceBreakButtonState;

        // interactable jesli currentPlayer jest z odbiorca w sojuszu i ma z kims wojne,
        // ale sojusznik nie uczestniczy w tej wojnie na razie
        call_to_war_button.interactable =
            HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            ArentBothAtSameWar() &&
            buttonStates.CallToWarButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny i rozejmu,
        // currentPlayer jeszcze nie subsydiuje odbiorcy i nie jest wasalem jakiekolwiek kraju
        subsidize_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Subsidies, currentPlayerIsWorseSide: true) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            buttonStates.SubsidizeButtonState;

        // interactable jesli currentPlayer subsydiuje odbiorce
        subs_end_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Subsidies, currentPlayerIsWorseSide: true) &&
            buttonStates.SubsEndButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny, rozejmu,
        // currentPlayer jeszcze nie jest subsydiowany przez odbiorce
        // ani currentPlayer, ani odbiorca nie s¹ wasalami kogokolwiek
        subs_request_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Subsidies, currentPlayerIsWorseSide: false) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsWorseSide: true) &&
            buttonStates.SubsRequestButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny, sojuszu, rozejmu
        // currentPlayer nie ma jeszcze dostepu wojskowego poprzez kraj odbiorcy,
        // ani currentPlayer, ani odbiorca nie s¹ wasalami kogokolwiek
        mil_acc_offer_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.MilitaryAccess, currentPlayerIsWorseSide: false) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, currentPlayerIsWorseSide: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsWorseSide: true) &&
            buttonStates.MilAccOfferButtonState;

        // interactable jesli currentPlayer daje juz dostep wojskowy odbiorcy i nie ma z nim relacji wasalstwa
        mil_acc_end_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.MilitaryAccess, currentPlayerIsWorseSide: true) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            buttonStates.MilAccEndButtonState;

        // interactable jesli currentPlayer nie ma z odbiorca wojny, sojuszu, rozejmu i wasalstwa
        vassal_offer_button.interactable =
            !HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            buttonStates.VassalOfferButtonState;
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
        });

    }

    private void SetDropdownCountries()
    {
        country_dropdown.ClearOptions();
        countryIdToDropdownIndexMap.Clear();

        var options = map.Countries
            .Where(c => c.Id != currentPlayer.Id &&
                !receiverCountryButtonStates[countryId].CountriesToSkip.Contains(c.Id) &&
                map.Relations.Any(relation =>
                    relation.type == RelationType.War &&
                    relation.Sides.Contains(currentPlayer) &&
                    relation.Sides.Contains(c)))
            .Select((country, index) =>
            {
                countryIdToDropdownIndexMap[country.Id] = index;
                return new TMP_Dropdown.OptionData
                {
                    text = country.Name,
                    image = country.getCoat(),

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
                int countryId = countryIdToDropdownIndexMap
                    .FirstOrDefault(entry => entry.Value == i).Key;

                if (countryId != default) itemImage.color = map.Countries[countryId].Color;
            }
        }
    }

    private void UpdateCaption()
    {
        UpdateSendButtonInteractionForDropdownAction();

        int selectedIndex = country_dropdown.value;

        var selectedEntry = countryIdToDropdownIndexMap
            .FirstOrDefault(entry => entry.Value == selectedIndex);

        if (selectedEntry.Key != default)
        {
            country_dropdown.captionImage.color = map.Countries[selectedEntry.Key].Color;
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
            var action = new actionContainer.TurnAction.start_war(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // declare war, diplomatic mission, send an insult, offer alliance, subsidize
            // request subsidy, request military access, demand vassalization
            receiverCountryButtonStates[countryId].WarDeclareButtonState = false;
            receiverCountryButtonStates[countryId].DiplomaticMissionButtonState = false;
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].AllianceOfferButtonState = false;
            receiverCountryButtonStates[countryId].SubsidizeButtonState = false;
            receiverCountryButtonStates[countryId].SubsRequestButtonState = false;
            receiverCountryButtonStates[countryId].MilAccOfferButtonState = false;
            receiverCountryButtonStates[countryId].VassalOfferButtonState = false;
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
            var action = new actionContainer.TurnAction.vassal_rebel(vassalage, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // rebel
            receiverCountryButtonStates[countryId].VassaRebelButtonState = false;
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(onSend);
        ShowPanelWithBasicArea();
    }

    private void SetIntegrateVassalMessagePanel()
    {
        message_text.text = "I've made the decision about your integration, my vassal.";
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
            var action = new actionContainer.TurnAction.integrate_vassal(vassalage, diplomatic_relations_manager);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // integrate vassal, subsidize, end subsidies, request subsidy
            receiverCountryButtonStates[countryId].IntegrateVassalButtonState = false;
            receiverCountryButtonStates[countryId].SubsidizeButtonState = false;
            receiverCountryButtonStates[countryId].SubsEndButtonState = false;
            receiverCountryButtonStates[countryId].SubsRequestButtonState = false;
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
            var action = new actionContainer.TurnAction.end_war(currentPlayer, war, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // offer peace
            receiverCountryButtonStates[countryId].PeaceOfferButtonState = false;
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
            actionContainer.TurnAction.praise action = new actionContainer.TurnAction.praise(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);
            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // diplomatic mission, declare war, send an insult, offer alliance, demand vassalization
            receiverCountryButtonStates[countryId].DiplomaticMissionButtonState = false;
            receiverCountryButtonStates[countryId].WarDeclareButtonState = false;
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].AllianceOfferButtonState = false;
            receiverCountryButtonStates[countryId].VassalOfferButtonState = false;
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
            actionContainer.TurnAction.insult action = new actionContainer.TurnAction.insult(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);
            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // send an insult, declare war, diplomatic mission, offer alliance, subsidize
            // request subsidy, request military access, demand vassalization
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].WarDeclareButtonState = false;
            receiverCountryButtonStates[countryId].DiplomaticMissionButtonState = false;
            receiverCountryButtonStates[countryId].AllianceOfferButtonState = false;
            receiverCountryButtonStates[countryId].SubsidizeButtonState = false;
            receiverCountryButtonStates[countryId].SubsRequestButtonState = false;
            receiverCountryButtonStates[countryId].MilAccOfferButtonState = false;
            receiverCountryButtonStates[countryId].VassalOfferButtonState = false;
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
            var action = new actionContainer.TurnAction.alliance_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // offer alliance, declare war, diplomatic mission, send an insult, demand vassalization
            receiverCountryButtonStates[countryId].AllianceOfferButtonState = false;
            receiverCountryButtonStates[countryId].WarDeclareButtonState = false;
            receiverCountryButtonStates[countryId].DiplomaticMissionButtonState = false;
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].VassalOfferButtonState = false;
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
            var action = new actionContainer.TurnAction.alliance_end(currentPlayer, alliance, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // break alliance, call to war, subsidize, request subsidy
            receiverCountryButtonStates[countryId].AllianceBreakButtonState = false;
            receiverCountryButtonStates[countryId].CallToWarButtonState = false;
            receiverCountryButtonStates[countryId].SubsidizeButtonState = false;
            receiverCountryButtonStates[countryId].SubsRequestButtonState = false;
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

        void onSend() { 
            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // call to war (jesli to ostatni wybrany kraj w dropdown), break alliance

            UpdateSendButtonInteractionForDropdownAction();
            receiverCountryButtonStates[countryId].CallToWarButtonState = (country_dropdown.options.Count > 1);
            receiverCountryButtonStates[countryId].AllianceBreakButtonState = false;

            int selectedIndex = country_dropdown.value;
            var selectedEntry = countryIdToDropdownIndexMap
                .FirstOrDefault(entry => entry.Value == selectedIndex);
            if (selectedEntry.Key != default) country_dropdown.captionImage.color = map.Countries[selectedEntry.Key].Color;

            receiverCountryButtonStates[countryId].CountriesToSkip.Add(selectedEntry.Key);
            Relation.War war = map.getRelationsOfType(map.Countries[selectedEntry.Key], RelationType.War).First(w => w.Sides.Contains(currentPlayer)) as War;
            var action = new actionContainer.TurnAction.call_to_war(currentPlayer, receiverCountry, war, dialog_box, diplomatic_relations_manager, camera_controller);
            currentPlayer.Actions.addAction(action);
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
            var action = new actionContainer.TurnAction.subs_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, goldValue, durationValue, camera_controller);
            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // subsidize, declare war, integrate vassal, send an insult, end subsidies
            receiverCountryButtonStates[countryId].SubsidizeButtonState = false;
            receiverCountryButtonStates[countryId].WarDeclareButtonState = false;
            receiverCountryButtonStates[countryId].IntegrateVassalButtonState = false;
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].SubsEndButtonState = false;
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
            var relation = map.getRelationsOfType(currentPlayer, RelationType.Subsidies).First(r => r.Sides[1] == receiverCountry);
            if (relation != null)
                diplomatic_relations_manager.endRelation(relation);
            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // end subsidies, subsidize
            receiverCountryButtonStates[countryId].SubsEndButtonState = false;
            receiverCountryButtonStates[countryId].SubsidizeButtonState = false;
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
            receiverCountry.Events.Add(new Event_.DiploEvent.SubsRequest(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, (int)subsidy_slider.value, durationValue, camera_controller));

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // request subsidy
            receiverCountryButtonStates[countryId].SubsRequestButtonState = false;
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
            var action = new actionContainer.TurnAction.access_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // request military access, send an insult, offer alliance, demand vassalization
            receiverCountryButtonStates[countryId].MilAccOfferButtonState = false;
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].AllianceOfferButtonState = false;
            receiverCountryButtonStates[countryId].VassalOfferButtonState = false;
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
            Relation.MilitaryAccess variable = map.getRelationsOfType(receiverCountry, RelationType.MilitaryAccess).First(a => a.Sides[0] == currentPlayer) as Relation.MilitaryAccess; ;
            var action = new Event_.DiploEvent.AccessEndMaster(variable, currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            receiverCountry.Events.Add(action);
            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // end military access
            receiverCountryButtonStates[countryId].MilAccEndButtonState = false;
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
            var action = new actionContainer.TurnAction.vassal_offer(currentPlayer, receiverCountry, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.addAction(action);

            // po wykonaniu tej akcji nie bedzie mozna wybrac nastepujacych akcji:
            // demand vassalization, declare war, diplomatic mission, send an insult,
            // offer alliance, request subsidy, request military access, end military access
            receiverCountryButtonStates[countryId].VassalOfferButtonState = false;
            receiverCountryButtonStates[countryId].WarDeclareButtonState = false;
            receiverCountryButtonStates[countryId].DiplomaticMissionButtonState = false;
            receiverCountryButtonStates[countryId].InsultButtonState = false;
            receiverCountryButtonStates[countryId].AllianceOfferButtonState = false;
            receiverCountryButtonStates[countryId].SubsRequestButtonState = false;
            receiverCountryButtonStates[countryId].MilAccOfferButtonState = false;
            receiverCountryButtonStates[countryId].MilAccEndButtonState = false;
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

    public void ResetRecevierButtonStates()
    {
        receiverCountryButtonStates.Clear();
    }
}