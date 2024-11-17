//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;


//public class buildings_interface : MonoBehaviour
//{
//    [SerializeField] private Button buildFortButton, buildMineButton, buildInfraButton, buildSchoolButton;

//    [SerializeField] private GameObject upgradeDowngradePrefab;
//    [SerializeField] private Transform buttonContainer;

//    private Map map;

//    public void Initialize(Map map)
//    {
//        this.map = map;

//        buildFortButton.onClick.AddListener(() => BuildBuilding(BuildingType.Fort));
//        buildMineButton.onClick.AddListener(() => BuildBuilding(BuildingType.Mine));
//        buildInfraButton.onClick.AddListener(() => BuildBuilding(BuildingType.Infrastructure));
//        buildSchoolButton.onClick.AddListener(() => BuildBuilding(BuildingType.School));
//    }
//    private void BuildBuilding(BuildingType buildingType)
//    {
//        Debug.Log("BuildBuilding called with type: " + buildingType);
//        var coordinates = map.Selected_province;
//        Building newBuilding = new Building(buildingType);
//        //map.addBuilding(coordinates, newBuilding); rozpedz sie i zapierdol glowa w sciane
//        UpdateInterface();
//    }
//    public void UpdateInterface()
//    {
//        foreach (Transform child in buttonContainer)
//        {
//            Destroy(child.gameObject);
//        }

//        var coordinates = map.Selected_province;
//        int prov = map.getProvinceIndex(coordinates);
//        foreach (var building in map.Provinces[prov].Buildings)
//        {
//            GameObject upgradeDowngradeButtons = Instantiate(upgradeDowngradePrefab, buttonContainer);

//            TMP_Text buildingText = upgradeDowngradeButtons.transform.Find("level_text").GetComponent<TMP_Text>();
//            buildingText.SetText(building.BuildingType.ToString() + " level: " + building.BuildingLevel.ToString());

//            Button upgradeButton = upgradeDowngradeButtons.transform.Find("upgrade_button").GetComponent<Button>();
//            Button downgradeButton = upgradeDowngradeButtons.transform.Find("downgrade_button").GetComponent<Button>();

//            if (upgradeButton == null || downgradeButton == null)
//            {
//                Debug.LogError("Upgrade or Downgrade Button not found.");
//                continue;
//            }

//            upgradeButton.onClick.RemoveAllListeners(); // Ensure old listeners are removed
//            downgradeButton.onClick.RemoveAllListeners();

//            upgradeButton.onClick.AddListener(() => {
//                Debug.Log("Upgrade Button Clicked");
//                UpgradeBuilding(building.BuildingType);
//            });

//            downgradeButton.onClick.AddListener(() => {
//                Debug.Log("Downgrade Button Clicked");
//                DowngradeBuilding(building.BuildingType);
//            });
//        }
//    }

//    private void UpgradeBuilding(BuildingType buildingType)
//    {
//        Debug.Log("UpgradeBuilding called with type: " + buildingType);
//        var coordinates = map.Selected_province;
//        //map.upgradeBuilding(coordinates, buildingType);
//        UpdateInterface();
//    }

//    private void DowngradeBuilding(BuildingType buildingType)
//    {
//        Debug.Log("DowngradeBuilding called with type: " + buildingType);
//        var coordinates = map.Selected_province;
//        //map.downgradeBuilding(coordinates, buildingType);
//        UpdateInterface();
//    }

//}
