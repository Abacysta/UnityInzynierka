using UnityEngine;

[CreateAssetMenu(fileName = "New Infrastructure", menuName = "Buildings/Infrastructure")]
public class Infrastructure : Building
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

}
