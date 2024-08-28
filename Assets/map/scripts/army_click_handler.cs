using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class army_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap army_movement_layer;
    [SerializeField] private TileBase base_tile;

    [SerializeField] private AudioSource army_click;
    [SerializeField] private AudioSource army_move_select;

    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private province_tooltip province_tooltip;
    private army_view selectedArmy;
    private List<Vector3Int> highlightedCells = new();

    private float duration = 0.75f;
    private Color startColor;
    private Color endColor;
    private float timeElapsed = 0.0f;
    private bool increasing = true;
    private bool isHighlighted = false;


    private void Start()
    {
        startColor = army_movement_layer.color;
        startColor.a = 20.0f / 255.0f;
        endColor = startColor;
        endColor.a = 150.0f / 255.0f;
    }
    void Update()
    {
        if (isHighlighted)
        {
            AnimateHighlitedTiles();
        }

        if (IsCursorOverUIObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }
    }

    private void HandleLeftClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            province_tooltip.OnMouseExitProvince();
            if (hit.collider.TryGetComponent<army_view>(out var armyView))
            {
                ResetSelectedArmy(); // Resetuj wybran¹ armiê przed przypisaniem nowej
                selectedArmy = armyView;
                selectedArmy.GetComponent<SpriteRenderer>().color = Color.red;
                HighlightPossibleMoveCells(selectedArmy.ArmyData);

                army_click.Play();
                Debug.Log($"Selected Army: ({armyView.ArmyData.position.Item1}, {armyView.ArmyData.position.Item2}), " +
                          $"Count: {armyView.ArmyData.count} origin:{armyView.ArmyData.position} destination:{armyView.ArmyData.destination}");
            }
        }
        else
        {
            if (selectedArmy != null)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;
                Vector3Int cellPosition = base_layer.WorldToCell(mouseWorldPos);

                if (highlightedCells.Contains(cellPosition))
                {
                    (int x, int y) = (cellPosition.x, cellPosition.y);
                    dialog_box.invokeArmyBox(map, selectedArmy.ArmyData, (x, y));
                    province_tooltip.OnMouseExitProvince();
                }

                ResetSelectedArmy(); // Zresetuj po zakoñczeniu ruchu
            }
        }
    }

    private void ResetSelectedArmy()
    {
        if (selectedArmy != null)
        {
            selectedArmy.GetComponent<SpriteRenderer>().color = Color.white;
        }

        foreach (var highlightCell in highlightedCells)
        {
            army_movement_layer.SetTile(highlightCell, null);
        }
        highlightedCells.Clear();
        selectedArmy = null;
        isHighlighted = false;
    }

    private void HighlightPossibleMoveCells(Army army)
    {
        isHighlighted = true;
        List<(int, int)> possibleCells = map.getPossibleMoveCells(army);

        highlightedCells.Clear();

        Vector3Int currentArmyPosition = new Vector3Int(army.position.Item1, army.position.Item2, 0);

        foreach (var cell in possibleCells)
        {
            Vector3Int cellPosition = new Vector3Int(cell.Item1, cell.Item2, 0);
            TileBase tile = base_layer.GetTile(cellPosition);

            // Sprawdzenie czy na danym tile'u nie znajduje siê bie¿¹ca armia
            if (cellPosition == currentArmyPosition)
            {
                continue;
            }

            bool hasArmy = false;
            Collider2D[] colliders = Physics2D.OverlapPointAll(base_layer.CellToWorld(cellPosition));
            foreach (var collider in colliders)
            {
                if (collider.GetComponent<army_view>() != null)
                {
                    hasArmy = true;
                    break;
                }
            }

            if (tile != null)
            {
                highlightedCells.Add(cellPosition);
                army_movement_layer.SetTile(cellPosition, base_tile);
            }
        }
    }

    //https://www.redblobgames.com/grids/hexagons/#line-drawing do przyszlego pathfindingu? 
    //https://www.redblobgames.com/grids/hexagons/#pathfinding

    private void AnimateHighlitedTiles()
    {
        timeElapsed += Time.deltaTime;
        float t = timeElapsed / duration;

        if (increasing)
        {
            army_movement_layer.color = Color.Lerp(startColor, endColor, t);
        }
        else
        {
            army_movement_layer.color = Color.Lerp(endColor, startColor, t);
        }

        if (timeElapsed >= duration)
        {
            timeElapsed = 0.0f;
            increasing = !increasing;
        }
    }

    public bool IsCursorOverHighlightedCell()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector3Int cellPosition = base_layer.WorldToCell(mouseWorldPos);

        return highlightedCells.Contains(cellPosition);
    }
}
