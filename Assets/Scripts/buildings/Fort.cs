using UnityEngine;

[CreateAssetMenu(fileName = "New Fort", menuName = "Buildings/Fort")]
public class Fort : Building
{
    public int combatBuff;

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
}
