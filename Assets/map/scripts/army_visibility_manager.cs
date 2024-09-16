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
            var armyView = map.getView(army); // Pobierz referencjê do widoku armii
            if (armyView != null)
            {
                // SprawdŸ, czy kafel, na którym znajduje siê armia, jest widoczny
                if (revealedTiles.Contains((army.Position.Item1, army.Position.Item2)))
                {
                    // Jeœli kafel jest widoczny, poka¿ widok armii
                    armyView.gameObject.SetActive(true);
                }
                else
                {
                    // Jeœli kafel nie jest widoczny, ukryj widok armii
                    armyView.gameObject.SetActive(false);
                }
            }
        }
    }
}
