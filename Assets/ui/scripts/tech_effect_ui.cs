using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static technology_manager;

public class tech_effect_ui : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;

    public void SetEffect(TechEffect effect)
    {
        icon.sprite = effect.Icon;
        string nameWithRoman = effect.Name;
        if (effect.Name.StartsWith("Building") || effect.Name.Contains("tax law") && effect.IntValue.HasValue)
        {
            nameWithRoman += $" {ToRoman(effect.IntValue.Value)}";
        }
        nameText.text = nameWithRoman + ":";
        valueText.text = effect.GetFormattedValue();

        float? value = effect.NumericValue ?? effect.IntValue;
        if (value.HasValue) 
        {
            valueText.color = (effect.IsEffectPositive ^ value < 0) ? 
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