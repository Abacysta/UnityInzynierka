using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject dialog_box;

    private RectTransform dbRectTransform;
    private RectTransform canvasRectTransform;

    void Start()
    {
        dbRectTransform = dialog_box.GetComponent<RectTransform>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition = dbRectTransform.anchoredPosition + eventData.delta / canvas.scaleFactor;

        float boxWidth = dbRectTransform.rect.width * dbRectTransform.localScale.x;
        float boxHeight = dbRectTransform.rect.height * dbRectTransform.localScale.y;

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        float clampedX = Mathf.Clamp(newPosition.x, -canvasWidth / 2 + boxWidth / 2, canvasWidth / 2 - boxWidth / 2);
        float clampedY = Mathf.Clamp(newPosition.y, -canvasHeight / 2 + boxHeight / 2, canvasHeight / 2 - boxHeight / 2);

        dbRectTransform.anchoredPosition = new Vector2(clampedX, clampedY);
    }
}
