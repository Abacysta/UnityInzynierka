using UnityEngine;

public class ArmyView : MonoBehaviour
{
    public Army ArmyData { get; private set; }
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Army armyData)
    {
        ArmyData = armyData;
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        Vector3 position = HexToWorldPosition(ArmyData.position.Item1, ArmyData.position.Item2);
        transform.position = position;
    }
    public void MoveTo((int, int) newPosition)
    {
        ArmyData.position = newPosition;
        UpdatePosition();
    }

    private Vector3 HexToWorldPosition(int x, int y)
    {
        float X;
        float Y;

        if (y % 2 == 0)
        {
            X = x * 1f;
            Y = y * 0.862f;
        }
        else
        {
            X = x * 1f + 0.5f;
            Y = y * 0.862f;
        }

        return new Vector3(X, Y, 0);
    }
}
