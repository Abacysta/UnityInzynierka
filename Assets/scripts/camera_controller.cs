using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;

public class camera_controller : cursor_helper
{
    [SerializeField] private Map map;
    [SerializeField] private Tilemap tile_map_layer_1;
    [SerializeField] private Texture2D default_cursor;
    [SerializeField] private Texture2D pan_cursor;
    [SerializeField] private province_click_handler province_click_handler;

    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 100f;
    [SerializeField] private float minPanSpeed = 5f;
    [SerializeField] private float maxPanSpeed = 100f;
    [SerializeField] private float minZoomSpeed = 5f;
    [SerializeField] private float maxZoomSpeed = 100f;
    [SerializeField] private float minScalingMargin = 10f;
    [SerializeField] private float panSpeed;
    [SerializeField] private float zoomSpeed;

    private Vector2 baseResolution = new(1920, 1080);
    private float resolutionScaleFactor;
    private Camera mainCamera;

    private readonly float mapMinX = 0f, mapMinY = 0f;
    private float mapMaxX = 200f, mapMaxY = 200f;
    private float cameraSizeForMap;
    private float mapCenterX, mapCenterY;

    private Vector3 panOrigin;
    private Vector3 lastPanPosition;
    private bool isPanning = false;

    private float countryMinX, countryMinY, countryMaxX, countryMaxY;
    private float cameraSizeForCountry;
    private float countryCenterX, countryCenterY;

    public bool IsPanning { get => isPanning; private set => isPanning = value; }

    void Start()
    {
        resolutionScaleFactor = ((float)Screen.width / baseResolution.x + (float)Screen.height / baseResolution.y) / 2f;
        mainCamera = Camera.main;

        CalculateMapCenterAndBounds();
        CalculateCameraSizeForMap();

        ZoomCameraOnCountry(map.currentPlayer);
    }

    void Update()
    {
        if (IsCursorOverUIObject())
        {
            isPanning = false;
            Cursor.SetCursor(default_cursor, Vector2.zero, CursorMode.Auto);
            return;
        }

        HandleCameraPan();
        HandleKeyboardPan();
        HandleCameraZoom();
    }

    private void CalculateMapCenterAndBounds()
    {
        int mapHexGridHeight = 80, mapHexGridWidth = 80;

        // Max tilemap coordinates
        if (map.Provinces.Any())
        {
            mapHexGridHeight = map.Provinces.Max(p => p.Y);
            mapHexGridWidth = map.Provinces.Max(p => p.X);
        }

        Vector3Int maxCellPosition = new(mapHexGridWidth, mapHexGridHeight, 0);

        // Max world coordinates
        mapMaxX = tile_map_layer_1.CellToWorld(maxCellPosition).x;
        mapMaxY = tile_map_layer_1.CellToWorld(maxCellPosition).y;

        mapCenterX = mapMaxX / 2f;
        mapCenterY = mapMaxY / 2f;
    }

    private void CalculateCameraSizeForMap()
    {
        float mapHeightWithMargin = mapMaxY * (1f + minScalingMargin * 0.01f);
        float mapWidthWithMargin = mapMaxX * (1f + minScalingMargin * 0.01f);

        float aspectRatio = mainCamera.aspect;
        float orthographicSizeHeight = mapHeightWithMargin / 2f;
        float orthographicSizeWidth = mapWidthWithMargin / (2f * aspectRatio);

        cameraSizeForMap = Mathf.Max(orthographicSizeHeight, orthographicSizeWidth);

        // Additional setting the maximum size of zooming out based on the map size
        maxZoom = cameraSizeForMap * 2.5f;
    }

