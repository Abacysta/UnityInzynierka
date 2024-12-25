using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.map.scripts;
using Assets.classes;
using static Assets.classes.Relation;
using static Assets.classes.TurnAction;
using Assets.classes.subclasses;
using static Assets.classes.subclasses.Constants.RelationConstants;
using Assets.classes.subclasses.Constants;

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
    [SerializeField] private province_interface province_interface;

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
    [SerializeField] private Button mil_acc_request_button;
    [SerializeField] private Button mil_acc_end_master_button;
    [SerializeField] private Button mil_acc_end_slave_button;
    [SerializeField] private Button vassal_offer_button;
    [SerializeField] private Button send_message_button;

    // effect sprites
    [SerializeField] private Sprite happiness_sprite;
    [SerializeField] private Sprite opinion_sprite;

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
        public bool MilAccRequestButtonState { get; set; } = true;
        public bool MilAccEndMasterButtonState { get; set; } = true;
        public bool MilAccEndSlaveButtonState { get; set; } = true;
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
        var bt = gameObject.GetComponentsInChildren<Button>();
        foreach(var b in bt) {
            b.onClick.AddListener(sound_manager.instance.playButton);
        }
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
        mil_acc_request_button.onClick.AddListener(SetRequestMilitaryAccessMessagePanel);
        mil_acc_end_master_button.onClick.AddListener(SetEndMilitaryAccessMasterMessagePanel);
        mil_acc_end_slave_button.onClick.AddListener(SetEndMilitaryAccessSlaveMessagePanel);
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
        province_interface.gameObject.SetActive(false);
    }

    private void SetCountryInfo()
    {
        receiverCountry.SetCoatandColor(country_color_img);

        country_name_txt.text = receiverCountry.Name +
            (map.Controllers[receiverCountry.Id] == Map.CountryController.Ai ? " (AI)" : "");

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
            return map.Relations.Any(rel => rel.Type == type &&
                rel.Sides.Contains(currentPlayer) && rel.Sides.Contains(receiverCountry));
        }

        bool HasCurrentPlayerWarRelationWithReceiver()
        {
            return map.Relations
                .OfType<Relation.War>()
                .Any(warRelation =>
                    (warRelation.Participants1.Contains(currentPlayer) && warRelation.Participants2.Contains(receiverCountry)) ||
                    (warRelation.Participants2.Contains(currentPlayer) && warRelation.Participants1.Contains(receiverCountry))
                );
        }

        bool HasCurrentPlayerOrderedRelationWithReceiver(RelationType type, bool curentPlayerIsSide1)
        {
            if (curentPlayerIsSide1)
            {
                return map.Relations.Any(rel => rel.Type == type &&
                    rel.Sides[0] == receiverCountry && rel.Sides[1] == currentPlayer);
            }
            else
            {
                return map.Relations.Any(rel => rel.Type == type &&
                    rel.Sides[0] == currentPlayer && rel.Sides[1] == receiverCountry);
            }
        }

        bool HasCurrentPlayerOrderedRelationWithAnyone(RelationType type, bool curentPlayerIsSide1)
        {
            if (curentPlayerIsSide1)
            {
                return map.Relations.Any(rel => rel.Type == type && rel.Sides[1] == currentPlayer);
            }
            else
            {
                return map.Relations.Any(rel => rel.Type == type && rel.Sides[0] == currentPlayer);
            }
        }


        bool HasReceiverOrderedRelationWithAnyone(RelationType type, bool receiverCountryIsSide1)
        {
            if (receiverCountryIsSide1)
            {
                return map.Relations.Any(rel => rel.Type == type && rel.Sides[1] == receiverCountry);
            }
            else
            {
                return map.Relations.Any(rel => rel.Type == type && rel.Sides[0] == receiverCountry);
            }
        }

        bool ArentBothAtSameWar()
        {
            return map.Relations.OfType<War>().Any(war =>
                war.Sides.Contains(currentPlayer) &&
                !war.Participants1.Contains(receiverCountry) && !war.Participants2.Contains(receiverCountry));
        }

        ReceiverCountryButtonStates buttonStates = receiverCountryButtonStates[countryId];

        // interactable if the currentPlayer has no war, alliance, truce, military access, or vassalage with the receiver,
        // is not a vassal of any country
        // and if the currentPlayer's opinion of the receiver is less than zero
        war_declare_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.MilitaryAccess) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            buttonStates.WarDeclareButtonState &&
            currentPlayer.Opinions[receiverCountry.Id] < 0;

        // interactable if the currentPlayer is a vassal of the receiver
        vassal_rebel_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            buttonStates.VassalOfferButtonState;

        // interactable if the currentPlayer is the senior of the receiver
        integrate_vassal_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Vassalage, curentPlayerIsSide1: false) &&
            buttonStates.IntegrateVassalButtonState;

        // interactable if the currentPlayer is at war with the receiver as one of the leaders (not an ally who joined war)
        peace_offer_button.interactable = HasCurrentPlayerRelationWithReceiver(RelationType.War) &&
            buttonStates.PeaceOfferButtonState;

        // interactable if the currentPlayer has no war, alliance, or truce with the receiver
        // and neither the currentPlayer nor the receiver are vassals of anyone
        diplomatic_mission_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsSide1: true) &&
            buttonStates.DiplomaticMissionButtonState;

        // interactable if the currentPlayer has no war, alliance, or truce with the receiver 
        // and neither the currentPlayer nor the receiver are vassals of anyone
        insult_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsSide1: true) &&
            buttonStates.InsultButtonState;

        // interactable if the currentPlayer has no war, alliance, truce, or vassalage with the receiver,
        // is not a vassal of any country
        // and if the currentPlayer's opinion of the receiver is greater than 2
        alliance_offer_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            buttonStates.AllianceOfferButtonState &&
            receiverCountry.Opinions[currentPlayer.Id] > 100;

        // interactable if the currentPlayer has an alliance with the receiver
        alliance_break_button.interactable = 
            HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            buttonStates.AllianceBreakButtonState;

        // interactable if the currentPlayer is allied with the receiver and is at war with someone, 
        // but the ally is not currently participating in this war
        call_to_war_button.interactable =
            HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            ArentBothAtSameWar() &&
            buttonStates.CallToWarButtonState;

        // interactable if the currentPlayer has no war and truce with the receiver
        // and if the currentPlayer is not subsidizing the receiver yet and is not a vassal of any country
        subsidize_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Subsidies, curentPlayerIsSide1: false) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            buttonStates.SubsidizeButtonState;

        // interactable if the currentPlayer is subsidizing the receiver
        subs_end_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Subsidies, curentPlayerIsSide1: false) &&
            buttonStates.SubsEndButtonState;

        // interactable if the currentPlayer has no war, truce with the receiver, 
        // the currentPlayer is not being subsidized by the receiver yet
        // and neither the currentPlayer nor the receiver are vassals of anyone
        subs_request_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.Subsidies, curentPlayerIsSide1: true) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsSide1: true) &&
            buttonStates.SubsRequestButtonState;

        // interactable if the currentPlayer has no war, alliance, or truce with the receiver
        // the currentPlayer is not giving military access to the receiver's country yet,
        // and neither the currentPlayer nor the receiver are vassals of anyone
        mil_acc_offer_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.MilitaryAccess, curentPlayerIsSide1: false) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsSide1: true) &&
            buttonStates.MilAccOfferButtonState;

        // interactable if the currentPlayer has no war, alliance, or truce with the receiver
        // the currentPlayer does not have military access through the receiver's country yet,
        // and neither the currentPlayer nor the receiver are vassals of anyone
        mil_acc_request_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Alliance) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Truce) &&
            !HasCurrentPlayerOrderedRelationWithReceiver(RelationType.MilitaryAccess, curentPlayerIsSide1: true) &&
            !HasCurrentPlayerOrderedRelationWithAnyone(RelationType.Vassalage, curentPlayerIsSide1: true) &&
            !HasReceiverOrderedRelationWithAnyone(RelationType.Vassalage, receiverCountryIsSide1: true) &&
            buttonStates.MilAccRequestButtonState;

        // interactable if the currentPlayer is already giving military access to the receiver and has no vassalage relation with him
        mil_acc_end_master_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.MilitaryAccess, curentPlayerIsSide1: false) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            buttonStates.MilAccEndMasterButtonState;

        // interactable if the currentPlayer is already being given military access by the receiver and has no vassalage relation with him
        mil_acc_end_slave_button.interactable =
            HasCurrentPlayerOrderedRelationWithReceiver(RelationType.MilitaryAccess, curentPlayerIsSide1: true) &&
            !HasCurrentPlayerRelationWithReceiver(RelationType.Vassalage) &&
            buttonStates.MilAccEndSlaveButtonState;

        // interactable if the currentPlayer has no war, alliance, truce with the receiver
        vassal_offer_button.interactable =
            !HasCurrentPlayerWarRelationWithReceiver() &&
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
            country_relations_table_manager.SetData();
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
                    relation.Type == RelationType.War &&
                    relation.Sides.Contains(currentPlayer) &&
                    relation.Sides.Contains(c)))
            .Select((country, index) =>
            {
                countryIdToDropdownIndexMap[country.Id] = index;
                return new TMP_Dropdown.OptionData
                {
                    text = country.Name,
                    image = country.GetCoat(),

                };
            })
            .ToList();

        country_dropdown.AddOptions(options);
        country_dropdown.RefreshShownValue();
    }

    private void SetEffects(List<Effect> actionEffects = null)
    {
        foreach (Transform child in effect_content.transform)
        {
            Destroy(child.gameObject);
        }

        if (actionEffects != null && actionEffects.Count > 0)
        {
            foreach (var effect in actionEffects)
            {
                GameObject effectRow = Instantiate(effect_row_prefab, effect_content.transform);
                effect_ui effectUI = effectRow.GetComponent<effect_ui>();
                effectUI.SetEffect(effect);
            }
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

    Relation GetRelationBetweenCurrentPlayerAndReceiver(RelationType type)
    {
        return map.Relations
            .FirstOrDefault(relation => relation.Type == type &&
                relation.Sides.Contains(currentPlayer) &&
                relation.Sides.Contains(receiverCountry));
    }

    private void SetDeclareWarMessagePanel()
    {
        message_text.text = "I'm declaring a war on you!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.StartWar);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(happiness_sprite, "Happiness", $"-{WAR_HAPP_PENALTY_INIT_C1}%", false),
            new(happiness_sprite, "Happiness (Enemy)", $"-{WAR_HAPP_PENALTY_INIT_C2}%", true),
            new(opinion_sprite, "Your Opinion of Them", $"{WAR_OPINION_PENALTY_INIT}", false),
            new(opinion_sprite, "Their Opinion of You", $"{WAR_OPINION_PENALTY_INIT}", false),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.WarDeclaration(currentPlayer, receiverCountry, 
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // declare war, diplomatic mission, send an insult, offer alliance, subsidize
            // request subsidy, offer military access, request military access, demand vassalization
            SetDeclareWarRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetDeclareWarRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].WarDeclareButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].DiplomaticMissionButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsidizeButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
    }

    private void SetVassalRebelMessagePanel()
    {
        message_text.text = "I don't want to be your vassal anymore. Time to fight for control!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.VassalRebel);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(happiness_sprite, "Happiness", $"-{WAR_HAPP_PENALTY_INIT_C1}%", false),
            new(happiness_sprite, "Happiness (Enemy)", $"-{WAR_HAPP_PENALTY_INIT_C2}%", true),
            new(opinion_sprite, "Your Opinion of Them", $"{WAR_OPINION_PENALTY_INIT}", false),
            new(opinion_sprite, "Their Opinion of You", $"{WAR_OPINION_PENALTY_INIT}", false),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            Vassalage vassalage = (Vassalage)GetRelationBetweenCurrentPlayerAndReceiver(RelationType.Vassalage);
            var action = new TurnAction.VassalRebellion(vassalage, diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // rebel
            SetVassalRebelRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetVassalRebelRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].VassaRebelButtonState = buttonState;
    }

    private void SetIntegrateVassalMessagePanel()
    {
        Vassalage vassalage = (Vassalage)GetRelationBetweenCurrentPlayerAndReceiver(RelationType.Vassalage);

        message_text.text = "I've made the decision about your integration, my vassal.";
        apCost = CostCalculator.TurnActionApCost(ActionType.IntegrateVassal, vassalage);
        ap_text.text = $"-{apCost:0.0}";

        SetEffects(null);

        void OnSend()
        {
            var action = new TurnAction.VassalIntegration(vassalage, diplomatic_relations_manager, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // integrate vassal, subsidize, end subsidies, request subsidy
            SetIntegrateVassalRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetIntegrateVassalRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].IntegrateVassalButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsidizeButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsEndButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsRequestButtonState = buttonState;
    }

    private void SetOfferPeaceMessagePanel()
    {
        message_text.text = "I seek peace. This could be our chance for a better tomorrow.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.WarEnd);
        ap_text.text = $"-{apCost:0.0}"; ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(opinion_sprite, "Your Opinion of Them", $"+{TRUCE_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Their Opinion of You", $"+{TRUCE_OPINION_BONUS_INIT}", true),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            War war = (War)GetRelationBetweenCurrentPlayerAndReceiver(RelationType.War);
            var action = new TurnAction.PeaceOffer(currentPlayer, war, diplomatic_relations_manager, dialog_box, camera_controller);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // offer peace
            SetOfferPeaceRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetOfferPeaceRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].PeaceOfferButtonState = buttonState;
    }

    private void SetDiplomaticMissionMessagePanel()
    {
        message_text.text = "I'm sending a diplomatic mission to you. I want to have good relations with you.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.Praise);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(opinion_sprite, "Your Opinion of Them", $"+{PRAISE_OUR_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Their Opinion of You", $"+{PRAISE_THEIR_OPINION_BONUS_INIT}", true)
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.Praise(currentPlayer, receiverCountry, diplomatic_relations_manager, 
                dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // diplomatic mission, declare war, send an insult, offer alliance, demand vassalization
            SetDiplomaticMissionRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetDiplomaticMissionRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].DiplomaticMissionButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].WarDeclareButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
    }

    private void SetInsultMessagePanel()
    {
        message_text.text = "You are nothing but a fool!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.Insult);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(opinion_sprite, "Your Opinion of Them", $"-{INSULT_OUR_OPINION_PENALTY_INIT}", false),
            new(opinion_sprite, "Their Opinion of You", $"-{INSULT_THEIR_OPINION_PENALTY_INIT}", false)
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.Insult(currentPlayer, receiverCountry, 
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // send an insult, declare war, diplomatic mission, offer alliance, subsidize
            // request subsidy, offer military access, request military access, demand vassalization
            SetInsultRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetInsultRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].WarDeclareButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].DiplomaticMissionButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsidizeButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
    }

    private void SetOfferAllianceMessagePanel()
    {
        message_text.text = "I'm proposing an alliance. Together, we can be stronger!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.AllianceOffer);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(happiness_sprite, "Happiness (Both)", $"+{ALLIANCE_HAPP_BONUS_INIT}%", true),
            new(happiness_sprite, "Happiness Per Turn (Both)", $"+{ALLIANCE_HAPP_BONUS_CONST}%", true),
            new(opinion_sprite, "Your Opinion of Them", $"+{ALLIANCE_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Their Opinion of You", $"+{ALLIANCE_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Your Opinion of Them Per Turn", $"+{ALLIANCE_OPINION_BONUS_CONST}", true),
            new(opinion_sprite, "Their Opinion of You Per Turn", $"+{ALLIANCE_OPINION_BONUS_CONST}", true),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.AllianceOffer(currentPlayer, receiverCountry, 
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // offer alliance, declare war, diplomatic mission, send an insult, demand vassalization
            SetOfferAllianceRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetOfferAllianceRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].WarDeclareButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].DiplomaticMissionButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
    }

    private void SetBreakAllianceMessagePanel()
    {
        message_text.text = "I am breaking the alliance with you!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.AllianceEnd);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(opinion_sprite, "Your Opinion of Them", $"+{TRUCE_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Their Opinion of You", $"+{TRUCE_OPINION_BONUS_INIT}", true),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            Alliance alliance = (Alliance)GetRelationBetweenCurrentPlayerAndReceiver(RelationType.Alliance);
            var action = new TurnAction.AllianceBreak(currentPlayer, alliance, diplomatic_relations_manager, 
                dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // break alliance, call to war, subsidize, request subsidy
            SetBreakAllianceRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetBreakAllianceRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].AllianceBreakButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].CallToWarButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsidizeButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsRequestButtonState = buttonState;
    }

    private void SetCallToWarMessagePanel()
    {
        message_text.text = "I'm calling upon you for help in the war. As my ally, we must support each other!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.CallToWar);
        ap_text.text = $"-{apCost:0.0}";

        SetDropdownCountries();

        SetEffects(null);

        void OnSend() 
        { 
            // after this action, the following actions will not be selectable:
            // call to war (jesli to ostatni wybrany kraj w dropdown), break alliance

            UpdateSendButtonInteractionForDropdownAction();
            receiverCountryButtonStates[countryId].CallToWarButtonState = (country_dropdown.options.Count > 1);
            receiverCountryButtonStates[countryId].AllianceBreakButtonState = false;

            int selectedIndex = country_dropdown.value;
            var selectedEntry = countryIdToDropdownIndexMap
                .FirstOrDefault(entry => entry.Value == selectedIndex);

            receiverCountryButtonStates[countryId].CountriesToSkip.Add(selectedEntry.Key);
            Relation.War war = map.GetRelationsOfType(map.Countries[selectedEntry.Key], RelationType.War).First(w => w.Sides.Contains(currentPlayer)) as War;
            var action = new TurnAction.CallToWar(currentPlayer, receiverCountry, war, dialog_box, diplomatic_relations_manager, camera_controller, this);
            currentPlayer.Actions.AddAction(action);
        }

        UpdateSendButtonInteractionForDropdownAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithCountryChoiceArea();
    }

    public void RevertCallToWarRelatedButtonStates(int receiverCountryId)
    {
        if (receiverCountryButtonStates[receiverCountryId].CountriesToSkip.Count > 0) {
            receiverCountryButtonStates[receiverCountryId].CountriesToSkip
                .RemoveAt(receiverCountryButtonStates[receiverCountryId].CountriesToSkip.Count - 1);
        }
        if (receiverCountryButtonStates[receiverCountryId].CountriesToSkip.Count == 0) {
            receiverCountryButtonStates[receiverCountryId].AllianceBreakButtonState = true;
        }
        receiverCountryButtonStates[receiverCountryId].CallToWarButtonState = (country_dropdown.options.Count > 0);
    }

    private void SetSubsidizeMessagePanel()
    {
        message_text.text = "I'm offering to subsidize you! Use these resources wisely.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.SubsOffer);
        ap_text.text = $"-{apCost:0.0}";
        SetSlider();

        List<Effect> actionEffects = new()
        {
            new(opinion_sprite, "Your Opinion of Them", $"+{SUBSIDIES_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Their Opinion of You", $"+{SUBSIDIES_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Your Opinion of Them Per Turn", $"+{SUBSIDIES_OPINION_BONUS_CONST}", true),
            new(opinion_sprite, "Their Opinion of You Per Turn", $"+{SUBSIDIES_OPINION_BONUS_CONST}", true),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.SubsOffer(currentPlayer, receiverCountry, diplomatic_relations_manager, 
                dialog_box, goldValue, durationValue, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // subsidize, declare war, integrate vassal, send an insult, end subsidies
            SetSubsidizeRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForSliderAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithSubsidyArea();
    }

    public void SetSubsidizeRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].SubsidizeButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].WarDeclareButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].IntegrateVassalButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsEndButtonState = buttonState;
    }

    private void SetEndSubsidiesMessagePanel()
    {
        message_text.text = "I'm ending subsidizing you.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.SubsEnd);
        ap_text.text = $"-{apCost:0.0}";

        SetEffects(null);

        void OnSend()
        {
            Subsidies subsidies = map.GetRelationsOfType(currentPlayer, RelationType.Subsidies).First(r => r.Sides[1] == receiverCountry) as Subsidies;
            var action = new TurnAction.SubsEnd(currentPlayer, receiverCountry, subsidies,
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // end subsidies, subsidize
            SetEndSubsidiesRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetEndSubsidiesRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].SubsEndButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsidizeButtonState = buttonState;
    }

    private void SetRequestSubsidiesMessagePanel()
    {
        message_text.text = "I am asking for your support through subsidies.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.SubsRequest);
        ap_text.text = $"-{apCost:0.0}";
        SetSlider();

        List<Effect> actionEffects = new()
        {
            new(opinion_sprite, "Your Opinion of Them", $"+{SUBSIDIES_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Their Opinion of You", $"+{SUBSIDIES_OPINION_BONUS_INIT}", true),
            new(opinion_sprite, "Your Opinion of Them Per Turn", $"+{SUBSIDIES_OPINION_BONUS_CONST}", true),
            new(opinion_sprite, "Their Opinion of You Per Turn", $"+{SUBSIDIES_OPINION_BONUS_CONST}", true),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.SubsRequest(currentPlayer, receiverCountry, diplomatic_relations_manager,
                dialog_box, goldValue, durationValue, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // request subsidy
            SetRequestSubsidiesRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForSliderAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithSubsidyArea();
    }

    public void SetRequestSubsidiesRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].SubsRequestButtonState = buttonState;
    }

    private void SetOfferMilitaryAccessMessagePanel()
    {
        message_text.text = "I'm offering you military access. Feel free to pass through my territories.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.MilAccOffer);
        ap_text.text = $"-{apCost:0.0}";

        SetEffects(null);

        void OnSend()
        {
            var action = new TurnAction.MilAccessOffer(currentPlayer, receiverCountry, 
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // offer military access, send an insult, offer alliance, demand vassalization
            SetOfferMilitaryAccessRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetOfferMilitaryAccessRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].MilAccOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
    }

    private void SetRequestMilitaryAccessMessagePanel()
    {
        message_text.text = "I kindly request military access to your territories. Allow my forces to pass through your lands.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.MilAccRequest);
        ap_text.text = $"-{apCost:0.0}";

        SetEffects(null);

        void OnSend()
        {
            var action = new TurnAction.MilAccessRequest(currentPlayer, receiverCountry,
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // request military access, send an insult, offer alliance, demand vassalization
            SetRequestMilitaryAccessRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetRequestMilitaryAccessRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].MilAccRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
    }

    private void SetEndMilitaryAccessMasterMessagePanel()
    {
        message_text.text = "Military access has been revoked. Stay vigilant!";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.MilAccEndMaster);
        ap_text.text = $"-{apCost:0.0}";

        SetEffects(null);

        void OnSend()
        {
            MilitaryAccess militaryAccess = map.GetRelationsOfType(receiverCountry, 
                RelationType.MilitaryAccess).First(a => a.Sides[0] == currentPlayer) as MilitaryAccess;
            var action = new TurnAction.MilAccessEndMaster(currentPlayer, receiverCountry, militaryAccess,
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // end military access master
            SetEndMilitaryAccessMasterRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetEndMilitaryAccessMasterRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].MilAccEndMasterButtonState = buttonState;
    }

    private void SetEndMilitaryAccessSlaveMessagePanel()
    {
        message_text.text = "I am stopping the use of the military access you granted to me.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.MilAccEndSlave);
        ap_text.text = $"-{apCost:0.0}";

        SetEffects(null);

        void OnSend()
        {
            MilitaryAccess militaryAccess = map.GetRelationsOfType(receiverCountry,
                RelationType.MilitaryAccess).First(a => a.Sides[1] == currentPlayer) as MilitaryAccess;
            var action = new TurnAction.MilAccessEndSlave(currentPlayer, receiverCountry, militaryAccess,
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // end military access slave
            SetEndMilitaryAccessSlaveRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetEndMilitaryAccessSlaveRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].MilAccEndSlaveButtonState = buttonState;
    }

    private void SetOfferVassalizationMessagePanel()
    {
        message_text.text = "You have little choice but to accept my offer of vassalization.";
        apCost = CostCalculator.GetTurnActionApCost(ActionType.VassalizationOffer);
        ap_text.text = $"-{apCost:0.0}";

        List<Effect> actionEffects = new()
        {
            new(happiness_sprite, "Happiness", $"+{VASSALAGE_HAPP_BONUS_INIT_C1}%", true),
            new(happiness_sprite, "Happiness (Enemy)", $"-{VASSALAGE_HAPP_PENALTY_INIT_C2}%", true),
            new(happiness_sprite, "Happiness Per Turn", $"+{VASSALAGE_HAPP_BONUS_CONST_C1}%", true),
            new(happiness_sprite, "Happiness Per Turn (Enemy)", $"-{VASSALAGE_HAPP_PENALTY_CONST_C2}%", true),
            new(opinion_sprite, "Their Opinion of You", $"{VASSALAGE_OPINION_PENALTY_INIT_C2}", false),
            new(opinion_sprite, "Their Opinion of You (Per Turn)", $"{VASSALAGE_OPINION_PENALTY_CONST_C2}", false),
        };

        SetEffects(actionEffects);

        void OnSend()
        {
            var action = new TurnAction.VassalizationDemand(currentPlayer, receiverCountry, 
                diplomatic_relations_manager, dialog_box, camera_controller, this);
            currentPlayer.Actions.AddAction(action);

            // after this action, the following actions will not be selectable:
            // demand vassalization, declare war, diplomatic mission, send an insult,
            // offer alliance, request subsidy, offer military access, request military access, end military access
            SetOfferVassalizationRelatedButtonStates(false, countryId);
        }

        UpdateSendButtonInteractionForBasicAction();
        SetSendButtonAction(OnSend);
        ShowPanelWithBasicArea();
    }

    public void SetOfferVassalizationRelatedButtonStates(bool buttonState, int receiverCountryId)
    {
        receiverCountryButtonStates[receiverCountryId].VassalOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].WarDeclareButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].DiplomaticMissionButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].InsultButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].AllianceOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].SubsRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccOfferButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccRequestButtonState = buttonState;
        receiverCountryButtonStates[receiverCountryId].MilAccEndMasterButtonState = buttonState;
    }

    private void DeactivateAreas()
    {
        message_area.SetActive(false);
        amount_area.SetActive(false);
        country_choice_area.SetActive(false);
        effect_area.SetActive(false);
        effect_content.SetActive(false);
    }

    private void ShowPanelWithBasicArea()
    {
        message_area.SetActive(true);
        amount_area.SetActive(false);
        country_choice_area.SetActive(false);
        effect_area.SetActive(true);
        effect_content.SetActive(true);
    }

    private void ShowPanelWithCountryChoiceArea()
    {
        message_area.SetActive(true);
        amount_area.SetActive(false);
        country_choice_area.SetActive(true);
        effect_area.SetActive(true);
        effect_content.SetActive(true);
    }

    private void ShowPanelWithSubsidyArea()
    {
        message_area.SetActive(true);
        amount_area.SetActive(true);
        country_choice_area.SetActive(false);
        effect_area.SetActive(true);
        effect_content.SetActive(true);
    }

    public void ResetReceiverButtonStates()
    {
        receiverCountryButtonStates.Clear();
    }
}