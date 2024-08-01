using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class province_click_handler : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TMP_Text id, res, type;
    public AudioSource province_click;
    public GameObject province_interface;
    public camera_pan panner;
    public GameObject blocker;


    private Tilemap tilemap;
    private Vector3Int previousCellPosition;
    private bool isHovering;
    private Color originalColor;
    private const float lightenFactor = 0.3f;

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
        if(blocker != null && blocker.activeSelf) return;
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

        if (Input.GetMouseButtonDown(0)/* && !panner.isDrag()*/)
        {
            TileBase clickedTile = tilemap.GetTile(cellPosition);
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
