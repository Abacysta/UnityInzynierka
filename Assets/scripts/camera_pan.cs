using UnityEngine;

public class camera_pan : MonoBehaviour
{
    [SerializeField] private float dragSpeed = 15f;
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private float minX = -40f, maxX = 40f, minY = -40f, maxY = 40f;
    public GameObject blocker;

    private Vector3 dragOrigin;
    private Vector2 hotSpot = Vector2.zero;
    //private bool drag=false;
    //private float dragThreshold = 0.5f;

    void Update()
    {
        if (blocker != null && blocker.activeSelf) {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto); // Zmieñ na w³aœciwy kursor
            //drag = false;
            return;
        }

        if(!Input.GetMouseButton(1)) {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Przywróæ domyœlny kursor
            return;
        }
        
            //drag = true;
            Vector3 pos = Camera.main.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
            Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);

            Vector3 newPosition = transform.position + move;

            // Ograniczenie pozycji kamery
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            transform.position = newPosition;

            dragOrigin = Input.mousePosition; // Aktualizuj pozycjê przeci¹gania
    }

    //public bool isDrag() { return drag; }
}


