using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class technology_tooltip_controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltip;

    private Coroutine showTooltipCoroutine;
    private RectTransform tooltipRectTransform;
    private RectTransform iconRectTransform;
    private Vector2 tooltipOffset = new(40f, 0f);

    void Start()
    {
        tooltip.SetActive(false);
        tooltipRectTransform = tooltip.GetComponent<RectTransform>();
        iconRectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
        }
        showTooltipCoroutine = StartCoroutine(ShowTooltip());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            Input.mousePosition,
            eventData.enterEventCamera))
        {
            if (showTooltipCoroutine != null)
            {
                StopCoroutine(showTooltipCoroutine);
                showTooltipCoroutine = null;
            }
            tooltip.SetActive(false);
        }
    }

    private IEnumerator ShowTooltip()
    {
        yield return new WaitForSeconds(0.5f);

        Vector3 tooltipPosition = iconRectTransform.position + (Vector3)tooltipOffset;
        tooltipRectTransform.position = tooltipPosition;
        tooltip.SetActive(true);
    }
}