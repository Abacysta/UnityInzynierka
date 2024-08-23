    using UnityEngine;
[System.Serializable]
public class OccupationInfo
{
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private int occupationCount = 0;
    [SerializeField] private int occupyingCountryId = -1;
    public OccupationInfo() { }

    public OccupationInfo(bool isOccupied, int occupationCount, int occupyingCountryId)
    {
        this.isOccupied = isOccupied;
        this.occupationCount = occupationCount;
        this.occupyingCountryId = occupyingCountryId;
    }
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
    public int OccupationCount { get => occupationCount; set => occupationCount = value; }
    public int OccupyingCountryId { get => occupyingCountryId; set => occupyingCountryId = value; }
}
