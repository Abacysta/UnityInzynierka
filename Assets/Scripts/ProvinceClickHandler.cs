using UnityEngine;
using UnityEngine.Tilemaps;

public class ProvinceClickHandler : MonoBehaviour
{
    private Tilemap tilemap;
    private Vector3Int previousCellPosition;
    private bool isHovering;
    private Color originalColor;
    private float lightenFactor = 0.3f;

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap not found!");
        }

        previousCellPosition = new Vector3Int(-1, -1, -1);
        isHovering = false;
    }

    private void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);


        if (cellPosition != previousCellPosition)
        {
            if (isHovering)
            {
                tilemap.SetColor(previousCellPosition, originalColor);
            }

            TileBase hoveredTile = tilemap.GetTile(cellPosition);
            if (hoveredTile != null)
            {
                originalColor = tilemap.GetColor(cellPosition);
                Color lightenedColor = Color.Lerp(originalColor, Color.white, lightenFactor);
                tilemap.SetColor(cellPosition, lightenedColor);

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
            TileBase clickedTile = tilemap.GetTile(cellPosition);
            if (clickedTile != null)
            {
                Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
            }
        }
    }


}
