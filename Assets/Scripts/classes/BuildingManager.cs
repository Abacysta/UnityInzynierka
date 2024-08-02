using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public Building fortBuilding;
    public Building infrastructureBuilding;
    public Building mineBuilding;
    public Building schoolBuilding;

    private Building selectedBuilding;
    public Map map;
    private void Start()
    {
        if (fortBuilding != null)
            fortBuilding.ResetLevel();

        if (infrastructureBuilding != null)
            infrastructureBuilding.ResetLevel();

        if (mineBuilding != null)
            mineBuilding.ResetLevel();

        if (schoolBuilding != null)
            schoolBuilding.ResetLevel();
    }

    public void SelectBuilding(Building building)
    {
        selectedBuilding = building;
    }

    public void BuildSelectedBuilding(int x, int y)
    {
        if (selectedBuilding != null)
        {
            Province province = map.GetProvince(x, y);
            if (province != null && province.Current_building == null)
            {
                province.AddBuilding(selectedBuilding);
            }
        }
    }

    public void UpgradeBuilding(int x, int y)
    {
        Province province = map.GetProvince(x, y);
        if (province != null && province.Current_building != null)
        {
            province.Current_building.Upgrade();
        }
    }

}
