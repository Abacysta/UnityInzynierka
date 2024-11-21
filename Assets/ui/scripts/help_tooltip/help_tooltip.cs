using System.Collections;
using TMPro;
using UnityEngine;

public class help_tooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text help_desc;

    private Coroutine tooltip;
    private GameObject container;
    private string info;

    public string Info { get => info; set => info = value; }


    private void Start()
    {
        container = gameObject.transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.y += 100f;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (mousePos.x + container.GetComponent<RectTransform>().rect.width > screenWidth)
        {
            mousePos.x = screenWidth - container.GetComponent<RectTransform>().rect.width;
        }

        if (mousePos.y + container.GetComponent<RectTransform>().rect.height > screenHeight)
        {
            mousePos.y = screenHeight - container.GetComponent<RectTransform>().rect.height;
        }

        transform.position = mousePos;
    }

    public void OnMouseEnterElement()
    {
        if (tooltip != null)
        {
            StopCoroutine(tooltip);
        }
        tooltip = StartCoroutine(ShowTooltip());
    }

    public void OnMouseExitElement()
    {
        if (tooltip != null)
        {
            StopCoroutine(tooltip);
            tooltip = null;
        }
        container.SetActive(false);
    }

    public IEnumerator ShowTooltip()
    {
        yield return new WaitForSeconds(0.5f);
        SetTooltipData();
        container.SetActive(true);
    }

    public void SetTooltipData()
    {
        help_desc.text = info;
    }
}
