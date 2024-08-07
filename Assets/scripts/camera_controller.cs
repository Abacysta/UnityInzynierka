using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class camera_controller : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private Tilemap tile_map_layer_1;
    [SerializeField] private Texture2D default_cursor;
    [SerializeField] private Texture2D pan_cursor;
    [SerializeField] private GameObject settings_menu;

    [SerializeField] private float minZoom = 30f;
    [SerializeField] private float maxZoom = 100f;
    [SerializeField] private float minPanSpeed = 1f;
    [SerializeField] private float maxPanSpeed = 15f;
    [SerializeField] private float minZoomSpeed = 5f;
    [SerializeField] private float maxZoomSpeed = 100f;
    [SerializeField] private float minScalingMargin = 10f;

    private float minX = -200f, maxX = 200f, minY = -200f, maxY = 200f;
    private float mapWorldHeight;
    private float mapWorldWidth;
    private float cameraSizeAfterScaling;
    private float centerX;
    private float centerY;
    private float panSpeed;
    private float zoomSpeed;

    private Vector3 panOrigin;
    private Vector3 lastPanPosition;
    private bool isPanning = false;

    void Start()
    {
        CalculateMapCenter();
        CalculateCameraBounds();
        CalculateCameraScalingSize();

        CenterAndScaleCamera();
    }

    void Update()
    {
        HandleCameraPan();
        HandleKeyboardPan();
        HandleCameraZoom();
    }

    void CalculateMapCenter()
    {
        int mapHexGridHeight = 80, mapHexGridWidth = 80;

        // Max tilemap coordinates
        mapHexGridHeight = map.Provinces.Max(p => p.Y);
        mapHexGridWidth = map.Provinces.Max(p => p.X);

        Vector3Int maxCellPosition = new(mapHexGridWidth, mapHexGridHeight, 0);

        // Max world coordinates
        mapWorldWidth = tile_map_layer_1.CellToWorld(maxCellPosition).x;
        mapWorldHeight = tile_map_layer_1.CellToWorld(maxCellPosition).y;

        centerX = mapWorldWidth / 2f;
        centerY = mapWorldHeight / 2f;
    }

    void CalculateCameraBounds()
    {
        minX = 0f;
        maxX = mapWorldWidth;
        minY = 0f;
        maxY = mapWorldHeight;
    }

    void CalculateCameraScalingSize()
    {
        Camera camera = Camera.main;

        float mapHeightWithMargin = mapWorldHeight * (1f + minScalingMargin * 0.01f);
        float mapWidthWithMargin = mapWorldWidth * (1f + minScalingMargin * 0.01f);

        float aspectRatio = camera.aspect;
        float orthographicSizeHeight = mapHeightWithMargin / 2f;
        float orthographicSizeWidth = mapWidthWithMargin / (2f * aspectRatio);

        cameraSizeAfterScaling = Mathf.Max(orthographicSizeHeight, orthographicSizeWidth);

        // Additional setting the maximum size of zooming out based on the map size
        maxZoom = cameraSizeAfterScaling * 2.5f;
    }

    void HandleCameraPan()
    {
        // Disabling camera panning while using the settings menu
        // and when the cursor is over UI
        if ((settings_menu != null && settings_menu.activeSelf) ||
            IsCursorOverUIObject())
        {
            Cursor.SetCursor(default_cursor, Vector2.zero, CursorMode.Auto);
            return;
        }

        // onMouseDown
        if (Input.GetMouseButtonDown(1))
        {
            panOrigin = Input.mousePosition;
            lastPanPosition = panOrigin;
            isPanning = true;
            Cursor.SetCursor(pan_cursor, Vector2.zero, CursorMode.Auto);
        }

        // onMouseUp
        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
            Cursor.SetCursor(default_cursor, Vector2.zero, CursorMode.Auto);
        }

        if (isPanning)
        {
            Vector3 currentPanPosition = Input.mousePosition;
            Vector3 difference = lastPanPosition - currentPanPosition;

            if (difference != Vector3.zero)
            {
                Vector3 move = new Vector3(difference.x * panSpeed * Time.deltaTime, difference.y * panSpeed * Time.deltaTime, 0);
                Vector3 newPosition = transform.position + move;

                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

                transform.position = newPosition;

                lastPanPosition = currentPanPosition;
            }
        }
    }

    void HandleKeyboardPan()
    {
        if (settings_menu != null && settings_menu.activeSelf) return;
        if (IsCursorOverUIObject()) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float adjustedPanSpeed = panSpeed * 10f;
        Vector3 move = new(horizontal * adjustedPanSpeed * Time.deltaTime, vertical * adjustedPanSpeed * Time.deltaTime, 0);
        Vector3 newPosition = transform.position + move;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

        transform.position = newPosition;
    }

    void HandleCameraZoom()
    {
        if (settings_menu != null && settings_menu.activeSelf) return;
        if (IsCursorOverUIObject()) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            float size = Camera.main.orthographicSize - scroll * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(size, minZoom, maxZoom);
        }

        UpdateSpeedSettings();
    }

    void UpdateSpeedSettings()
    {
        float cameraSize = Camera.main.orthographicSize;
        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, cameraSize);

        panSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, zoomFactor * 0.7f);
        zoomSpeed = Mathf.Lerp(minZoomSpeed, maxZoomSpeed, zoomFactor);
    }

    bool IsCursorOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    public void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(centerX, centerY, transform.position.z);
    }

    public void ScaleCameraToMapSize()
    {
        Camera.main.orthographicSize = cameraSizeAfterScaling;
    }

    public void CenterAndScaleCamera()
    {
        CenterCamera();
        ScaleCameraToMapSize();
    }
}
