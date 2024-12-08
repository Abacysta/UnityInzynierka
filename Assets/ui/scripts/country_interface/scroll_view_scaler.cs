using Mosframe;
using UnityEngine;

public class scroll_view_scaler : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform row_prefab;
    [SerializeField] private RectTransform scroll_view_content;
    [SerializeField] private DynamicVScrollView dynamic_vscroll_view_script;
    [SerializeField] private int rowsVisible = 13;

    void Update()
    {
        float viewportHeight = viewport.rect.height;
        float rowHeight = viewportHeight / rowsVisible;

        row_prefab.sizeDelta = new Vector2(row_prefab.sizeDelta.x, rowHeight);

        for (int i = 0; i < scroll_view_content.childCount; i++)
        {
            RectTransform child = scroll_view_content.GetChild(i) as RectTransform;
            if (child != null)
            {
                child.sizeDelta = new Vector2(child.sizeDelta.x, rowHeight);
            }
        }
        dynamic_vscroll_view_script.refresh();
    }
}
