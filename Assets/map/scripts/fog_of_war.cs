using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class fog_of_war : MonoBehaviour
{
    [SerializeField] private Tilemap fogTilemap;
    [SerializeField] private TileBase tileFog;

    private int playerCountryId = 1;
    [SerializeField] private Map map;
    public void Start() 
    {
        StartTurn();
    }
    public void ApplyFogOfWar()
    {
        fogTilemap.ClearAllTiles();
        UpdateFogOfWar();
    }

    public void UpdateFogOfWar()
    {
        foreach (Country country in map.Countries)
        {
            if (country.Id == playerCountryId) 
            {
                foreach (Province province in map.Provinces)
                {
                    if (country.RevealedTiles.Contains((province.X, province.Y)))
                    {
                        RevealTile((province.X, province.Y));
                    }
                    else
                    {
                        HideTile((province.X, province.Y));
                    }
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
        Debug.Log($"Revealed tile at {coordinates.x}, {coordinates.y}");
    }

    public void HideTile((int x, int y) coordinates)
    {
        Vector3Int position = new Vector3Int(coordinates.x, coordinates.y, 0);
        fogTilemap.SetTile(position, tileFog);
        Debug.Log($"Hidden tile at {coordinates.x}, {coordinates.y}");
    }

    public void StartTurn()
    {
        foreach (Country country in map.Countries)
        {
            CalculateVisibilityForCountry(country);
        }
        CalculateVisibilityForArmies();
        UpdateFogOfWar();
    }

    public void CalculateVisibilityForCountry(Country country)
    {
        country.ClearRevealedTiles();

        foreach ((int x, int y) in country.Provinces)
        {
            country.RevealedTiles.Add((x, y));
            UpdateVisibilityAroundProvince(x, y, country.techStats.lvlFoW);
        }
    }

    private void UpdateVisibilityAroundProvince(int x, int y, int visibilityRange)
    {
        Province province = map.getProvince(x, y);

        if (province == null)
        {
            return;
        }

        Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
        if (country == null)
        {
            Debug.LogError($"Country with ID {province.Owner_id} not found for province at {x},{y}.");
            return;
        }

        HexUtils.Cube centerCube = HexUtils.OffsetToCube(x, y);
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
                }
            }
        }
    }
    private void UpdateVisibilityAroundArmy(Army army)
    {
        Province province = map.getProvince(army.position.Item1, army.position.Item2);

        if (province == null)
        {
            return;
        }

        Country country = map.Countries.FirstOrDefault(c => c.Id == army.OwnerId);
        if (country == null)
        {
            Debug.LogError($"Country with ID {province.Owner_id} not found for province at {army.position.Item1},{army.position.Item2}.");
            return;
        }

        HexUtils.Cube centerCube = HexUtils.OffsetToCube(army.position.Item1, army.position.Item2);
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
                }
            }
        }
    }
    private void CalculateVisibilityForArmies()
    {
        List<Army>armies = map.Armies;
        foreach(Army army in armies)
        {
            Country country = map.Countries.FirstOrDefault(c => c.Id == army.ownerId);
            country.RevealedTiles.Add((army.position.Item1, army.position.Item2));
            UpdateVisibilityAroundArmy(army);
        }
    }
}
