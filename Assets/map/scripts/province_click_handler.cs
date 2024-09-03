using UnityEngine;
using UnityEngine.Tilemaps;

public class province_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap mouse_hover_layer;
    [SerializeField] private TileBase base_tile;

    [SerializeField] private AudioSource province_click;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private province_tooltip province_tooltip;
    [SerializeField] private army_click_handler armyClickHandler;
    [SerializeField] private camera_controller cameraController;
    [SerializeField] private country_interface_manager country_interface_manager;

    private Vector3Int previousCellPosition;
    private Vector3Int cellPosition;
    private bool isHovering;

    void Start()
    {
        previousCellPosition = new Vector3Int(-1, -1, -1);
        isHovering = false;
    }

    void Update()
    {
        if (cameraController.IsPanning) 
        {
            mouse_hover_layer.ClearAllTiles();
            province_tooltip.OnMouseExitProvince();
            return;
        }
        if (IsCursorOverUIObject()) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        cellPosition = mouse_hover_layer.WorldToCell(mouseWorldPos);

        HandleMouseHover();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsCursorOverArmy() || armyClickHandler.IsCursorOverHighlightedCell()) return;
            province_tooltip.OnMouseExitProvince();
            country_interface_manager.HideCountryInterface();
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
                DisplayProvinceInterface(cellPosition.x, cellPosition.y);
                Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
                Debug.Log("res:" + map.getProvince((cellPosition.x, cellPosition.y)).ResourcesP + "mul:" + map.getProvince((cellPosition.x, cellPosition.y)).Prod_mod);
            }
            else
            {
                province_interface.SetActive(false);
                Debug.Log($"tile at position: ({cellPosition.x}, {cellPosition.y} is not revealed!)");
            }
       }
    }

    private void DisplayProvinceInterface(int x, int y)
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
        //Province province = map.getProvince(x, y);
        //if (province != null)
        //{
        //    foreach(Country country in map.Countries)
        //    {
        //        if (country.RevealedTiles.Contains((x, y))) return true;
        //    }
        //}
        //return false;
        return true;
    }
}
