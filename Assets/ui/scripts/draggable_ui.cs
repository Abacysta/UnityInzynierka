using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject ui;

    private RectTransform uiRectTransform;
    private RectTransform canvasRectTransform;

    void Start()
    {
        uiRectTransform = ui.GetComponent<RectTransform>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition = uiRectTransform.anchoredPosition + eventData.delta / canvas.scaleFactor;

        float boxWidth = uiRectTransform.rect.width * uiRectTransform.localScale.x;
        float boxHeight = uiRectTransform.rect.height * uiRectTransform.localScale.y;

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        float clampedX = Mathf.Clamp(newPosition.x, -canvasWidth / 2 + boxWidth / 2, canvasWidth / 2 - boxWidth / 2);
        float clampedY = Mathf.Clamp(newPosition.y, -canvasHeight / 2 + boxHeight / 2, canvasHeight / 2 - boxHeight / 2);

        uiRectTransform.anchoredPosition = new Vector2(clampedX, clampedY);
    }
}