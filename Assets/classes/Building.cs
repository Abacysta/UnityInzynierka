using UnityEngine;

[System.Serializable]
public class Building
{
    [SerializeField] private BuildingType buildingType;
    [SerializeField] private int buildingLevel;

    public Building(BuildingType type, int level = 0)
    {
        buildingType = type;
        buildingLevel = level;
    }
    public BuildingType BuildingType { get => buildingType; }
    public int BuildingLevel { get => buildingLevel; }

    public void Upgrade()
    {
        if(buildingLevel < 3){
            buildingLevel++;
        }
    }
    public void Downgrade()
    {
        if(buildingLevel>0 && buildingLevel < 4)buildingLevel--;
    }
    public void Reset()
    {
        buildingLevel = 0;
    }
}