using Mirror;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Newtonsoft.Json;
using static Map;
using static player_table;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using Telepathy;
using Unity.VisualScripting;

public class MultiplayerLobby : NetworkBehaviour
{
    [SerializeField] private GameObject dummy;
    [SerializeField] private GameObject playerTable;
    [SerializeField] private Map map;

    private List<CountryData> currentStates = new List<CountryData>();
    private List<Province> provinces = new List<Province>();

    [SyncVar(hook = nameof(UpdateCountryAssignmentWithDelay))]
    
    public SyncList<int> countryPlayerAssignment = new SyncList<int>();

    private IEnumerator DelayedRpcUpdateCountryUI()
    {

        yield return new WaitForSeconds(0.1f);
        RpcUpdateCountryUI();
    }

    public void UpdateCountryAssignmentWithDelay()
    {
        StartCoroutine(DelayedRpcUpdateCountryUI());
    }

    public void LoadMap(string mapName)
    {
        string json = LoadJsonFromFile($"Assets/Resources/{mapName}.json");
        GameState gameState = JsonConvert.DeserializeObject<GameState>(json);

        currentStates = gameState.states;
        provinces = gameState.provinces;

        showCountries(currentStates);

        if (isServer)
        {
            countryPlayerAssignment.Clear();
            for (int i = 0; i < currentStates.Count; i++)
            {
                countryPlayerAssignment.Add(0); // Dodajemy 0, czyli AI
            }
            UpdateCountryAssignmentWithDelay();
        }
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

    [Server]
    public void AssignCountryToPlayer(int countryId, NetworkIdentity playerIdentity)
    {
        if (countryId < 0 || countryId >= countryPlayerAssignment.Count)
        {
            return;
        }

        if (countryPlayerAssignment[countryId] == 0)
        {
            if(countryPlayerAssignment.Contains((int)playerIdentity.netId))
            {
                countryPlayerAssignment[countryPlayerAssignment.IndexOf((int)playerIdentity.netId)] = 0;
            }
            countryPlayerAssignment[countryId] = (int)playerIdentity.netId;
        }
        UpdateCountryAssignmentWithDelay();
    }

    [ClientRpc]
    private void RpcUpdateCountryUI()
    { 
        for (int i = 0; i < countryPlayerAssignment.Count; i++)
        {
            int countryId = i;
            int playerNumber = countryPlayerAssignment[i];

            GameObject countryUI = playerTable.transform.Cast<Transform>()
                .Select(child => child.gameObject)
                .FirstOrDefault(ui => GetCountryIdFromUI(ui) == countryId);

            if (countryUI != null)
            {
                TMP_Text countryNameText = countryUI.transform.Find("controller")?.GetComponentInChildren<TMP_Text>();
                if (countryNameText != null)
                {
                    if (playerNumber != 0)
                    {
                        PlayerInfo playerInfo;
                        if (isServer)
                        {
                            playerInfo = FindPlayerById(playerNumber);
                        }
                        else
                        {
                            playerInfo = FindPlayerByIdOnClient(playerNumber);
                        }
                        if (playerInfo != null)
                        {
                            countryNameText.text = playerInfo.GetPlayerName();
                        }
                    }
                    else
                    {
                        countryNameText.text = "AI";
                    }
                }
            }
        }
    }

    public PlayerInfo FindPlayerById(int id)
    {
        foreach (var networkIdentity in NetworkServer.spawned.Values) // Działa tylko na serwerze
        {
            PlayerInfo playerInfo = networkIdentity.GetComponent<PlayerInfo>();
            if (playerInfo != null && playerInfo.playerId == id)
            {
                return playerInfo;
            }
        }
        return null; 
    }
    public static PlayerInfo FindPlayerByIdOnClient(int id)
    {
        foreach (var playerInfo in FindObjectsOfType<PlayerInfo>())
        {
            if (playerInfo.playerId == id)
            {
                return playerInfo;
            }
        }
        return null;
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
                    countryButton.onClick.AddListener(() => OnCountryClicked(capturedId));
                }
            }
        }
    }

    private void OnCountryClicked(int countryId)
    {

        if (countryPlayerAssignment[countryId] != 0)
        {
            return;
        }
        var localPlayer = FindPlayerByIdOnClient((int)NetworkClient.localPlayer.netId);
        var playerCommands = localPlayer.GetComponent<PlayerLobbyCommands>();
        if (playerCommands != null)
        {
            playerCommands.CmdAssignCountryToPlayer(countryId);
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
        // dodać przekazanie kraje do mapy moze zapisywanie do playera wybranego kraju? bedzie tak najprościej bo netId bedzie wieksze o player count.
        // i np niech bedzie przekazywana lista controllerow i jak bedzie Net to bedzie pobierać kontrolowany kraj z gracza
    }
}