using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class map_preview : MonoBehaviour
{
    [SerializeField] private RectTransform parentRectTransform;
    [SerializeField] private Tilemap tilemap; 
    [SerializeField] private TileBase baseTile; 
    [SerializeField] private List<Province> provinces; 

    public List<Province> Provinces { get => provinces; set => provinces = value; }

    private void Start()
    {
        SetPreview();
    }

    private void Update()
    {
        FitTilemapToParent();
    }

    public void Reload()
    {
        SetPreview();
    }

    private void SetPreview()
    {
        tilemap.ClearAllTiles();

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var p in provinces)
        {
            if (p.X < minX) minX = p.X;
            if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.Y > maxY) maxY = p.Y;
        }

        int centerX = (minX + maxX) / 2;
        int centerY = (minY + maxY) / 2;
        int offsetX = -centerX;
        int offsetY = -centerY;

        foreach (var p in provinces)
        {
            Vector3Int position = new Vector3Int(p.X + offsetX, p.Y + offsetY, 0);
            tilemap.SetTile(position, baseTile);
            tilemap.SetColor(position, p.IsLand ? Color.green : Color.blue);
        }
        FitTilemapToParent();
    }

    private void FitTilemapToParent()
    {
        if (parentRectTransform == null || tilemap == null)
        {
            Debug.LogError("Brak przypisania do parentRectTransform lub tilemap.");
            return;
        }
        Vector2 parentSize = parentRectTransform.rect.size;

        Vector3Int tilemapSize = tilemap.cellBounds.size;

        Vector2 tilemapWorldSize = new Vector2(
            tilemapSize.x * tilemap.cellSize.x,
            tilemapSize.y * tilemap.cellSize.y);

        float scaleX = parentSize.x / tilemapWorldSize.x;
        float scaleY = parentSize.y / tilemapWorldSize.y;
        float scale = Mathf.Min(scaleX, scaleY);

        tilemap.transform.localScale = new Vector3(scale, scale, 1);
        tilemap.transform.position = parentRectTransform.position;
    }
}
