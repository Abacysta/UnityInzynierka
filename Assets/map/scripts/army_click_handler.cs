using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class army_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap army_movement_layer;
    [SerializeField] private TileBase base_tile;

    //[SerializeField] private AudioSource army_click;
    [SerializeField] private AudioSource army_move_select;
    //[SerializeField] private GameObject disbandButtonPrefab;
    //[SerializeField] private Canvas canvas;
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

    //private GameObject disbandButtonInstance;

    void Start()
    {
        InitializeColors();
    }

    void Update()
    {
        if (isHighlighted)
        {
            AnimateHighlitedTiles();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (IsCursorOverUIObject())
            {
                //ResetSelectedArmy();
                return;
            }
            HandleLeftClick();
        }

       // UpdateDisbandButtonPosition();
    }

    private void HandleLeftClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<army_view>(out var armyView))
            {
                if (armyView.ArmyData.OwnerId != map.currentPlayer) return;

                ResetSelectedArmy(); // Resetuj wybran� armi� przed przypisaniem nowej
                selectedArmy = armyView;
                selectedArmy.GetComponent<SpriteRenderer>().color = Color.red;
                HighlightPossibleMoveCells(selectedArmy.ArmyData);
                //army_click.Play();
                Debug.Log($"Selected Army: ({armyView.ArmyData.Position.Item1}, {armyView.ArmyData.Position.Item2}), " +
                          $"Count: {armyView.ArmyData.Count} origin:{armyView.ArmyData.Position} destination:{armyView.ArmyData.Destination}");
               //CreateDisbandButton(armyView);
               armyView.disbandButton.gameObject.SetActive(true);
            }
            else if(hit.collider.TryGetComponent<Button>(out var button)){
                button.onClick.Invoke();
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

                ResetSelectedArmy(); // Zresetuj po zako�czeniu ruchu
            }
        }
    }

    private void ResetSelectedArmy()
    {
        if (selectedArmy != null)
        {
            selectedArmy.GetComponent<SpriteRenderer>().color = Color.white;
            selectedArmy.disbandButton.gameObject.SetActive(false);
        }

        foreach (var highlightCell in highlightedCells)
        {
            army_movement_layer.SetTile(highlightCell, null);
        }
        highlightedCells.Clear();
        selectedArmy = null;
        isHighlighted = false;
        /*
        if (disbandButtonInstance != null)
        {
            Destroy(disbandButtonInstance);
        }
        */
    }

    private void HighlightPossibleMoveCells(Army army)
    {
        isHighlighted = true;
        List<(int, int)> possibleCells = map.getPossibleMoveCells(army);

        highlightedCells.Clear();

        Vector3Int currentArmyPosition = new Vector3Int(army.Position.Item1, army.Position.Item2, 0);

        foreach (var cell in possibleCells)
        {
            Vector3Int cellPosition = new Vector3Int(cell.Item1, cell.Item2, 0);
            TileBase tile = base_layer.GetTile(cellPosition);

            // Sprawdzenie czy na danym tile'u nie znajduje si� bie��ca armia
            //fajne polskie znaki xd
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

    // https://www.redblobgames.com/grids/hexagons/#line-drawing do przyszlego pathfindingu? 
    // https://www.redblobgames.com/grids/hexagons/#pathfinding

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

    private void InitializeColors()
    {
        startColor = army_movement_layer.color;
        startColor.a = 20.0f / 255.0f;
        endColor = startColor;
        endColor.a = 150.0f / 255.0f;
    }
/*
    private void CreateDisbandButton(army_view armyView)
    {
        if (disbandButtonInstance != null)
        {
            Destroy(disbandButtonInstance);
        }

        disbandButtonInstance = Instantiate(disbandButtonPrefab, canvas.transform);

        Vector3 worldPosition = base_layer.CellToWorld(new Vector3Int(armyView.ArmyData.Position.Item1, armyView.ArmyData.Position.Item2, 0));
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        disbandButtonInstance.transform.position = screenPosition + new Vector3(-50, -50, 0);

        Button disbandButton = disbandButtonInstance.GetComponent<Button>();
        disbandButton.onClick.AddListener(() => DisbandArmy(armyView));
    }

    private void UpdateDisbandButtonPosition()
    {
        if (selectedArmy != null && disbandButtonInstance != null)
        {
            Vector3 worldPosition = base_layer.CellToWorld(new Vector3Int(selectedArmy.ArmyData.Position.Item1, selectedArmy.ArmyData.Position.Item2, 0));
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            disbandButtonInstance.transform.position = screenPosition + new Vector3(-50, -50, 0);
        }
    }
    */
    public void DisbandArmy(Army army)
    {
        if (army != null)
        {
            dialog_box.invokeDisbandArmyBox(map, army);
            ResetSelectedArmy();
        }
    }
}
