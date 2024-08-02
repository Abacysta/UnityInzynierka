using UnityEngine;

[CreateAssetMenu(fileName = "New Mine", menuName = "Buildings/Mine")]

public class Mine : Building 
{
    public override void ApplyEffect()
    {
        // na razie puste
    }
    public override void Upgrade()
    {
        if(buildingLevel < 3){
            buildingLevel += 1;
            Debug.Log($"{buildingName} upgraded to level {buildingLevel}");
        }
        else {
            Debug.Log($"Building is at maximum leevel");
        }
    }
    public override void ResetLevel()
    {
        buildingLevel = 0;
    }
}