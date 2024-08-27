using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_tooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text provinceNameText;
    [SerializeField] private TMP_Text happinessText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text terrainText;
    [SerializeField] private TMP_Text buildingsText;

    [SerializeField] private Map map;
    public void SetTooltipData(Province province)
    {
        if(province.Owner_id != 0)
        {
            provinceNameText.text = $"Province: {map.Countries[province.Owner_id].Name}";
        }
        else
        {
            provinceNameText.text = $"Province: Neutral";
        }
        happinessText.text = $"Happiness: {province.Happiness}";
        populationText.text = $"Population: {province.Population}";
        terrainText.text = $"Terrain: {province.Type}";
        buildingsText.text = "";
        foreach (var building in province.Buildings)
        {
            if(building.BuildingLevel != 0)
            {
                buildingsText.text += $"{building.BuildingType} - {building.BuildingLevel}\t";
            }
        }

        Vector3 mousePos = Input.mousePosition;
        transform.position = mousePos;
    }
}
