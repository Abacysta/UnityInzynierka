using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class effect_ui : MonoBehaviour
{
    [SerializeField] private Image effect_icon;
    [SerializeField] private TMP_Text effect_name;
    [SerializeField] private TMP_Text effect_value;

    public void SetEffect(Effect effect)
    {
        effect_icon.sprite = effect.Icon;
        effect_name.text = effect.Name;
        effect_value.text = effect.Value;
        effect_value.color = (effect.IsEffectPositive) ?
                    new Color32(0, 159, 18, 255) : // green
                    new Color32(255, 83, 83, 255); // red
    }
}
