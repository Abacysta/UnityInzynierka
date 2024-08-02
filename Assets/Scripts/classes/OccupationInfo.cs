using UnityEngine;
[System.Serializable]
public class OccupationInfo
{
    [SerializeField] private bool isOccupied;
    [SerializeField] private int occupationCount;
    [SerializeField] private int occupyingCountryId;

    public OccupationInfo(bool isOccupied, int occupationCount, int occupyingCountryId)
    {
        this.isOccupied = isOccupied;
        this.occupationCount = occupationCount;
        this.occupyingCountryId = occupyingCountryId;
    }
}
