using UnityEngine;
using UnityEngine.EventSystems;

public class draggable_dialog_box : MonoBehaviour, IDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject dialog_box;
    [SerializeField] private RectTransform content_area;
    [SerializeField] private RectTransform draggable_area;

    private RectTransform boxRectTransform;
    private RectTransform canvasRectTransform;
    private float height = 0f;

    void Start()
    {
        boxRectTransform = dialog_box.GetComponent<RectTransform>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        AdjustLayout();
    }

    private void AdjustLayout()
    {
        draggable_area.offsetMax = new Vector2(0f, 0f);
        draggable_area.offsetMin = new Vector2(0f, 0f);
        height = 0f;

        foreach (Transform child in content_area.transform)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();

            if (!child.gameObject.activeSelf)
            {
                height += rectTransform.rect.height;
            }
            else
            {
                // top
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, height);

                // bottom
                rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, height);
            }
        }
        draggable_area.sizeDelta = new Vector2(draggable_area.sizeDelta.x, draggable_area.sizeDelta.y - height);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPosition = boxRectTransform.localPosition + (Vector3)eventData.delta / canvas.scaleFactor;

        float areaWidth = draggable_area.rect.width * draggable_area.localScale.x;
        float areaHeight = draggable_area.rect.height * draggable_area.localScale.y;

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        float invisibleHeightAdjustment = height / 2f;

        float clampedX = Mathf.Clamp(newPosition.x, -canvasWidth / 2 + areaWidth / 2, canvasWidth / 2 - areaWidth / 2);
        float clampedY = Mathf.Clamp(newPosition.y,
            -canvasHeight / 2 + areaHeight / 2 - invisibleHeightAdjustment,
            canvasHeight / 2 - areaHeight / 2 - invisibleHeightAdjustment
        );

        boxRectTransform.localPosition = new Vector2(clampedX, clampedY);
    }
}