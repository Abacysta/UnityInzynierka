using System.Threading.Tasks;
using UnityEngine;

public class army_view : MonoBehaviour
{
    public Army ArmyData { get; private set; }
    private PolygonCollider2D army_collider;
    private Vector3 targetPosition;

    private readonly float moveSpeed = 2f;
    private bool isMoving = false;

    void Awake()
    {
        army_collider = GetComponent<PolygonCollider2D>();
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

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
        army_collider.enabled = true;
    }

    public void PrepareToMoveTo((int, int) newPosition)
    {
        Vector3 worldNewPosition = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        targetPosition = Vector3.Lerp(transform.position, worldNewPosition, 0.45f);
        isMoving = true;
        army_collider.enabled = false;
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
