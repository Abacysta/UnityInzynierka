using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static technology_manager;

public class bonus_ui : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text valueText;

    public void SetBonus(Bonus bonus)
    {
        icon.sprite = bonus.Icon;
        string nameWithRoman = bonus.Name;
        if (bonus.Name.StartsWith("Building") || bonus.Name.Contains("tax law") && bonus.IntValue.HasValue)
        {
            nameWithRoman += $" {ToRoman(bonus.IntValue.Value)}";
        }
        nameText.text = nameWithRoman + ":";
        valueText.text = bonus.GetFormattedValue();

        float? value = bonus.NumericValue ?? bonus.IntValue;
        if (value.HasValue) 
        {
            valueText.color = (bonus.IsBonusPositive ^ value < 0) ? 
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