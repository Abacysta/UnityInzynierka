using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class legend_item_ui : MonoBehaviour
{
    [SerializeField] private Image color_img;
    [SerializeField] private TMP_Text item_text;

    public void SetLegendItem(Color color, string description)
    {
        color_img.color = color;
        item_text.text = description;
    }
}