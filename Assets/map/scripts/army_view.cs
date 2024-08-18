using UnityEngine;

public class army_view : MonoBehaviour
{
    public Army ArmyData { get; private set; }
    private SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;

    private readonly float moveSpeed = 2f;
    private bool isMoving = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
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
        targetPosition = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        isMoving = true;
    }
    public void ScaleArmyView(army_view army_view)
    {
        Vector3 originalSize = army_view.transform.localScale;
        Vector3 targetScale = originalSize * 0.5f;
        army_view.transform.localScale = targetScale;
    }

    public Vector3 HexToWorldPosition(int x, int y)
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
