using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.classes.Relation;
using static Province;

public class province_tooltip : MonoBehaviour
{
    [SerializeField] private Image country_coat;
    [SerializeField] private TMP_Text country_province_name;
    [SerializeField] private TMP_Text terrain_name;
    [SerializeField] private Image resource_img;
    [SerializeField] private TMP_Text happiness_value;
    [SerializeField] private TMP_Text population_value;
    [SerializeField] private Image relation_img;

    [SerializeField] private GameObject terrain_row;
    [SerializeField] private GameObject resource_row;
    [SerializeField] private GameObject happiness_row;
    [SerializeField] private GameObject population_row;
    [SerializeField] private GameObject relation_row;

    [SerializeField] private Map map;

    [SerializeField] private Sprite war_sprite;
    [SerializeField] private Sprite alliance_sprite;
    [SerializeField] private Sprite truce_sprite;
    [SerializeField] private Sprite vassalage_sprite_1;
    [SerializeField] private Sprite vassalage_sprite_2;
    [SerializeField] private Sprite rebellion_sprite;

    [SerializeField] private dynamic_scoll_view_province_row province_row_script;
    [SerializeField] private filter_modes filter_modes;

    private Coroutine tooltip;
    private GameObject oTooltip;

    private void Start() {
        oTooltip = gameObject.transform.GetChild(0).gameObject;
    }

    private void Update() {
        Vector3 mousePos = Input.mousePosition; 
        mousePos.y += 90f;
        transform.position = mousePos;
    }

    public void OnMouseEnterProvince(Province province) {
        if(tooltip != null) {
            StopCoroutine(tooltip);
        }
        tooltip = StartCoroutine(ShowTooltip(province));
    }
    public void OnMouseExitProvince() {
        if(tooltip != null) {
            StopCoroutine(tooltip);
            tooltip = null;
        }
        oTooltip.SetActive(false);
    }
    private IEnumerator ShowTooltip(Province province) {
        yield return new WaitForSeconds(0.5f);
        if(province != null) {
            SetTooltipData(province);
            oTooltip.SetActive(true);
        }
    }
    public void SetTooltipData(Province province)
    {
        Country provinceOwner = map.Countries[province.OwnerId];
        country_province_name.text = !province.IsLand ? "Ocean" : (province.OwnerId != 0 ? provinceOwner.Name : "Tribal") + " - " + province.Name;
        provinceOwner.SetCoatandColor(country_coat);

        switch (filter_modes.CurrentMode)
        {
            case filter_modes.MapMode.Terrain:
                terrain_name.text = GetTerrainName(province.Terrain);
                terrain_row.SetActive(true);
                resource_row.SetActive(false);
                happiness_row.SetActive(false);
                population_row.SetActive(false);
                relation_row.SetActive(false);
                break;

            case filter_modes.MapMode.Resource:
                resource_img.sprite = province_row_script.GetResourceSprite(province.ResourceType);
                terrain_row.SetActive(false);
                resource_row.SetActive(true);
                happiness_row.SetActive(false);
                population_row.SetActive(false);
                relation_row.SetActive(false);
                break;

            case filter_modes.MapMode.Happiness:
                happiness_value.text = province.Happiness.ToString() + "%";
                happiness_value.color = province_row_script.GetHappinessColor(province.Happiness);
                terrain_row.SetActive(false);
                resource_row.SetActive(false);
                happiness_row.SetActive(true);
                population_row.SetActive(false);
                relation_row.SetActive(false);
                break;

            case filter_modes.MapMode.Population:
                population_value.text = province.Population.ToString();
                terrain_row.SetActive(false);
                resource_row.SetActive(false);
                happiness_row.SetActive(false);
                population_row.SetActive(true);
                relation_row.SetActive(false);
                break;

            case filter_modes.MapMode.Political:
                terrain_row.SetActive(false);
                resource_row.SetActive(false);
                happiness_row.SetActive(false);
                population_row.SetActive(false);
                relation_row.SetActive(false);
                break;

            case filter_modes.MapMode.Diplomatic:
                var relationType = map.GetHardRelationType(map.CurrentPlayer, provinceOwner);
                Sprite relationSprite = GetRelationSprite(relationType, provinceOwner);
                if (relationSprite != null)
                {
                    relation_img.gameObject.SetActive(true);
                    relation_img.sprite = relationSprite;
                }
                else relation_img.gameObject.SetActive(false);
                terrain_row.SetActive(false);
                resource_row.SetActive(false);
                happiness_row.SetActive(false);
                population_row.SetActive(false);
                relation_row.SetActive(true);
                break;
        }
    }

    private string GetTerrainName(Province.TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Tundra:
                return "Tundra";
            case TerrainType.Forest:
                return "Forest";
            case TerrainType.Lowlands:
                return "Lowlands";
            case TerrainType.Desert:
                return "Desert";
            case TerrainType.Ocean:
                return "Ocean";
            default:
                return "Unknown";
        }
    }

    private Sprite GetRelationSprite(RelationType? relationType, Country country)
    {
        switch (relationType)
        {
            case RelationType.War:
                return war_sprite;
            case RelationType.Truce:
                return alliance_sprite;
            case RelationType.Alliance:
                return truce_sprite;
            case RelationType.Vassalage:
                var isSide0 = map.Relations
                    .FirstOrDefault(r => r.Type == RelationType.Vassalage &&
                                         r.Sides.Contains(map.CurrentPlayer) &&
                                         r.Sides.Contains(country))?.Sides[0] == map.CurrentPlayer;
                return isSide0 ? vassalage_sprite_1 : vassalage_sprite_2;
            case RelationType.Rebellion:
                return rebellion_sprite;
            default:
                return null;
        }
    }
}
