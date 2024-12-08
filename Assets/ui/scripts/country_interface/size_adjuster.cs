using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class size_adjuster : MonoBehaviour
{
    [SerializeField] private int minFontSize = 11;
    [SerializeField] private int maxFontSize = 17;
    [SerializeField] private int minResolutionHeight = 664;
    [SerializeField] private int maxResolutionHeight = 1080;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1f;

    private TMP_Text[] textComponents;
    private Image[] imageComponents;
 
    void Start()
    {
        textComponents = GetComponentsInChildren<TMP_Text>();
        imageComponents = GetComponentsInChildren<Image>();

        AdjustFontSizes();
        AdjustObjectScales();
    }

    void AdjustFontSizes()
    {
        int currentHeight = Screen.height;

        float newFontSize = Mathf.Clamp(
            Mathf.Lerp(minFontSize, maxFontSize, (float)(currentHeight - minResolutionHeight) / (maxResolutionHeight - minResolutionHeight)),
            minFontSize,
            maxFontSize
        );

        foreach (var textComponent in textComponents)
        {
            if (textComponent.text.Contains("effects")) {
                textComponent.fontSize = newFontSize + 1;
                textComponent.fontSizeMax = newFontSize + 1;
                textComponent.fontSizeMin = minFontSize;
            }
            else {
                textComponent.fontSize = newFontSize;
                textComponent.fontSizeMax = newFontSize;
                textComponent.fontSizeMin = minFontSize;
            }
        }
    }

    void AdjustObjectScales()
    {
        int currentHeight = Screen.height;

        float newScale = Mathf.Clamp(
            Mathf.Lerp(minScale, maxScale, (float)(currentHeight - minResolutionHeight) / (maxResolutionHeight - minResolutionHeight)),
            minScale,
            maxScale
        );

        foreach (var imageComponent in imageComponents)
        {
            if (imageComponent.transform.parent != null && imageComponent.transform.parent.name.Contains("row"))
            {
                imageComponent.gameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
    }

    void Update()
    {
        textComponents = GetComponentsInChildren<TMP_Text>();
        imageComponents = GetComponentsInChildren<Image>();

        AdjustFontSizes();
        AdjustObjectScales();
    }
}
