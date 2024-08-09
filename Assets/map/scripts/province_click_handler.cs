using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class province_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap mouse_hover_layer;
    [SerializeField] private TileBase base_tile;

    [SerializeField] private AudioSource province_click;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private army_click_handler armyClickHandler;

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
        if (IsCursorOverUIObject()) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        cellPosition = mouse_hover_layer.WorldToCell(mouseWorldPos);

        HandleMouseHover();

        if (Input.GetMouseButtonDown(0))
        {
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
            }

            TileBase hoveredTile = base_layer.GetTile(cellPosition);

            if (hoveredTile != null)
            {
                mouse_hover_layer.SetTile(cellPosition, base_tile);

                isHovering = true;
                previousCellPosition = cellPosition;
            }
            else
            {
                isHovering = false;
                previousCellPosition = new Vector3Int(-1, -1, -1);
            }
        }
    }

    private void HandleLeftClick()
    {
        TileBase clickedTile = base_layer.GetTile(cellPosition);

        if (clickedTile != null)
        {
            province_click.Play();
            DisplayProvinceInterface(cellPosition.x, cellPosition.y);
            Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
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
}
