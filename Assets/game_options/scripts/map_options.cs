using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown mapDropdown;       
    [SerializeField] private TMP_Dropdown resourceDropdown;   
    [SerializeField] private TMP_Dropdown turnLimitDropdown;  

    private List<string> availableMaps = new List<string>(); 
    private string selectedMap;
    private int selectedResourceRate;
    private int selectedTurnLimit;

    void Start()
    {
        LoadAvailableMaps();   
        SetupResourceOptions(); 
        SetupTurnLimitOptions();  
    }

    private void LoadAvailableMaps()
    {

        availableMaps.Add("Map 1");
        availableMaps.Add("Map 2");
        availableMaps.Add("Map 3");

        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(availableMaps);

        selectedMap = availableMaps[0];
        mapDropdown.onValueChanged.AddListener(delegate { OnMapSelected(mapDropdown); });
    }

    private void SetupResourceOptions()
    {
        List<string> resourceOptions = new List<string> { "75%", "100%", "125%" };

        resourceDropdown.ClearOptions();
        resourceDropdown.AddOptions(resourceOptions);

        selectedResourceRate = 100;
        resourceDropdown.value = 1;  
        resourceDropdown.onValueChanged.AddListener(delegate { OnResourceRateSelected(resourceDropdown); });
    }

    private void SetupTurnLimitOptions()
    {
        List<string> turnLimitOptions = new List<string> { "60", "80", "100" };

        turnLimitDropdown.ClearOptions();
        turnLimitDropdown.AddOptions(turnLimitOptions);

        selectedTurnLimit = 80;
        turnLimitDropdown.value = 1; 
        turnLimitDropdown.onValueChanged.AddListener(delegate { OnTurnLimitSelected(turnLimitDropdown); });
    }

    private void OnMapSelected(TMP_Dropdown dropdown)
    {
        selectedMap = availableMaps[dropdown.value];
    }

    private void OnResourceRateSelected(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                selectedResourceRate = 75;
                break;
            case 1:
                selectedResourceRate = 100;
                break;
            case 2:
                selectedResourceRate = 125;
                break;
        }
    }
    private void OnTurnLimitSelected(TMP_Dropdown dropdown)
    {
        selectedTurnLimit = int.Parse(dropdown.options[dropdown.value].text);
    }

    public void StartGame()
    {
    }
}
