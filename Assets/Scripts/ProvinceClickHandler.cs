using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProvinceClickHandler : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text id, res, type;

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
                Display_province_info(cellPosition.x, cellPosition.y);
                Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
            }
        }
    }

    public void Display_province_info(int x, int y)
    {
        Province province = map.Provinces.Find(p => p.X == x && p.Y == y);

        if (province != null)
        {
            id.SetText($"Wspó³rzêdne prowincji: {province.X}, {province.Y}");
            res.SetText($"Zasób: {province.Resources} ({province.Resources_amount})");
            type.SetText($"Typ prowincji: {province.Type}");
        }
    }
}
