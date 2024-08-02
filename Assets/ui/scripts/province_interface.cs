using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class province_interface : MonoBehaviour
{
    public TMP_Text id, res, type, pop, rec_pop;
    public TMP_Text buildingNameText;
    public TMP_Text buildingLevelText;
    public TMP_Text occupationCountText;
    public TMP_Text occupationText;
    public Button upgradeButton;
    public Button buildFortButton;
    public Button buildInfrastructureButton;
    public Button buildMineButton;
    public Button buildSchoolButton;
    public Button occupationButton;
    public Map map;
    private int prov;
    private BuildingManager buildingManager;

    private void Start() {
        buildingManager = FindObjectOfType<BuildingManager>();

        upgradeButton.onClick.AddListener(UpgradeBuilding);
        buildFortButton.onClick.AddListener(() => SelectBuilding(buildingManager.fortBuilding));
        buildInfrastructureButton.onClick.AddListener(() => SelectBuilding(buildingManager.infrastructureBuilding));
        buildMineButton.onClick.AddListener(() => SelectBuilding(buildingManager.mineBuilding));
        buildSchoolButton.onClick.AddListener(() => SelectBuilding(buildingManager.schoolBuilding));
        id.SetText("null");
        res.SetText("null");
        type.SetText("null");
        pop.SetText("null");
        rec_pop.SetText("null");
        occupationText.SetText("null");
        occupationCountText.SetText("null");
        buildingNameText.SetText("No building");
        buildingLevelText.SetText("Level 0");
    }

    private void Update() {
        var coordinates = map.Selected_province;
        prov = map.Provinces.FindIndex(p => p.X == coordinates.Item1 && p.Y == coordinates.Item2);
        id.SetText("id:" + map.Provinces[prov].X + '_' + map.Provinces[prov].Y);
        //icons TBD
        res.SetText("resource:" + map.Provinces[prov].Resources);
        type.SetText(map.Provinces[prov].Type == "land" ? "land" : "sea");
        pop.SetText("population " + map.Provinces[prov].Population);
        rec_pop.SetText("recruitable population " + map.Provinces[prov].RecruitablePopulation);
        occupationText.SetText(""+map.Provinces[prov].Occupation);
        occupationCountText.SetText(""+map.Provinces[prov].Occupation_count);

        if (map.Provinces[prov].Current_building != null)
        {
            buildingNameText.SetText(map.Provinces[prov].Current_building.buildingName);
            buildingLevelText.SetText("Level " + map.Provinces[prov].Current_building.buildingLevel);
            SetBuildingDetailsActive(true);
            SetBuildingButtonsActive(false);
        }
        else
        {
            SetBuildingDetailsActive(false);
            SetBuildingButtonsActive(true);
        }
    }

    public void PopulationIncrease(int val) {
        map.Provinces[prov].Population += val;
    }

    public void SelectBuilding(Building building)
    {
        if (buildingManager != null)
        {
            buildingManager.SelectBuilding(building);
            BuildSelectedBuilding();
        }
    }

    public void BuildSelectedBuilding()
    {
        if (buildingManager != null)
        {
            var coordinates = map.Selected_province;
            buildingManager.BuildSelectedBuilding(coordinates.Item1, coordinates.Item2);
        }
    }

    private void UpgradeBuilding()
    {
        if (buildingManager != null)
        {
            buildingManager.UpgradeBuilding(map.Provinces[prov].X, map.Provinces[prov].Y);
        }
    }

    private void SetBuildingButtonsActive(bool isActive)
    {
        buildFortButton.gameObject.SetActive(isActive);
        buildInfrastructureButton.gameObject.SetActive(isActive);
        buildMineButton.gameObject.SetActive(isActive);
        buildSchoolButton.gameObject.SetActive(isActive);
    }

    private void SetBuildingDetailsActive(bool isActive)
    {
        buildingNameText.gameObject.SetActive(isActive);
        buildingLevelText.gameObject.SetActive(isActive);
        upgradeButton.gameObject.SetActive(isActive);
    }
    public void SetOccupation()
    {
        var coordinates = map.Selected_province;
        prov = map.Provinces.FindIndex(p => p.X == coordinates.Item1 && p.Y == coordinates.Item2);

        Province province = map.Provinces[prov];
        province.Occupation = true;
        province.Occupation_count = 3;
    }
}
