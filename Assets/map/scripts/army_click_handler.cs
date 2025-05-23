using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections.Generic;
using static Assets.classes.Relation;
using System.Linq;
using Assets.classes;

public class army_click_handler : cursor_helper
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap base_layer;
    [SerializeField] private Tilemap army_movement_layer;
    [SerializeField] private TileBase base_tile;

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
                if (armyView.ArmyData.OwnerId != map.CurrentPlayerId) return;

                ResetSelectedArmy(); // Reset the selected army before assigning a new one
                selectedArmy = armyView;
                selectedArmy.GetComponent<SpriteRenderer>().color = Color.red;
                HighlightPossibleMoveCells(selectedArmy.ArmyData);
                province_click_handler.DeselectProvince();
                Debug.Log($"Selected Army: ({armyView.ArmyData.Position.Item1}, {armyView.ArmyData.Position.Item2}), " +
                          $"Count: {armyView.ArmyData.Count} origin:{armyView.ArmyData.Position} destination:{armyView.ArmyData.Destination}");
               armyView.DisbandButton.gameObject.SetActive(true);
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
                    dialog_box.InvokeArmyBox(selectedArmy.ArmyData, (x, y));
                    province_tooltip.OnMouseExitProvince();
                }

                ResetSelectedArmy();
            }
        }
    }

    private void ResetSelectedArmy()
    {
        if (selectedArmy != null)
        {
            selectedArmy.GetComponent<SpriteRenderer>().color = Color.white;
            selectedArmy.DisbandButton.gameObject.SetActive(false);
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
        List<(int, int)> possibleCells = map.GetPossibleMoveCells(army);

        highlightedCells.Clear();

        Vector3Int currentArmyPosition = new Vector3Int(army.Position.Item1, army.Position.Item2, 0);

        foreach (var cell in possibleCells)
        {
            Vector3Int cellPosition = new Vector3Int(cell.Item1, cell.Item2, 0);
            TileBase tile = base_layer.GetTile(cellPosition);

            // Check if the current army is not on the given tile
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
        Province tileProvince = map.GetProvince(cellPosition.x, cellPosition.y);
        Country tileOwner = map.Countries[tileProvince.OwnerId];
        Country armyOwner = map.Countries[armyOwnerId];

        // Do not highlight the tile if:
        // the province is a water tile and
        // the armyOwner cannot boat
        if (!tileProvince.IsLand && !armyOwner.TechStats.CanBoat)
        {
            return false;
        }

        // Highlight the tile if the tile's owner is:
        // - tribal or
        // - armyOwner or
        // - at war with armyOwner or
        // - in a alliance relation with armyOwner or
        // - in a vassalage relation with armyOwner or
        // - granting military access to armyOwner or
        return tileOwner.Id == 0 || tileOwner.Id == armyOwnerId ||
            map.AreCountriesOpposingInTheSameWar(armyOwner, tileOwner) ||
            map.HasRelationOfType(armyOwner, tileOwner, RelationType.Alliance) ||
            map.HasRelationOfType(armyOwner, tileOwner, RelationType.Vassalage) ||
            map.HasOrderedRelationOfType(tileOwner, armyOwner, RelationType.MilitaryAccess);
    }

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
            dialog_box.InvokeDisbandArmyBox(army);
            ResetSelectedArmy();
            province_click_handler.DeselectProvince();
        }
    }
}