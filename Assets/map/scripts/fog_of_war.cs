using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class fog_of_war : MonoBehaviour
{
    [SerializeField] private Tilemap fogTilemap;
    [SerializeField] private Tilemap fogMemoryTilemap;
    [SerializeField] private TileBase tileFog;
    [SerializeField] private TileBase tileMemoryFog;
    [SerializeField] private Map map;

    public void Start() 
    {
        StartTurn();
        UpdateFogOfWar();
    }

    public void ApplyFogOfWar()
    {
        fogTilemap.ClearAllTiles();
        UpdateFogOfWar();
    }

    public void UpdateFogOfWar()
    {
        foreach (Province province in map.Provinces)
        {
            if (map.CurrentPlayer.RevealedTiles.Contains((province.X, province.Y)))
            {
                RevealTile((province.X, province.Y));
                HideMemoryTile((province.X, province.Y));  
            }
            else
            {
                if(map.CurrentPlayer.SeenTiles.Contains((province.X, province.Y)))
                {
                    MemoryTile((province.X, province.Y));
                }
                else
                {
                    HideTile((province.X, province.Y));
                }
            }
        }
    }

    public void InitializeFogOfWar()
    {
        fogTilemap.ClearAllTiles();
    }

    public void RevealTile((int x, int y) coordinates)
    {
        Vector3Int position = new Vector3Int(coordinates.x, coordinates.y, 0);
        fogTilemap.SetTile(position, null);
    }

    public void HideTile((int x, int y) coordinates)
    {
        Vector3Int position = new Vector3Int(coordinates.x, coordinates.y, 0);
        fogTilemap.SetTile(position, tileFog);
    }

    public void MemoryTile((int x, int y) coordinates)
    {
        Vector3Int position = new Vector3Int(coordinates.x, coordinates.y, 0);
        fogMemoryTilemap.SetTile(position, tileMemoryFog);
    }

    public void HideMemoryTile((int x, int y) coordinates)
    {
        Vector3Int position = new Vector3Int(coordinates.x, coordinates.y, 0);
        fogMemoryTilemap.SetTile(position, null);
    }

    public void StartTurn()
    {
        foreach (Country country in map.Countries)
        {
            CalculateVisibilityForCountry(country);
        }
        CalculateVisibilityForArmies();
    }

    public void CalculateVisibilityForCountry(Country country)
    {
        if (country == null)
        {
            Debug.LogError("Country is null in CalculateVisibilityForCountry!");
            return;
        }

        country.ClearRevealedTiles();

        if (country.Provinces != null)
        {
            foreach (Province province in country.Provinces)
            {
                if (province != null)
                {
                    country.RevealedTiles.Add(province.coordinates);
                    UpdateVisibilityAroundProvince(province, country.techStats.lvlFoW);
                }
            }
        }
        else
        {
            Debug.LogError($"Provinces are null for country {country.Id}!");
        }
    }

    private void UpdateVisibilityAroundProvince(Province province, int visibilityRange)
    {
        if (province == null)
        {
            return;
        }

        Country country = map.Countries.FirstOrDefault(c => c.Id == province.OwnerId);
        HexUtils.Cube centerCube = HexUtils.OffsetToCube(province.X, province.Y);
        List<HexUtils.Cube> visibleCubes = HexUtils.CubeRange(centerCube, visibilityRange);

        foreach (HexUtils.Cube cube in visibleCubes)
        {
            (int offsetX, int offsetY) = HexUtils.CubeToOffset(cube);

            if (map.IsValidPosition(offsetX, offsetY))
            {
                Province visibleProvince = map.getProvince(offsetX, offsetY);
                if (visibleProvince != null)
                {
                    country.RevealedTiles.Add((offsetX, offsetY));
                    country.SeenTiles.Add((offsetX, offsetY));
                }
            }
        }
    }

    private void UpdateVisibilityAroundArmy(Army army)
    {
        Province province = map.getProvince(army.Position.Item1, army.Position.Item2);

        if (province == null)
        {
            return;
        }

        Country country = map.Countries.FirstOrDefault(c => c.Id == army.OwnerId);

        HexUtils.Cube centerCube = HexUtils.OffsetToCube(army.Position.Item1, army.Position.Item2);
        List<HexUtils.Cube> visibleCubes = HexUtils.CubeRange(centerCube, country.techStats.lvlFoW);

        foreach (HexUtils.Cube cube in visibleCubes)
        {
            (int offsetX, int offsetY) = HexUtils.CubeToOffset(cube);

            if (map.IsValidPosition(offsetX, offsetY))
            {
                Province visibleProvince = map.getProvince(offsetX, offsetY);
                if (visibleProvince != null)
                {
                    country.RevealedTiles.Add((offsetX, offsetY));
                    country.SeenTiles.Add((offsetX, offsetY));
                }
            }
        }
    }

    private void CalculateVisibilityForArmies()
    {
        List<Army>armies = map.Armies;
        foreach(Army army in armies)
        {
            Country country = map.Countries.FirstOrDefault(c => c.Id == army.OwnerId);
            country.RevealedTiles.Add((army.Position.Item1, army.Position.Item2));
            country.SeenTiles.Add((army.Position.Item1, army.Position.Item2));
            UpdateVisibilityAroundArmy(army);
        }
    }
}