using Assets.classes;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class army_view : MonoBehaviour
{
    [SerializeField] private Button disbandButton;
    [SerializeField] private Map map;
    [SerializeField] private Material armyMaterial;
    [SerializeField] private Material shipMaterial;
    [SerializeField] private Sprite landSprite;
    [SerializeField] private Sprite oceanSprite;

    private PolygonCollider2D army_collider;
    private SpriteRenderer spriteRenderer;
    private AudioSource move_army_sound;
    private TMP_Text army_count_text;
    private Vector3 targetPosition;
    private Country country;

    private const float moveSpeed = 2f;
    private bool isMoving = false;

    public Army ArmyData { get; set; }
    public Button DisbandButton { get => disbandButton; set => disbandButton = value; }
    public TMP_Text Army_count_text { get => army_count_text; set => army_count_text = value; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        army_collider = GetComponent<PolygonCollider2D>();
        move_army_sound = GetComponent<AudioSource>();
        army_count_text = transform.Find("army_counter_background/army_count_text").GetComponent<TMP_Text>();

        disbandButton = transform.Find("disbandButton").GetComponent<Button>();
        disbandButton.gameObject.SetActive(false);

        disbandButton.onClick.AddListener(DisbandArmy);
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

        var relationType = map.GetHardRelationType(map.CurrentPlayer, map.Countries[ArmyData.OwnerId]);
        UpdateArmyCounterColor(relationType);

        bool isLand = map.GetProvince(ArmyData.Position).IsLand || 
            map.GetProvince(ArmyData.Destination).IsLand;

        if (isLand)
        {
            spriteRenderer.sprite = landSprite;
            spriteRenderer.material = armyMaterial;

        }
        else if(!isLand)
        {
            spriteRenderer.sprite = oceanSprite;
            spriteRenderer.material = shipMaterial;
        }
    }

    public void Initialize(Army armyData, Relation.RelationType? relationType)
    {
        ArmyData = armyData;
        UpdateArmyCounter(ArmyData.Count);
        UpdateArmyCounterColor(relationType);
        ArmyData.OnArmyCountChanged += UpdateArmyCounter;
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
        transform.position = AdjustPosition(position);
    }
    public void UpdateArmyViewSortingOrder(army_view army_View)
    {
        if (army_View == null)
        {
            Debug.LogError("army_View is null in UpdateArmyViewSortingOrder");
            return;
        }
        if (army_View.ArmyData == null)
        {
            Debug.LogError("ArmyData is null in army_View");
            return;
        }
        if (army_View.map == null)
        {
            Debug.LogError("map is null in army_View");
            return;
        }

        if (army_View.ArmyData.OwnerId == map.CurrentPlayerId)
        {
            army_View.SetOrdingLayer(1);
        }
        else
        {
            army_View.SetOrdingLayer(0);
        }
    }

    public void SetOrdingLayer(int order)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("spriteRenderer is null in SetOrdingLayer");
            return;
        }
        spriteRenderer.sortingOrder = order;
    }


    public void MoveTo((int, int) newPosition)
    {
        ArmyData.Position = newPosition;
        UpdatePosition();
        army_collider.enabled = true;
    }

    public void ReturnTo((int, int) newPosition)
    {
        Vector3 position = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        transform.position = AdjustPosition(position);
        army_collider.enabled = true;
    }

    public void PrepareToMoveTo((int, int) newPosition)
    {
        Vector3 position = HexToWorldPosition(newPosition.Item1, newPosition.Item2);
        targetPosition = AdjustPosition(position, isNotPreparingToMoveMethod: false);
        if (map.Controllers[ArmyData.OwnerId] == Map.CountryController.Local)
        {
            isMoving = true;
            move_army_sound.Play();
        }
        army_collider.enabled = false;
    }

    private Vector3 AdjustPosition(Vector3 basePosition, bool isNotPreparingToMoveMethod = true)
    {
        Vector2 provinceCoordinates = WorldToHexPosition(basePosition);
        Province province = map.GetProvince((int)provinceCoordinates.x, (int)provinceCoordinates.y);

        AdjustArmyScale(isNotPreparingToMoveMethod, province);

        // If the army's owner is the owner of the province and it is not the PrepareToMove method,
        // then the position is not adjusted and the army is placed at the center.
        if (isNotPreparingToMoveMethod && province != null && ArmyData.OwnerId == province.OwnerId)
        {
            return basePosition;
        }

        return GetAdjustedPosition(basePosition, provinceCoordinates);
    }

    private Vector3 GetAdjustedPosition(Vector3 basePosition, Vector2 provinceCoordinates) 
    {
        Vector3 baseDirection = (basePosition - transform.position).normalized;
        Vector3 adjustedPosition;
        float offsetDistance = 0.35f; // distance from the center of the province
        float initialOffset = 0f; // starting angle, to which angleReduction(+30, +15, +7.5...) is added
        float angleReduction = 30f;
        bool collides;

        List<army_view> provinceArmyViews = GetProvinceOtherArmyViews(provinceCoordinates);

        while (angleReduction > 0.1f)
        {
            Vector3 direction = Quaternion.Euler(0, 0, initialOffset) * baseDirection;

            for (int i = 0; i < 6; i++)
            {
                adjustedPosition = basePosition - direction * offsetDistance;
                collides = provinceArmyViews.Any(army => Vector3.Distance(army.transform.position, adjustedPosition) < 0.1f);

                if (!collides)
                {
                    return adjustedPosition;
                }

                direction = Quaternion.Euler(0, 0, -60) * direction;
            }

            initialOffset += angleReduction;
            angleReduction /= 2f;
        }

        return basePosition;
    }

    private void AdjustArmyScale(bool isNotPreparingToMoveMethod, Province province)
    {
        // If the army's owner is the owner of the province or it is not the PrepareToMove method, 
        // the army should be larger in size.
        transform.localScale = (isNotPreparingToMoveMethod || (province != null && ArmyData.OwnerId == province.OwnerId))
            ? new Vector3(0.45f, 0.45f, 1f)
            : new Vector3(0.4f, 0.4f, 1f);
    }

    private List<army_view> GetProvinceOtherArmyViews(Vector2 provinceCoordinates)
    {
        // Return the armyViews related to the province that do not belong to the army's owner 
        // and are active in the hierarchy.
        return map.ArmyViews
            .Where(a =>
                a != null && a.ArmyData != null &&
                (a.ArmyData.Position == ((int)provinceCoordinates.x, (int)provinceCoordinates.y)
                || a.ArmyData.Destination == ((int)provinceCoordinates.x, (int)provinceCoordinates.y)) &&
                a.gameObject.activeInHierarchy &&
                !a.ArmyData.Equals(ArmyData)
            ).ToList();
    }

    private Vector3 HexToWorldPosition(int x, int y)
    {
        float X;
        float Y;

        if (y % 2 == 0)
        {
            X = x * 1f;
            Y = y * 0.866f;
        }
        else
        {
            X = x * 1f + 0.5f;
            Y = y * 0.866f;
        }

        return new Vector3(X, Y, 0);
    }

    private Vector2 WorldToHexPosition(Vector3 worldPosition)
    {
        float x = worldPosition.x;
        float y = worldPosition.y;

        int hexX, hexY;

        if (Mathf.FloorToInt(y / 0.866f) % 2 == 0)
        {
            hexX = Mathf.FloorToInt(x);
            hexY = Mathf.FloorToInt(y / 0.866f);
        }
        else
        {
            hexX = Mathf.FloorToInt(x - 0.5f);
            hexY = Mathf.FloorToInt(y / 0.866f);
        }

        return new Vector2(hexX, hexY);
    }

    public void UpdateArmyCounter(int newCount)
    {
        army_count_text.text = newCount.ToString();
    }

    private void UpdateArmyCounterColor(Relation.RelationType? relationType)
    {
        Color relationColor = filter_modes.GetDiplomaticColor(relationType);
        army_count_text.color = relationColor;
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