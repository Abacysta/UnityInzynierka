using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class cursor_helper : MonoBehaviour
{
    protected bool IsCursorOverUIObject()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem is not found!");
            return false;
        }

        PointerEventData eventDataCurrentPosition = new(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("IgnoreRaycast")) return false;
        }

        return results.Count > 0;
    }

    protected bool IsCursorOverArmy()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<army_view>(out var armyView))
            {
                return true;
            }
        }

        return false;
    }
}