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

    [SerializeField] private Map map;

    [SerializeField] private Sprite neutral_sprite;
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
        mousePos.y += 150f;
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
        Country provinceOwner = map.Countries[province.Owner_id];

        country_province_name.text = (province.Owner_id != 0 ? provinceOwner.Name : "Tribal") + " - " + province.Name;

        provinceOwner.setCoatandColor(country_coat);
        terrain_name.text = GetTerrainName(province.Terrain);
        resource_img.sprite = province_row_script.GetResourceSprite(province.ResourcesT);
        happiness_value.text = province.Happiness.ToString() + "%";
        happiness_value.color = province_row_script.GetHappinessColor(province.Happiness);
        population_value.text = province.Population.ToString();

        var relationType = map.GetHardRelationType(map.CurrentPlayer, provinceOwner);
        relation_img.sprite = GetRelationSprite(relationType, provinceOwner);
    }

    private string GetTerrainName(Province.TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.tundra:
                return "Tundra";
            case TerrainType.forest:
                return "Forest";
            case TerrainType.lowlands:
                return "Lowlands";
            case TerrainType.desert:
                return "Desert";
            case TerrainType.ocean:
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
                    .FirstOrDefault(r => r.type == RelationType.Vassalage &&
                                         r.Sides.Contains(map.CurrentPlayer) &&
                                         r.Sides.Contains(country))?.Sides[0] == map.CurrentPlayer;
                return isSide0 ? vassalage_sprite_1 : vassalage_sprite_2;
            case RelationType.Rebellion:
                return rebellion_sprite;
            default:
                return neutral_sprite;
        }
    }
}
