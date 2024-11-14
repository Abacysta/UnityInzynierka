using UnityEngine;
using UnityEngine.Tilemaps;

public class province_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap filter_layer;
    [SerializeField] private Tilemap mouse_hover_layer;
    [SerializeField] private Tilemap province_select_layer;
    [SerializeField] private TileBase base_tile;

    [SerializeField] private AudioSource province_click;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private province_tooltip province_tooltip;
    [SerializeField] private army_click_handler armyClickHandler;
    [SerializeField] private camera_controller cameraController;
    [SerializeField] private country_interface_manager country_interface_manager;
    [SerializeField] private map_loader map_loader;

    private Vector3Int previousCellPosition = new(-1, -1, -1);
    private Vector3Int cellPosition;
    private bool isHovering = false;
    private Vector3Int selectedProvincePosition = new(-1, -1, -1);
    private bool isSelected = false;
    private bool isHighlighted = false;
    private float duration = 0.5f;
    private Color startColor;
    private Color endColor;
    private float timeElapsed = 0.0f;
    private bool increasing = true;

    void Update()
    {
        if (isHighlighted)
        {
            AnimateSelectedProvince();
        }

        if (Input.GetMouseButtonDown(0) && IsCursorOverUIObject())
        {
            DeselectProvince();
            return;
        }

        if (cameraController.IsPanning || IsCursorOverUIObject()) 
        {
            mouse_hover_layer.ClearAllTiles();
            province_tooltip.OnMouseExitProvince();
            previousCellPosition = new(-1, -1, -1);
            return;
        }

        SetMouseCellPosition();
        HandleMouseHover();

        if (Input.GetMouseButtonDown(0))
        {
            province_tooltip.OnMouseExitProvince();
            country_interface_manager.HideCountryInterface();
            if (IsCursorOverArmy() || armyClickHandler.IsCursorOverHighlightedCell()) return;
            HandleLeftClick();
        }
    }

    private void HandleMouseHover()
    {
        if (cellPosition != previousCellPosition)
        {
            if (isHovering)
            {
                mouse_hover_layer.SetTile(previousCellPosition, null);
                province_tooltip.OnMouseExitProvince();
            }

            TileBase hoveredTile = base_layer.GetTile(cellPosition);

            if (hoveredTile != null)
            {
                mouse_hover_layer.SetTile(cellPosition, base_tile);
                mouse_hover_layer.SetColor(cellPosition, GetHighlightColor((cellPosition.x, cellPosition.y)));

                if(IsProvinceRevealed(cellPosition.x,cellPosition.y))
                {
                    var prov = map.getProvince(cellPosition.x, cellPosition.y);
                    province_tooltip.OnMouseEnterProvince(prov);
                }
                isHovering = true;
                previousCellPosition = cellPosition;
            }
            else
            {
                isHovering = false;
                previousCellPosition = new Vector3Int(-1, -1, -1);
                province_tooltip.OnMouseExitProvince();
            }
        }
    }

    private void HandleLeftClick()
    {
        TileBase clickedTile = base_layer.GetTile(cellPosition);

        if (clickedTile != null)
        {
            if(IsProvinceRevealed(cellPosition.x, cellPosition.y))
            {
                //province_click.Play();
                SelectProvince(cellPosition.x, cellPosition.y);
                DisplayProvinceInterface(cellPosition.x, cellPosition.y);
                Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
                Debug.Log("res:" + map.getProvince((cellPosition.x, cellPosition.y)).ResourcesP + "mul:" + map.getProvince((cellPosition.x, cellPosition.y)).Modifiers.ProdMod);
            }
            else
            {
                province_interface.SetActive(false);
                Debug.Log($"tile at position: ({cellPosition.x}, {cellPosition.y} is not revealed!)");
            }
       }
    }

    public void DisplayProvinceInterface(int x, int y)
    {
        Province province = map.getProvince(x, y); 

        if (province != null)
        {
            map.Selected_province = (province.X, province.Y);
            province_interface.SetActive(true);
        }
    }
    private void DisplayProvinceTooltip(int x, int y) {
        Province province = map.getProvince(x, y);

        if(province != null) {
            province_tooltip.OnMouseExitProvince();
        }
    }

    private bool IsProvinceRevealed(int x, int y)
    {
        Province province = map.getProvince(x, y);
        if (province != null)
        {
            if (map.CurrentPlayer.RevealedTiles.Contains((x, y))) return true;
        }
        return false;
    }
    public void SelectProvince(int x, int y)
    {
        DeselectProvince();
        selectedProvincePosition = new Vector3Int(x, y, 0);
        province_select_layer.SetTile(selectedProvincePosition, base_tile);
        SetProvinceSelectLayerColor();
        isSelected = true;
        isHighlighted = true;
        timeElapsed = 0.0f;
    }

    public void DeselectProvince()
    {
        if (isSelected)
        {
            province_select_layer.SetTile(selectedProvincePosition, null);
            selectedProvincePosition = new Vector3Int(-1, -1, -1);
            isSelected = false;
            isHighlighted = false;
        }
    }

    private void AnimateSelectedProvince()
    {
        timeElapsed += Time.deltaTime;
        float t = timeElapsed / duration;

        if (increasing)
        {
            province_select_layer.color = Color.Lerp(startColor, endColor, t);
        }
        else
        {
            province_select_layer.color = Color.Lerp(endColor, startColor, t);
        }

        if (timeElapsed >= duration)
        {
            timeElapsed = 0.0f;
            increasing = !increasing;
        }
    }

    private void SetMouseCellPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        cellPosition = mouse_hover_layer.WorldToCell(mouseWorldPos);
    }

    private void SetProvinceSelectLayerColor()
    {
        startColor = GetHighlightColor((selectedProvincePosition.x, selectedProvincePosition.y));
        startColor.a = 0f / 255.0f;
        endColor = startColor;
        endColor.a = 60.0f / 255.0f;
    }

    public Color GetHighlightColor((int x, int y) coordinates)
    {
        Tilemap tileMap = (map_loader.CurrentMode == map_loader.MapMode.Terrain ||
                           map_loader.CurrentMode == map_loader.MapMode.Political)
                           ? base_layer
                           : filter_layer;

        Province province = map.getProvince(coordinates.x, coordinates.y);

        if (province == null || province.Type != "land") {
            return Color.white;
        }

        Vector3Int tilePosition = new(coordinates.x, coordinates.y, 0);
        Color tileColor = tileMap.GetColor(tilePosition);

        float brightness = tileColor.r * 0.299f + tileColor.g * 0.587f + tileColor.b * 0.114f;

        return brightness > 0.8f ? Color.black : Color.white;
    }
}