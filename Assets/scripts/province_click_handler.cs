using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class province_click_handler : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private Tilemap tile_map_layer_1;
    [SerializeField] private TMP_Text id, res, type;
    [SerializeField] private AudioSource province_click;
    [SerializeField] private GameObject province_interface;
    [SerializeField] private GameObject blocker;

    private Vector3Int previousCellPosition;
    private bool isHovering;
    private Color originalColor;
    private const float lightenFactor = 0.3f;

    private void Start()
    {
        if (tile_map_layer_1 == null)
        {
            Debug.LogError("Tilemap not found!");
        }

        previousCellPosition = new Vector3Int(-1, -1, -1);
        isHovering = false;
    }

    private void Update()
    {
        if(blocker != null && blocker.activeSelf) return;
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
            if (hoveredTile != null)
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
            TileBase clickedTile = tile_map_layer_1.GetTile(cellPosition);
            if (clickedTile != null)
            {
                province_click.Play();
                DisplayProvinceInfo(cellPosition.x, cellPosition.y);
                Debug.Log($"Clicked on tile at position: ({cellPosition.x}, {cellPosition.y})");
            }
        }
    }

    public void DisplayProvinceInfo(int x, int y)
    {
        Province province = map.Provinces.Find(p => p.X == x && p.Y == y);

        if (province != null)
        {
            map.Selected_province = (province.X, province.Y);
            province_interface.SetActive(true);
        }
    }
}
