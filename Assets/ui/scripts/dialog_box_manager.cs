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

    public class DialogConfig
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Action OnConfirm { get; set; }
        public Action OnCancel { get; set; }
        public Action OnClose { get; set; }
        public Action OnZoom {  get; set; }
        public bool Confirmable { get; set; } = true;
        public bool Rejectable { get; set; } = true;
        public bool Zoomable { get; set; } = false;
        public string ConfirmText { get; set; } = "OK";
        public string CancelText { get; set; } = "Cancel";
    }

    public class EffectsDialogConfig : DialogConfig
    {
        public Dictionary<Resource, float> Cost { get; set; } = new();
        public List<Effect> Effects { get; set; } = new();
    }

    public class SliderDialogConfig : EffectsDialogConfig
    {
        public int MaxValue { get; set; }
        public int AffordableValue { get; set; }
    }

    public class DialogBoxBuilder
    {
        private DialogConfig _dialogConfig = new DialogConfig();

        public DialogBoxBuilder SetBasicParams(DialogConfig basicParameters)
        {
            _dialogConfig.Title = basicParameters.Title;
            _dialogConfig.Message = basicParameters.Message;
            _dialogConfig.OnConfirm = basicParameters.OnConfirm;
            _dialogConfig.OnCancel = basicParameters.OnCancel;
            _dialogConfig.OnClose = basicParameters.OnClose;
            _dialogConfig.OnZoom = basicParameters.OnZoom;
            _dialogConfig.Confirmable = basicParameters.Confirmable;
            _dialogConfig.Rejectable = basicParameters.Rejectable;
            _dialogConfig.Zoomable = basicParameters.Zoomable;
            _dialogConfig.ConfirmText = basicParameters.ConfirmText;
            _dialogConfig.CancelText = basicParameters.CancelText;
            return this;
        }

        public DialogBoxBuilder SetSliderParams(int affordableValue, int maxValue)
        {
            if (_dialogConfig is not SliderDialogConfig sliderConfig)
            {
                sliderConfig = new SliderDialogConfig();
                CopyBaseProperties(_dialogConfig, sliderConfig);
                _dialogConfig = sliderConfig;
            }

            sliderConfig.MaxValue = maxValue;
            sliderConfig.AffordableValue = affordableValue;
            return this;
        }

        public DialogBoxBuilder SetEffectsParams(Dictionary<Resource, float> cost = null, List<Effect> effects = null)
        {
            if (_dialogConfig is not EffectsDialogConfig effectsConfig)
            {
                effectsConfig = new EffectsDialogConfig();
                CopyBaseProperties(_dialogConfig, effectsConfig);
                _dialogConfig = effectsConfig;
            }

            effectsConfig.Cost = cost ?? new Dictionary<Resource, float>();
            effectsConfig.Effects = effects ?? new List<Effect>();
            return this;
        }

        public DialogConfig Build()
        {
            return _dialogConfig;
        }

        private void CopyBaseProperties(DialogConfig source, DialogConfig target)
        {
            target.Title = source.Title;
            target.Message = source.Message;
            target.OnConfirm = source.OnConfirm;
            target.OnCancel = source.OnCancel;
            target.OnClose = source.OnClose;
            target.Confirmable = source.Confirmable;
            target.Rejectable = source.Rejectable;
            target.Zoomable = source.Zoomable;
            target.ConfirmText = source.ConfirmText;
            target.CancelText = source.CancelText;
        }
    }

    void Update()
    {
        if (gameObject.activeSelf) 
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                gameObject.SetActive(false);
                overlay.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.LeftShift)) 
            {
                cancel_button.onClick.Invoke();
                overlay.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Return)) 
            {
                confirm_button.onClick.Invoke();
                overlay.SetActive(false);
            }
        }
    }

    public void InvokeArmyBox(Army army, (int, int) destination) 
    {
        var cost = CostsCalculator.TurnActionFullCost(ActionType.ArmyMove);
        int affordableValue = map.CurrentPlayer
            .CalculateMaxArmyUnits(CostsCalculator.TurnActionFullCost(ActionType.ArmyMove), army.Count);

        var basicParameters = new DialogConfig
        {
            Title = "Move Army",
            Message = "Select how many units you want to move.",
            OnConfirm = () =>
            {
                var action = new ArmyMove(army.Position, destination, (int)dialog_slider.value, army);
                map.Countries[army.OwnerId].Actions.addAction(action);
            }
        };

        var sliderWithEffectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetSliderParams(affordableValue, army.Count)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(sliderWithEffectsBoxParameters);
    }

    public void InvokeRecBox((int, int) coordinates)
    {
        var province = map.getProvince(coordinates);
        var techStats = map.Countries[province.OwnerId].techStats;
        var cost = CostsCalculator.TurnActionFullCost(ActionType.ArmyRecruitment, techStats);
        int affordableValue = map.CurrentPlayer.CalculateMaxArmyUnits(cost, province.RecruitablePopulation);

        var basicParameters = new DialogConfig
        {
            Title = "Recruit Army",
            Message = "Select how many units you want to recruit.",
            OnConfirm = () =>
            {
                var action = new ArmyRecruitment(coordinates, (int)dialog_slider.value, techStats);
                map.Countries[province.OwnerId].Actions.addAction(action);
            }
        };

        var sliderWithEffectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetSliderParams(affordableValue, province.RecruitablePopulation)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(sliderWithEffectsBoxParameters);
    }

    public void InvokeDisbandArmyBox(Army army)
    {
        var cost = CostsCalculator.TurnActionFullCost(ActionType.ArmyDisbandment);

        var basicParameters = new DialogConfig
        {
            Title = "Disband Army",
            Message = "Select how many units you want to disband.",
            OnConfirm = () =>
            {
                int unitsToDisband = (int)dialog_slider.value;
                var action = new ArmyDisbandment(army, unitsToDisband);
                map.Countries[army.OwnerId].Actions.addAction(action);
            }
        };

        int affordableValue = map.CurrentPlayer.CalculateMaxArmyUnits(cost, army.Count);

        var sliderWithEffectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetSliderParams(affordableValue, army.Count)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(sliderWithEffectsBoxParameters);
    }

    public void InvokeUpgradeBuilding((int, int) coordinates, BuildingType buildingType) 
    {
        var province = map.getProvince(coordinates);
        int lvl = province.Buildings.ContainsKey(buildingType) ? province.Buildings[buildingType] + 1 : 0;
        var cost = CostsCalculator.TurnActionFullCost(ActionType.BuildingUpgrade, buildingType, lvl);

        var basicParameters = new DialogConfig
        {
            Title = $"Build {buildingType} - level {lvl}",
            Message = $"Do you want to build {buildingType} - level {lvl}?",
            OnConfirm = () =>
            {
                var action = new BuildingUpgrade(province, buildingType);
                map.CurrentPlayer.Actions.addAction(action);
            },
            Confirmable = map.CurrentPlayer.isPayable(cost)
        };

        var effectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(effectsBoxParameters);
    }

    public void InvokeDowngradeBuilding((int, int) coordinates, BuildingType buildingType) {
        var province = map.getProvince(coordinates);
        int lvl = province.Buildings.ContainsKey(buildingType) ? province.Buildings[buildingType] : 0;
        var cost = CostsCalculator.TurnActionFullCost(ActionType.BuildingDowngrade);

        var basicParameters = new DialogConfig
        {
            Title = $"Raze {buildingType} - level {lvl}",
            Message = $"Do you want to raze {buildingType} - level {lvl}?",
            OnConfirm = () =>
            {
                var action = new BuildingDowngrade(province, buildingType);
                map.CurrentPlayer.Actions.addAction(action);
            },
            Confirmable = map.CurrentPlayer.isPayable(cost)
        };

        var effectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(effectsBoxParameters);
    }

    public void InvokeTechUpgradeBox(Technology technologyType)
    {
        int lvl = map.CurrentPlayer.Technologies[technologyType] + 1;
        var cost = CostsCalculator.TurnActionFullCost(ActionType.TechnologyUpgrade,
            tech: map.CurrentPlayer.Technologies, techType: technologyType);

        var basicParameters = new DialogConfig
        {
            Title = "Upgrade Technology",
            Message = $"Do you want to upgrade " +
                $"{technologyType.ToString().ToLower()} technology to level {lvl}?",
            OnConfirm = () =>
            {
                var action = new TechnologyUpgrade(map.CurrentPlayer, technologyType);
                map.CurrentPlayer.Actions.addAction(action);
                technology_manager.UpdateData();
            },
            Confirmable = map.CurrentPlayer.isPayable(cost)
        };

        var effectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(effectsBoxParameters);
    }

    public void InvokeTaxBreakIntroductionBox((int, int) coordinates)
    {
        var province = map.getProvince(coordinates);
        var cost = CostsCalculator.TurnActionFullCost(ActionType.TaxBreakIntroduction);

        List<Effect> effects = new()
        {
            new(tax_mod_sprite, "Tax", $"{TaxBreak.TaxMod}%", true),
            new(happiness_sprite, "Happiness growth", $"{(TaxBreak.HappMod >= 0 ? "+" : "")}{TaxBreak.HappMod * 100}%", true),
            new(happiness_sprite, "Happiness", $"{(TaxBreak.HappStatic >= 0 ? "+" : "")}{TaxBreak.HappStatic}", true)
        };

        var basicParameters = new DialogConfig
        {
            Title = "Introduce Tax Break",
            Message = $"Do you want to introduce a tax break?",
            OnConfirm = () =>
            {
                var action = new TaxBreakIntroduction(province);
                map.CurrentPlayer.Actions.addAction(action);
            },
            Confirmable = map.CurrentPlayer.isPayable(cost)
        };

        var effectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetEffectsParams(cost, effects)
            .Build();

        ShowDialogBox(effectsBoxParameters);
    }

    public void InvokeFestivitiesOrganizationBox((int, int) coordinates)
    {
        var province = map.getProvince(coordinates);
        var cost = CostsCalculator.TurnActionFullCost(ActionType.FestivitiesOrganization);

        List<Effect> effects = new()
        {
            new(prod_mod_sprite, "Production", $"{(Festivities.ProdMod >= 0 ? "+" : "")}{Festivities.ProdMod * 100}%", false),
            new(pop_mod_sprite, "Population growth", $"{(Festivities.PopMod >= 0 ? "+" : "")}{Festivities.PopMod * 100}%", true),
            new(happiness_sprite, "Happiness", $"{(Festivities.HappStatic >= 0 ? "+" : "")}{Festivities.HappStatic}", true)
        };

        var basicParameters = new DialogConfig
        {
            Title = "Organize Festivities",
            Message = $"Do you want to organize festivities?",
            OnConfirm = () =>
            {
                var action = new FestivitiesOrganization(province);
                map.CurrentPlayer.Actions.addAction(action);
            },
            Confirmable = map.CurrentPlayer.isPayable(cost)
        };

        var effectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetEffectsParams(cost, effects)
            .Build();

        ShowDialogBox(effectsBoxParameters);
    }

    public void InvokeRebelSuppressionBox((int, int) coordinates)
    {
        var province = map.getProvince(coordinates);
        var cost = CostsCalculator.TurnActionFullCost(ActionType.RebelSuppresion);

        var basicParameters = new DialogConfig
        {
            Title = "Suppress the Rebellion",
            Message = $"Do you want to suppress the rebellion?",
            OnConfirm = () =>
            {
                var action = new RebelSuppresion(province);
                map.CurrentPlayer.Actions.addAction(action);
            },
            Confirmable = map.CurrentPlayer.isPayable(cost)
        };

        var effectsBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .SetEffectsParams(cost)
            .Build();

        ShowDialogBox(effectsBoxParameters);
    }

    public void InvokeConfirmBox(string title, string message, Action onConfirm) 
    {
        var basicParameters = new DialogConfig
        {
            Title = title,
            Message = message,
            OnConfirm = onConfirm
        };

        var basicBoxParameters = new DialogBoxBuilder()
            .SetBasicParams(basicParameters)
            .Build();

        ShowDialogBox(basicBoxParameters);
    }

    public void InvokeEventBox(Event_ _event)
    {
        // Search for a non-static, public, non-inherited method named "reject"
        MethodInfo rejectMethod = _event.GetType().GetMethod("reject", 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        // If the reject method exists and has a body (braces), it is rejectable
        bool rejectable = rejectMethod != null && rejectMethod.GetMethodBody() != null;

        var basicParameters = new DialogConfig
        {
            Title = "",
            Message = _event.Message,
            OnConfirm = () => {
                _event.accept();
                map.CurrentPlayer.Events.Remove(_event);
                alerts.sortedevents.Remove(_event);
                alerts.reloadAlerts();
            },
            OnCancel = () => {
                _event.reject();
                map.CurrentPlayer.Events.Remove(_event);
                alerts.sortedevents.Remove(_event);
                alerts.reloadAlerts();
            },
            OnZoom = () => {
                _event.zoom();
            },
            Confirmable = _event.Cost != null ? map.CurrentPlayer.isPayable(_event.Cost) : true,
            Rejectable = rejectable,
            Zoomable = true,
            ConfirmText = "Confirm",
            CancelText = "Reject"
        };

        var eventBox = new DialogBoxBuilder()
            .SetBasicParams(basicParameters);
        if (_event.Cost != null) eventBox.SetEffectsParams(_event.Cost);
        var eventBoxParameters = eventBox.Build();

        ShowDialogBox(eventBoxParameters);
    }

    private void ShowDialogBox(DialogConfig parameters)
    {
        choice_element.SetActive(false);
        cost_element.SetActive(false);

        SetCoatOfArms();
        SetDialogTexts(parameters);

        if (parameters is SliderDialogConfig sliderParameters)
        {
            SetSlider(sliderParameters);
        }
        else if (parameters is EffectsDialogConfig effectParameters)
        {
            SetCostContent(effectParameters.Cost, effectParameters.Effects);
        }

        SetDialogButtons(parameters);
        CenterDialogBox();
        ShowDialog();
    }

    private void SetCoatOfArms()
    {
        if (map.CurrentPlayerId > 0 && map.Countries.Count > 0)
        {
            map.CurrentPlayer.setCoatandColor(db_country_color_img);
            db_country_color_img.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            db_country_color_img.transform.parent.gameObject.SetActive(false);
        }
    }

    private void SetDialogTexts(DialogConfig parameters)
    {
        confirm_button.GetComponentInChildren<TMP_Text>().SetText(parameters.ConfirmText);
        cancel_button.GetComponentInChildren<TMP_Text>().SetText(parameters.CancelText);

        dialog_title.text = parameters.Title;
        dialog_message.text = parameters.Message;
    }

    private void SetDialogButtons(DialogConfig parameters)
    {
        close_button.onClick.RemoveAllListeners();
        confirm_button.onClick.RemoveAllListeners();
        cancel_button.onClick.RemoveAllListeners();
        zoom_button.onClick.RemoveAllListeners();

        close_button.onClick.AddListener(() =>
        {
            parameters.OnClose?.Invoke();
            HideDialog();
        });

        confirm_button.interactable = parameters.Confirmable;
        confirm_button.onClick.AddListener(() =>
        {
            parameters.OnConfirm?.Invoke();
            HideDialog();
        });

        cancel_button.interactable = parameters.Rejectable;
        cancel_button.onClick.AddListener(() =>
        {
            parameters.OnCancel?.Invoke();
            click_sound.Play();
            HideDialog();
        });

        zoom_button.gameObject.SetActive(parameters.Zoomable);
        zoom_button.onClick.AddListener(() => {
            parameters.OnZoom?.Invoke();
            HideDialog();
        });
    }

    private void SetSlider(SliderDialogConfig parameters) {
        dialog_slider.value = 0;
        quantity_text.text = dialog_slider.value.ToString();
        dialog_slider.maxValue = parameters.MaxValue;
        slider_max.text = parameters.MaxValue.ToString();

        confirm_button.interactable = false;

        SetCostContent(parameters.Cost, parameters.Effects, dialog_slider.value);

        dialog_slider.onValueChanged.RemoveAllListeners();
        dialog_slider.onValueChanged.AddListener((value) =>
        {
            quantity_text.text = value.ToString();
            SetCostContent(parameters.Cost, parameters.Effects, dialog_slider.value);
            confirm_button.interactable = value > 0 && value <= parameters.AffordableValue;
        });

        choice_element.SetActive(true);
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

    private void SetCostContent(Dictionary<Resource, float> cost = null, 
        List<Effect> effects = null, float? sliderValue = null)
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
        cost_element.SetActive(true);
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