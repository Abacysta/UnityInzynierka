using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static technology_manager;

public class tech_effect_ui : MonoBehaviour
{
    [SerializeField] private Image effect_icon;
    [SerializeField] private TMP_Text effect_name;
    [SerializeField] private TMP_Text effect_value;

    public void SetEffect(TechEffect effect)
    {
        effect_icon.sprite = effect.Icon;
        string name = effect.Name;
        if (effect.IntValue.HasValue && effect.Name.StartsWith("Building"))
        {
            name += $" {ToRoman(effect.IntValue.Value)}";
        }
        effect_name.text = name + ":";
        effect_value.text = effect.GetFormattedValue();

        float? value = effect.NumericValue ?? effect.IntValue;
        if (value.HasValue) 
        {
            if (value == 0)
            {
                effect_value.color = new Color32(255, 204, 0, 255); // yellow
            }
            else
            {
                effect_value.color = (effect.IsEffectPositive ^ value < 0) ?
                    new Color32(0, 159, 18, 255) : // green
                    new Color32(180, 25, 37, 255); // red
            }
        }
        else if (effect.BoolValue.HasValue)
        {
            effect_value.color = effect.IsEffectPositive ^ !effect.BoolValue.Value ?
                new Color32(0, 159, 18, 255) : // green
                new Color32(180, 25, 37, 255); // red
        }
    }

    private string ToRoman(int number)
    {
        if (number < 1 || number > 5)
            return "N/A";

        string[] romanNumerals = { "I", "II", "III", "IV", "V" };
        return romanNumerals[number - 1];
    }
}