
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Map;

public class map_options : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown mapDropdown;
    [SerializeField] private GameObject optionsTable;
    [SerializeField] private player_table playerTable;
    private List<string> availableMaps = new List<string>();
    [SerializeField] private Map map;
    private string selectedMap;
    public string SelectedMap => selectedMap;
    private int selectedResourceRate = 100;

    
    private int selectedTurnLimit = 80;

    private int minResourceRate = 50;
    private int maxResourceRate = 200;

    private int minTurnLimit = 20;
    private int maxTurnLimit = 160;

    private int resourceValue = 5;
    private int shiftInc = 5;

    private int turnValue = 1;
    void Start()
    {
        LoadAvailableMaps();
        SetupResourceOptions();
        SetupTurnOptions();
	}

    private void LoadAvailableMaps()
    {
        availableMaps.Clear();
        string mapsPath;
        if(Application.isEditor)
        {
            mapsPath = Path.Combine(Application.dataPath, "Resources/Maps");
        }
        else
        {
            mapsPath = Path.Combine(Application.dataPath, "../Maps");
        }
        if (Directory.Exists(mapsPath))
        {
            string[] mapFiles = Directory.GetFiles(mapsPath, "*.json");
            foreach (var file in mapFiles)
            {
                string mapName = Path.GetFileNameWithoutExtension(file);
                availableMaps.Add(mapName);
            }
        }
        else
        {
            Debug.LogError($"Nie znaleziono folderu z mapami: {mapsPath}");
        }
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(availableMaps);

        if (availableMaps.Count > 0)
        {
            selectedMap = availableMaps[0];
            playerTable.LoadMap(selectedMap);
            mapDropdown.onValueChanged.AddListener(delegate { OnMapSelected(mapDropdown); });
        }
        else
        {
            Debug.LogError("Brak dostêpnych map!");
        }
    }
    public void SelectMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex <= availableMaps.Count)
        {
            selectedMap = availableMaps[mapIndex];
            playerTable.LoadMap(selectedMap);
        }
    }
    public string GetSelectedMap()
    {
        return selectedMap;
    }
    private void SetupResourceOptions()
    {
        Transform option = optionsTable.transform.Find("option_1");

        if (option != null)
        {
            TMP_Text resValueText = option.Find("resValue").GetComponent<TMP_Text>();
            if (resValueText != null)
            {
                resValueText.text = selectedResourceRate + " %";
            }

            Button increaseButton = option.Find("resInc").GetComponent<Button>();
            if (increaseButton != null)
            {
                increaseButton.onClick.AddListener(() =>
                {
                    AddValueRes();
                });
            }

            Button decreaseButton = option.Find("resDec").GetComponent<Button>();
            if (decreaseButton != null)
            {
                decreaseButton.onClick.AddListener(() =>
                {
                    DecValueRes();
                });
            }
        }
    }
    private void SetupTurnOptions()
    {
        Transform option = optionsTable.transform.Find("option_2");

        if (option != null)
        {
            TMP_Text turnValueText = option.Find("turnValue").GetComponent<TMP_Text>();
            if (turnValueText != null)
            {
                turnValueText.text = selectedTurnLimit + " turns";
            }

            Button increaseButton = option.Find("turnInc").GetComponent<Button>();
            if (increaseButton != null)
            {
                increaseButton.onClick.AddListener(() =>
                {
                    AddValueTurn();
                });
            }

            Button decreaseButton = option.Find("turnDec").GetComponent<Button>();
            if (decreaseButton != null)
            {
                decreaseButton.onClick.AddListener(() =>
                {
                    DecValueTurn();
                });
            }
        }
    }

    public void AddValueRes()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            selectedResourceRate = maxResourceRate;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (selectedResourceRate + resourceValue * shiftInc <= maxResourceRate)
                selectedResourceRate += resourceValue * shiftInc;
            else
                selectedResourceRate = maxResourceRate;
        }
        else
        {
            if (selectedResourceRate + resourceValue <= maxResourceRate)
                selectedResourceRate += resourceValue;
        }

        UpdateResourceText();
    }

    public void DecValueRes()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            selectedResourceRate = minResourceRate;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (selectedResourceRate - resourceValue * shiftInc >= minResourceRate)
                selectedResourceRate -= resourceValue * shiftInc;
            else
                selectedResourceRate = minResourceRate;
        }
        else
        {
            if (selectedResourceRate - resourceValue >= minResourceRate)
                selectedResourceRate -= resourceValue;
        }

        UpdateResourceText();
    }

    public void AddValueTurn()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            selectedTurnLimit = maxTurnLimit;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (selectedTurnLimit + turnValue * shiftInc <= maxTurnLimit)
                selectedTurnLimit += turnValue * shiftInc;
            else
                selectedTurnLimit = maxTurnLimit;
        }
        else
        {
            if (selectedTurnLimit + turnValue <= maxTurnLimit)
                selectedTurnLimit += turnValue;
        }

        UpdateTurnText();
    }

    public void DecValueTurn()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            selectedTurnLimit = minTurnLimit;
        }
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (selectedTurnLimit - turnValue * shiftInc >= minTurnLimit)
                selectedTurnLimit -= turnValue * shiftInc;
            else
                selectedTurnLimit = minTurnLimit;
        }
        else
        {
            if (selectedTurnLimit - turnValue >= minTurnLimit)
                selectedTurnLimit -= turnValue;
        }

        UpdateTurnText();
    }

    private void UpdateResourceText()
    {
        Transform option = optionsTable.transform.Find("option_1/resValue");
        if (option != null)
        {
            TMP_Text resValueText = option.GetComponent<TMP_Text>();
            if (resValueText != null)
            {
                resValueText.text = selectedResourceRate + " %";
            }
        }
    }

    private void UpdateTurnText()
    {
        Transform option = optionsTable.transform.Find("option_2/turnValue");
        if (option != null)
        {
            TMP_Text turnValueText = option.GetComponent<TMP_Text>();
            if (turnValueText != null)
            {
                turnValueText.text = selectedTurnLimit + " turns";
            }
        }
    }

    private void OnMapSelected(TMP_Dropdown dropdown)
    {
        selectedMap = availableMaps[dropdown.value];
        playerTable.LoadMap(selectedMap);
    }

    public void StartGame()
    {
        if (!playerTable.Controllers.Contains(CountryController.Local))
        {
            Debug.Log("At least one human player!");
            return;
        }

        map.FileName = selectedMap;
        map.ResourceRate = selectedResourceRate;
        map.Turnlimit = selectedTurnLimit;

        playerTable.StartGame();

        SceneManager.LoadScene("game_map");
    }
    public void ExitButton()
    {
        SceneManager.LoadScene("main_menu");
    }
}
