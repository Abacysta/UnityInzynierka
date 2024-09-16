using System.Collections.Generic;
using UnityEngine;

public class army_visibility_manager : MonoBehaviour
{
    [SerializeField] private Map map;

    public void Initialize(Map map)
    {
        this.map = map;
    }

    public void UpdateArmyVisibility(HashSet<(int, int)> revealedTiles)
    {
        foreach (var army in map.Armies)
        {
            var armyView = map.getView(army); // Pobierz referencj� do widoku armii
            if (armyView != null)
            {
                // Sprawd�, czy kafel, na kt�rym znajduje si� armia, jest widoczny
                if (revealedTiles.Contains((army.Position.Item1, army.Position.Item2)))
                {
                    // Je�li kafel jest widoczny, poka� widok armii
                    armyView.gameObject.SetActive(true);
                }
                else
                {
                    // Je�li kafel nie jest widoczny, ukryj widok armii
                    armyView.gameObject.SetActive(false);
                }
            }
        }
    }
}