    private void HandleCameraPan()
    {
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
                Vector3 move = new(difference.x * panSpeed * Time.deltaTime * resolutionScaleFactor,
                                      difference.y * panSpeed * Time.deltaTime * resolutionScaleFactor, 0);
                Vector3 newPosition = transform.position + move;

                newPosition.x = Mathf.Clamp(newPosition.x, mapMinX, mapMaxX);
                newPosition.y = Mathf.Clamp(newPosition.y, mapMinY, mapMaxY);

                transform.position = newPosition;

                lastPanPosition = currentPanPosition;
            }
        }
    }

    private void HandleKeyboardPan()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float adjustedPanSpeed = panSpeed * 3f * resolutionScaleFactor;
        Vector3 move = new(horizontal * adjustedPanSpeed * Time.deltaTime, vertical * adjustedPanSpeed * Time.deltaTime, 0);
        Vector3 newPosition = transform.position + move;

        newPosition.x = Mathf.Clamp(newPosition.x, mapMinX, mapMaxX);
        newPosition.y = Mathf.Clamp(newPosition.y, mapMinY, mapMaxY);

        transform.position = newPosition;
    }

    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0.0f)
        {
            float size = mainCamera.orthographicSize - scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(size, minZoom, maxZoom);
        }

        UpdateSpeedSettings();
    }

    private void UpdateSpeedSettings()
    {
        float cameraSize = mainCamera.orthographicSize;

        float zoomFactor = Mathf.InverseLerp(maxZoom, minZoom, cameraSize);
        float steepFactor = Mathf.Pow(zoomFactor, 2); 

        panSpeed = Mathf.Lerp(maxPanSpeed, minPanSpeed, steepFactor);
        zoomSpeed = Mathf.Lerp(maxZoomSpeed, minZoomSpeed, zoomFactor);
    }

    public void CenterAndScaleCamera()
    {
        mainCamera.transform.position = new Vector3(mapCenterX, mapCenterY, transform.position.z);
        mainCamera.orthographicSize = cameraSizeForMap;
    }

    private void CalculateCountryCenterAndBounds(Country country)
    {
        int hexCountryMinX = 0, hexCountryMinY = 0;
        int hexCountryMaxX = 80, hexCountryMaxY = 80;

        // Max country tilemap coordinates
        hexCountryMinX = country.Provinces.Min(p => p.X);
        hexCountryMinY = country.Provinces.Min(p => p.Y);
        hexCountryMaxX = country.Provinces.Max(p => p.X);
        hexCountryMaxY = country.Provinces.Max(p => p.Y);

        Vector3Int minCellPosition = new(hexCountryMinX, hexCountryMinY, 0);
        Vector3Int maxCellPosition = new(hexCountryMaxX, hexCountryMaxY, 0);

        // Max world coordinates
        countryMinX = tile_map_layer_1.CellToWorld(minCellPosition).x;
        countryMinY = tile_map_layer_1.CellToWorld(minCellPosition).y;
        countryMaxX = tile_map_layer_1.CellToWorld(maxCellPosition).x;
        countryMaxY = tile_map_layer_1.CellToWorld(maxCellPosition).y;

        countryCenterX = (countryMinX + countryMaxX) / 2f;
        countryCenterY = (countryMinY + countryMaxY) / 2f;
    }

    private void CalculateCameraSizeForCountry(Country country)
    {
        float countryWidth = countryMaxX - countryMinX;
        float countryHeight = countryMaxY - countryMinY;

        float countryHeightWithMargin = countryHeight * (1.5f + minScalingMargin * 0.01f);
        float countryWidthWithMargin = countryWidth * (1f + minScalingMargin * 0.01f);

        float aspectRatio = mainCamera.aspect;
        float orthographicSizeHeight = countryHeightWithMargin / 2f;
        float orthographicSizeWidth = countryWidthWithMargin / (2f * aspectRatio);

        var cameraSize = Mathf.Max(orthographicSizeHeight, orthographicSizeWidth);
        cameraSizeForCountry = cameraSize >= minZoom ? Mathf.Max(orthographicSizeHeight, orthographicSizeWidth) : minZoom;
    }

    public void ZoomCameraOnCountry(int countryid)
    {
        ZoomCameraOnCountry(map.Countries[countryid]);
    }

    public void ZoomCameraOnCountry(Country country)
    {
        if (country.Provinces.Any())
        {
            CalculateCountryCenterAndBounds(country);
            CalculateCameraSizeForCountry(country);
            mainCamera.transform.position = new Vector3(countryCenterX, countryCenterY, transform.position.z);
            mainCamera.orthographicSize = cameraSizeForCountry;
        }
        else
        {
            CenterAndScaleCamera();
        }
    }

    public void ZoomCameraOnProvince(Province province)
    {
        Vector3Int provincePosition = new(province.X, province.Y, 0);
        Vector3 worldPosition = tile_map_layer_1.CellToWorld(provincePosition);
        mainCamera.transform.position = new Vector3(worldPosition.x, worldPosition.y, mainCamera.transform.position.z);
        mainCamera.orthographicSize = minZoom;
    }

    public void ZoomOnProvinceTest()
    {
        Province province = map.CurrentPlayer.Provinces.FirstOrDefault();
        ZoomCameraOnProvince(province);
    }
}