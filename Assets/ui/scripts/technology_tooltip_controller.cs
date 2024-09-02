using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class technology_tooltip_controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltip;

    private Coroutine showTooltipCoroutine;

    void Start()
    {
        tooltip.SetActive(false);
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
        tooltip.SetActive(true);
    }
}