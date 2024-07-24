using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    [SerializeField] private float dragSpeed = 15f;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private float minX = -4f, maxX = 4f, minY = -4f, maxY = 4f;

    private Vector3 dragOrigin;
    private Vector2 hotSpot = Vector2.zero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto); // Zmieñ na w³aœciwy kursor
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Przywróæ domyœlny kursor
            return;
        }

        Vector3 pos = Camera.main.ScreenToViewportPoint(dragOrigin - Input.mousePosition); 
        Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);

        Vector3 newPosition = transform.position + move;

        // Ograniczenie pozycji kamery
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;

        dragOrigin = Input.mousePosition; // Aktualizuj pozycjê przeci¹gania
    }
}


