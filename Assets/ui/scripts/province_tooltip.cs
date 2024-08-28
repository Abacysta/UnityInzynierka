using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_tooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text provinceNameText;
   // [SerializeField] private TMP_Text happinessText;
   // [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text terrainText;
    //[SerializeField] private TMP_Text buildingsText;

    [SerializeField] private Map map;

    private Coroutine tooltip;
    private GameObject oTooltip;

    private void Start() {
        oTooltip = gameObject.transform.GetChild(0).gameObject;
    }

    private void Update() {
        Vector3 mousePos = Input.mousePosition; 
        mousePos.x += 120f;
        mousePos.y -= 65f;
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
        if(province.Owner_id != 0)
        {
            provinceNameText.text = map.Countries[province.Owner_id].Name;
        }
        else
        {
            provinceNameText.text = "Tribal";
        }
       // happinessText.text = $"Happiness: {province.Happiness}";
        //populationText.text = $"Population: {province.Population}";
        terrainText.text = $"Terrain: {province.Type}";
        //buildingsText.text = "";
        //foreach (var building in province.Buildings)
        //{
        //    if(building.BuildingLevel != 0)
        //    {
        //        buildingsText.text += $"{building.BuildingType} - {building.BuildingLevel}\t";
        //    }
        //}
    }
}
