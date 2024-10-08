using Assets.classes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class army_view : MonoBehaviour
{
    public Army ArmyData { get; private set; }
    private PolygonCollider2D army_collider;
    private SpriteRenderer spriteRenderer;
    private AudioSource move_army_sound;
    private TMP_Text army_count_text;
    private Vector3 targetPosition;

    private readonly float moveSpeed = 2f;
    private bool isMoving = false;

    public Button disbandButton;
    [SerializeField] Map map;
    private dialog_box_manager dialog_box;
    public Material armyMaterial;
    public Material shipMaterial;
    private Country country;

    public Sprite landSprite;
    public Sprite oceanSprite;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        army_collider = GetComponent<PolygonCollider2D>();
        move_army_sound = GetComponent<AudioSource>();
        army_count_text = transform.Find("army_counter_background/army_count_text").GetComponent<TMP_Text>();

        disbandButton = transform.Find("disbandButton").GetComponent<Button>();
        disbandButton.gameObject.SetActive(false);

        disbandButton.onClick.AddListener(DisbandArmy);

        dialog_box = GetComponent<dialog_box_manager>();
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

        string terrainType = map.getProvince(ArmyData.Position).Type;

        if (terrainType == "land")
        {
            spriteRenderer.sprite = landSprite;
            spriteRenderer.material = armyMaterial;

        }
        else if(terrainType == "ocean")
        {
            spriteRenderer.sprite = oceanSprite;
            spriteRenderer.material = shipMaterial;

        }
    }

    public void Initialize(Army armyData, Relation.RelationType? type)
    {
        ArmyData = armyData;
        UpdateArmyCounter(ArmyData.Count);
        int tval = type != null ? (int)type.Value : 50;
        UpdateArmyCounterColor(tval);
        ArmyData.OnArmyCountChanged += UpdateArmyCounter;
        ArmyData.OnArmyCountChanged += UpdateArmyCounterColor;
        UpdatePosition();

        country = map.Countries[armyData.OwnerId];
        armyMaterial = new Material(armyMaterial);
        shipMaterial = new Material(shipMaterial);

        armyMaterial.SetColor("_ColorChange", country.Color);
        shipMaterial.SetColor("_ColorChange", country.Color);
    }

    public void UpdatePosition()
    {
        Vector3 position = HexToWorldPosition(ArmyData.Position.Item1, ArmyData.Position.Item2);
        transform.position = position;
    }

    public void MoveTo((int, int) newPosition)
    {
        UpdatePosition();
        ArmyData.Position = newPosition;
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
        transform.position = targetPosition;
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

    private void UpdateArmyCounter(int newCount)
    {
        army_count_text.text = newCount.ToString();
    }

    private void UpdateArmyCounterColor(int type)
    {
        Debug.Log("rtype=" + type);
        Color adjusted;
        switch (type)
        {
            case -1://war
                adjusted = new Color(1f, 0.27f, 0);//orange
                break;
            case 0://truce
                adjusted = Color.white;
                break;
            case 3://alliance
                adjusted = Color.cyan;
                break;
            case 4://vassalage
                adjusted = new Color(0.7f, 0, 1);
                break;
            case -2://rebellion
                adjusted = Color.magenta;
                break;
            default:
                adjusted = Color.yellow;
                break;
        }
        army_count_text.color = adjusted;
    }

    public void DisbandArmy()
    {
        army_click_handler click_Handler = FindObjectOfType<army_click_handler>();
        if (click_Handler != null)
        {
            click_Handler.DisbandArmy(ArmyData);
        }
        else
        {
            Debug.Log("click handler is empty");
        }
    }
}