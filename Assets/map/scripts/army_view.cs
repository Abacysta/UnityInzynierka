using UnityEngine;

public class army_view : MonoBehaviour
{
    public Army ArmyData { get; private set; }
    private PolygonCollider2D army_collider;
    private SpriteRenderer spriteRenderer;
    private AudioSource move_army_sound;
    private Vector3 targetPosition;

    private readonly float moveSpeed = 2f;
    private bool isMoving = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        army_collider = GetComponent<PolygonCollider2D>();
        move_army_sound = GetComponent<AudioSource>(); 
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
        UpdatePosition();
        ArmyData.position = newPosition;
        targetPosition = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        isMoving = true;
        move_army_sound.Play();
        army_collider.enabled = true;
        spriteRenderer.color = Color.white;
    }
    public void ReturnTo((int, int) newPosition)
    {
        targetPosition = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        army_collider.enabled = true;
        spriteRenderer.color = Color.white;
    }

    public void PrepareToMoveTo((int, int) newPosition)
    {
        targetPosition = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        isMoving = true;
        move_army_sound.Play();
        army_collider.enabled = false;
        spriteRenderer.color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
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