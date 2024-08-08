using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class province_click_handler : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap tile_map_layer_1;
    [SerializeField] private AudioSource province_click;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private GameObject settings_menu;

    private Vector3Int previousCellPosition;
    private bool isHovering;
    private Color originalColor;
    private List<Color> originalCellsColor = new List<Color>();
    private const float lightenFactor = 0.3f;
    private ArmyView selectedArmy;
    private List<Vector3Int> highlightedCells = new List<Vector3Int>();

    void Start()
    {
        if (tile_map_layer_1 == null)
        {
            Debug.LogError("Tilemap not found!");
        }

        previousCellPosition = new Vector3Int(-1, -1, -1);
        isHovering = false;
    }

    void Update()
    {
        if (settings_menu != null && settings_menu.activeSelf) return;
        if (IsCursorOverUIObject()) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPosition = tile_map_layer_1.WorldToCell(mouseWorldPos);

        if (cellPosition != previousCellPosition)
        {
            if (isHovering)
            {
                tile_map_layer_1.SetColor(previousCellPosition, originalColor);
            }

            TileBase hoveredTile = tile_map_layer_1.GetTile(cellPosition);

            if (hoveredTile != null && !highlightedCells.Contains(cellPosition))
            {
                originalColor = tile_map_layer_1.GetColor(cellPosition);
                Color lightenedColor = Color.Lerp(originalColor, Color.white, lightenFactor);
                tile_map_layer_1.SetColor(cellPosition, lightenedColor);

                isHovering = true;
                previousCellPosition = cellPosition;
            }
            else
            {
                isHovering = false;
                previousCellPosition = new Vector3Int(-1, -1, -1);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick(cellPosition);
        }
    }

    private bool IsCursorOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    private void HandleLeftClick(Vector3Int cellPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            ArmyView armyView = hit.collider.GetComponent<ArmyView>();
            if (armyView != null)
            {
                if (selectedArmy != null)
                {
                    selectedArmy.GetComponent<SpriteRenderer>().color = Color.white;
                }
                selectedArmy = armyView;
                selectedArmy.GetComponent<SpriteRenderer>().color = Color.red;
                HighlightPossibleMoveCells(selectedArmy.ArmyData);
                Debug.Log($"Selected Army: ({armyView.ArmyData.position.Item1}, {armyView.ArmyData.position.Item2}), Count: {armyView.ArmyData.count}");
                return;
            }
        }

        if (selectedArmy != null)
        {
            if (highlightedCells.Contains(cellPosition))
            {
                (int x, int y) = (cellPosition.x, cellPosition.y);
                map.updateArmyDestination(selectedArmy.ArmyData, (x, y));
                Debug.Log($"Army destination set to ({x},{y})");
            }

            selectedArmy.GetComponent<SpriteRenderer>().color = Color.white;

            foreach (var highlightCell in highlightedCells)
            {
                if (originalCellsColor.Count > 0)
                {
                    int index = highlightedCells.IndexOf(highlightCell);
                    tile_map_layer_1.SetColor(highlightCell, originalCellsColor[index]);
                }
                else
                {
                    tile_map_layer_1.SetColor(highlightCell, originalColor);
                }
            }
            highlightedCells.Clear();
            originalCellsColor.Clear();
            selectedArmy = null;
        }

        TileBase clickedTile = tile_map_layer_1.GetTile(cellPosition);
        if (clickedTile != null)
        {
            province_click.Play();
            DisplayProvinceInfo(cellPosition.x, cellPosition.y);
            Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
        }
    }

    public void DisplayProvinceInfo(int x, int y)
    {
        Province province = map.getProvince(x, y);

        if (province != null)
        {
            map.Selected_province = (province.X, province.Y);
            province_interface.SetActive(true);
        }
    }

    private void HighlightPossibleMoveCells(Army army)
    {
        List<(int, int)> possibleCells = map.getPossibleMoveCells(army);

        highlightedCells.Clear();
        originalCellsColor.Clear();

        foreach (var cell in possibleCells)
        {
            Vector3Int cellPosition = new Vector3Int(cell.Item1, cell.Item2, 0);
            TileBase tile = tile_map_layer_1.GetTile(cellPosition);
            // bez sprawdzania czy nie ma armii, hex na ktorym znajduje sie armia zostaje podswietlony.
            bool hasArmy = false;
            Collider2D[] colliders = Physics2D.OverlapPointAll(tile_map_layer_1.CellToWorld(cellPosition));
            foreach (var collider in colliders)
            {
                if (collider.GetComponent<ArmyView>() != null)
                {
                    hasArmy = true;
                    break;
                }
            }

            if (tile != null && !hasArmy)
            {
                highlightedCells.Add(cellPosition);
                originalColor = tile_map_layer_1.GetColor(cellPosition);
                originalCellsColor.Add(originalColor);
                Color lightenedColor = Color.Lerp(originalColor, Color.white, lightenFactor);
                tile_map_layer_1.SetColor(cellPosition, lightenedColor);
            }
        }
        //https://www.redblobgames.com/grids/hexagons/#line-drawing do przysz³ego pathfindingu? 
        //https://www.redblobgames.com/grids/hexagons/#pathfinding
    }
}
