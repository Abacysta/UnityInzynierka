﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;
using static Map;
using static player_table;

public class MultiplayerLobby : NetworkBehaviour
{
    [SerializeField] private GameObject dummy;
    [SerializeField] private GameObject playerTable;
    [SerializeField] private Map map;

    public List<CountryController> controllers = new List<CountryController>();
    private List<CountryData> currentStates = new List<CountryData>();
    private List<Province> provinces = new List<Province>();

    public int currentMaxPlayerNumber = 0;

    [SyncVar] public readonly SyncDictionary<int, int> countryPlayerAssignment = new SyncDictionary<int, int>();

    public GameObject localPlayer;

    public override void OnStartClient()
    {
        base.OnStartClient();
        localPlayer = NetworkClient.localPlayer?.gameObject;
        if (localPlayer == null)
        {
            StartCoroutine(AssignLocalPlayer());
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server is up.");
    }
    private IEnumerator AssignLocalPlayer()
    {
        while (NetworkClient.localPlayer == null)
        {
            yield return null;
        }

        localPlayer = NetworkClient.localPlayer.gameObject;
        Debug.Log("Local player successfully assigned in MultiplayerLobby.");
    }
    public void LoadMap(string mapName)
    {
        string json = LoadJsonFromFile($"Assets/Resources/{mapName}.json");
        GameState gameState = JsonConvert.DeserializeObject<GameState>(json);

        currentStates = gameState.states;
        provinces = gameState.provinces;

        showCountries(currentStates);

        controllers.Clear();
        controllers = Enumerable.Repeat(CountryController.Ai, currentStates.Count).ToList();
    }
    private string LoadJsonFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError("Plik nie został znaleziony: " + filePath);
            return null;
        }
    }

    // Assigns country to player on the server
    [Server]
    public void AssignCountryToPlayer(int countryId, int playerNumber)
    {
        if (countryPlayerAssignment.ContainsKey(countryId))
        {
            countryPlayerAssignment.Remove(countryId);
        }

        countryPlayerAssignment[countryId] = playerNumber;
        foreach (var assignment in countryPlayerAssignment)
        {
            Debug.Log($"Country {assignment.Key} assigned to player {assignment.Value}");
        }
        // Update the UI after the change
        RpcUpdateCountryUI();
    }

    [ClientRpc]
    private void RpcUpdateCountryUI()
    {
        UpdateCountryUI();
    }
    [Command(requiresAuthority = false)]
    public void UpdateCountryUI()
    {
        foreach (var assignment in countryPlayerAssignment)
        {
            int countryId = assignment.Key;
            int playerNumber = assignment.Value;

            GameObject countryUI = playerTable.transform.Cast<Transform>()
                .Select(child => child.gameObject)
                .FirstOrDefault(ui => GetCountryIdFromUI(ui) == countryId);

            if (countryUI != null)
            {
                TMP_Text countryNameText = countryUI.transform.Find("controller")?.GetComponentInChildren<TMP_Text>();
                if (countryNameText != null)
                {
                    if (countryId != 0)
                    {
                        var playerInfo = localPlayer.GetComponent<PlayerInfo>();
                        countryNameText.text = playerInfo != null ? playerInfo.GetPlayerName() : "Player";
                    }
                    else
                    {
                        countryNameText.text = "AI";
                    }
                }
            }
        }
    }

    private int GetCountryIdFromUI(GameObject countryUI)
    {
        Transform nameTransform = countryUI.transform.Find("name");
        if (nameTransform != null)
        {
            TMP_Text countryNameText = nameTransform.GetComponentInChildren<TMP_Text>();
            if (countryNameText != null)
            {
                string countryName = countryNameText.text;
                CountryData state = currentStates.FirstOrDefault(s => s.name == countryName);
                return currentStates.IndexOf(state);
            }
        }
        return -1;
    }

    public void showCountries(List<CountryData> states)
    {
        foreach (Transform child in playerTable.transform)
        {
            Destroy(child.gameObject);
        }

        float yOffset = 0f;

        for (int i = 0; i < states.Count; i++)
        {
            CountryData state = states[i];

            GameObject countryUI = Instantiate(dummy, playerTable.transform);

            RectTransform rt = countryUI.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - yOffset);
                yOffset += 50f;
            }

            Transform nameTransform = countryUI.transform.Find("name");
            if (nameTransform != null)
            {
                TMP_Text countryNameText = nameTransform.GetComponentInChildren<TMP_Text>();
                if (countryNameText != null)
                {
                    countryNameText.text = state.name;
                }
            }

            Image nameBackgroundImage = nameTransform?.GetComponent<Image>();
            if (nameBackgroundImage != null)
            {
                nameBackgroundImage.color = new Color(
                    state.color[0] / 255f,
                    state.color[1] / 255f,
                    state.color[2] / 255f,
                    70f / 255f
                );
            }

            Transform emblemTransform = countryUI.transform.Find("emblem");
            if (emblemTransform != null)
            {
                Image emblemImage = emblemTransform.GetComponent<Image>();
                if (emblemImage != null)
                {
                    string emblemPath = "sprites/coat_" + state.coat;
                    Sprite emblemSprite = Resources.Load<Sprite>(emblemPath);
                    if (emblemSprite != null)
                    {
                        emblemImage.sprite = emblemSprite;
                        emblemImage.color = new Color(
                            state.color[0] / 255f,
                            state.color[1] / 255f,
                            state.color[2] / 255f
                        );
                    }
                }
            }

            Transform controllerTransform = countryUI.transform.Find("controller");
            if (controllerTransform != null)
            {
                Button countryButton = controllerTransform.GetComponent<Button>();
                if (countryButton != null)
                {
                    int capturedId = i;
                    countryButton.onClick.AddListener(() => OnCountryClicked(countryUI, controllerTransform, capturedId));
                }
            }
        }
    }

    private void OnCountryClicked(GameObject countryUI, Transform nameTransform, int countryId)
    {
        if (countryId < 0 || countryId >= controllers.Count)
        {
            Debug.LogError($"Invalid countryId: {countryId}. Must be between 0 and {controllers.Count - 1}.");
            return;
        }

        var playerCommands = localPlayer.GetComponent<PlayerLobbyCommands>();
        if (playerCommands != null)
        {
            Debug.Log($"Assigning country {countryId} to player via CmdAssignCountryToPlayer.");
            playerCommands.CmdAssignCountryToPlayer(countryId);
        }
        else
        {
            Debug.LogError("PlayerLobbyCommands component not found on localPlayer.");
        }
    }

    private Color toColor(int[] color)
    {
        return new Color(color[0] / 255f, color[1] / 255f, color[2] / 255f);
    }
    public void StartGame()
    {
        if (currentStates == null || currentStates.Count == 0)
        {
            Debug.LogError("Brak wczytanych stanów w currentStates.");
            return;
        }

        map.Countries.Clear();
        map.Controllers.Clear();
        map.addCountry(new Country(0, "", (-1, -1), new Color(0.8392f, 0.7216f, 0.4706f), 1, map), Map.CountryController.Ai);

        foreach (CountryData state in currentStates)
        {
            if (state.owner_id == 0)
            {
                continue;
            }
            Country newCountry = new Country(
                state.owner_id,
                state.name,
                (state.capitol[0], state.capitol[1]),
                toColor(state.color),
                state.coat,
                map
            );

            map.addCountry(newCountry, CountryController.Ai);


            Debug.Log($"Dodano kraj: {newCountry.Name}, ID: {newCountry.Id}");
        }

        map.Provinces.Clear();
        int j = 1;
        foreach (var provinceData in provinces)
        {
            Province.TerrainType terrain;
            if (provinceData.Type == "land")
            {
                string terrainStr = provinceData.Terrain.ToString().ToLower();
                switch (terrainStr)
                {
                    case "forest":
                        terrain = Province.TerrainType.forest;
                        break;
                    case "desert":
                        terrain = Province.TerrainType.desert;
                        break;
                    case "lowlands":
                        terrain = Province.TerrainType.lowlands;
                        break;
                    case "tundra":
                        terrain = Province.TerrainType.tundra;
                        break;
                    default:
                        Debug.LogWarning($"Nieznany typ terenu: {terrainStr}, ustawiam tundra jako domyślny.");
                        terrain = Province.TerrainType.tundra;
                        break;
                }
            }
            else
            {
                terrain = Province.TerrainType.ocean;
            }
            Province newProvince = new Province(
                    j++.ToString(),
                    provinceData.Name,
                    provinceData.X,
                    provinceData.Y,
                    provinceData.Type,
                    terrain,
                    provinceData.Resources,
                    (int)provinceData.Resources_amount,
                    provinceData.Population,
                    (int)provinceData.Rec_pop,
                    50,
                    provinceData.Is_coast,
                    provinceData.Owner_id
                );

            map.Provinces.Add(newProvince);
        }

        for (int i = 1; i <= controllers.Count; i++)
        {
            map.Controllers[i] = controllers[i - 1];
        }

        Debug.Log("Game setup complete. Ready to start the game.");
    }
}
