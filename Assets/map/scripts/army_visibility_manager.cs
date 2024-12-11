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
            var armyView = map.GetView(army);
            if (armyView != null)
            {
                if (revealedTiles.Contains((army.Position.Item1, army.Position.Item2)))
                {
                    armyView.gameObject.SetActive(true);
                }
                else
                {
                    armyView.gameObject.SetActive(false);
                }
            }
        }
    }
}
