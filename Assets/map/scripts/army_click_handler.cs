using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using static Assets.classes.Relation;
using System.Linq;

public class army_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap army_movement_layer;
    [SerializeField] private TileBase base_tile;

    //[SerializeField] private AudioSource army_click;
    [SerializeField] private AudioSource army_move_select;
    [SerializeField] private dialog_box_manager dialog_box;
    [SerializeField] private province_tooltip province_tooltip;
    [SerializeField] private province_click_handler province_click_handler;

    private army_view selectedArmy;
    private List<Vector3Int> highlightedCells = new();

    private float duration = 0.5f;
    private Color startColor;
    private Color endColor;
    private float timeElapsed = 0.0f;
    private bool increasing = true;
    private bool isHighlighted = false;

    private void Start()
    {
        SetStartAndEndColor();
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
                ResetSelectedArmy();
                return;
            }
            HandleLeftClick();
        }
    }

    private void HandleLeftClick()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<army_view>(out var armyView))
            {
                if (armyView.ArmyData.OwnerId != map.currentPlayer) return;

                ResetSelectedArmy(); // Resetuj wybrana armie przed przypisaniem nowej
                selectedArmy = armyView;
                selectedArmy.GetComponent<SpriteRenderer>().color = Color.red;
                HighlightPossibleMoveCells(selectedArmy.ArmyData);
                province_click_handler.DeselectProvince();
                //army_click.Play();
                Debug.Log($"Selected Army: ({armyView.ArmyData.Position.Item1}, {armyView.ArmyData.Position.Item2}), " +
                          $"Count: {armyView.ArmyData.Count} origin:{armyView.ArmyData.Position} destination:{armyView.ArmyData.Destination}");
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

                ResetSelectedArmy(); // Zresetuj po zakonczeniu ruchu
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

            // Sprawdzenie czy na danym tile'u nie znajduje sie biezaca armia
            if (cellPosition == currentArmyPosition)
            {
                continue;
            }

            Collider2D[] colliders = Physics2D.OverlapPointAll(base_layer.CellToWorld(cellPosition));
            foreach (var collider in colliders)
            {
                if (collider.GetComponent<army_view>() != null)
                {
                    break;
                }
            }

            if (!IsTileAccessibleForArmyMovement(cellPosition, army.OwnerId)) continue;

            if (tile != null)
            {
                highlightedCells.Add(cellPosition);
                army_movement_layer.SetTile(cellPosition, base_tile);
                army_movement_layer.SetColor(cellPosition, province_click_handler.GetHighlightColor((cellPosition.x, cellPosition.y)));
            }
        }
    }

    public bool IsTileAccessibleForArmyMovement(Vector3Int cellPosition, int armyOwnerId)
    {
        bool HasCurrentPlayerRelationWithTileOwner(RelationType type, Country tileOwner)
        {
            return map.Relations.Any(rel => rel.type == type &&
                rel.Sides.Contains(map.CurrentPlayer) && rel.Sides.Contains(tileOwner));
        }

        bool HasCurrentPlayerRelationWithTileOwnerAsSide0(RelationType type, Country tileOwner)
        {
            return map.Relations.Any(rel => rel.type == type &&
                rel.Sides[0] == tileOwner && rel.Sides[1] == map.CurrentPlayer);
        }

        Province tileProvince = map.getProvince(cellPosition.x, cellPosition.y);
        Country tileOwner = map.Countries[tileProvince.Owner_id];
        Country armyOwner = map.Countries[armyOwnerId];

        // Do not highlight the tile if:
        // the province is a water tile and
        // the currentPlayer cannot boat
        if (tileProvince.Type == "ocean" && !armyOwner.techStats.canBoat)
        {
            return false;
        }

        // Highlight the tile if the tile's owner is:
        // - currentPlayer or
        // - tribal or
        // - at war with currentPlayer or
        // - in a alliance relation with currentPlayer or
        // - in a vassalage relation with currentPlayer or
        // - granting military access to currentPlayer or
        return tileOwner.Id == 0 || tileOwner.Id == armyOwnerId ||
            HasCurrentPlayerRelationWithTileOwner(RelationType.War, tileOwner) ||
            HasCurrentPlayerRelationWithTileOwner(RelationType.Alliance, tileOwner) ||
            HasCurrentPlayerRelationWithTileOwner(RelationType.Vassalage, tileOwner) ||
            HasCurrentPlayerRelationWithTileOwnerAsSide0(RelationType.MilitaryAccess, tileOwner);
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

    private void SetStartAndEndColor()
    {
        startColor = Color.white;
        startColor.a = 0f;
        endColor = startColor;
        endColor.a = 130.0f / 255.0f;
    }

    public void DisbandArmy(Army army)
    {
        if (army != null)
        {
            dialog_box.invokeDisbandArmyBox(map, army);
            ResetSelectedArmy();
            province_click_handler.DeselectProvince();
        }
    }
}