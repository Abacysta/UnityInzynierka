using Assets.classes.subclasses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using static Assets.classes.subclasses.Constants;
using UnityEngine;
using UnityEngine.UI;
using static Map;

public class player_table : MonoBehaviour
{
    [SerializeField] private GameObject dummy;
    [SerializeField] private GameObject playerTable;
    [SerializeField] private Map map;
    [SerializeField] private map_options optionsTable;

    private GameObject currentPlayerSelection = null;
    public List<CountryController> controllers = new List<CountryController>();
    private List<CountryData> currentStates = new List<CountryData>();
    private List<Province> provinces = new List<Province>();

    private int currentMaxPlayerNumber = 0;
    private Dictionary<int, int> countryPlayerAssignment = new Dictionary<int, int>();

    public class CountryData
    {
        public int owner_id;
        public string name;
        public int[] color;
        public int coat;
        public int[] capitol;
    }

    public class GameState
    {
        public List<CountryData> states;
        public List<Province> provinces;
    }

    public void LoadMap(string mapName)
    {
        string json = LoadJsonFromFile($"Assets/Resources/{mapName}.json");
        GameState gameState = JsonConvert.DeserializeObject<GameState>(json);

        currentStates = gameState.states;
        provinces = gameState.provinces;

        showCountries(currentStates);
        showButton();
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

        ClearMap();

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
                ParseResource(provinceData.ResourceType.ToString().ToLower()),
                (int)provinceData.ResourceAmount,
                provinceData.Population,
                1,
                50,
                provinceData.Is_coast,
                provinceData.Owner_id
            );

            map.Provinces.Add(newProvince);
        }

        map.addCountry(new Country(0, "", DEFAULT_CORD, new Color(0.8392f, 0.7216f, 0.4706f), 1, map), Map.CountryController.Ai);

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

        for (int i = 1; i <= controllers.Count; i++)
        {
            map.Controllers[i] = controllers[i - 1];
        }
		//createTestCountrys();
        map.initCountries();
		SetCurrentPlayer();
        SetCountryPrioritiesAndOpinions();
        InitializeProvinces();
        map.calcPopExtremes();
        map.turnCnt = 0;
        Debug.Log("Game setup complete. Ready to start the game. " + map.Countries.Count + " countries present.");
    }

    private void ClearMap()
    {
        map.Armies.Clear();
        map.ArmyViews.Clear();
        map.Countries.Clear();
        map.Controllers.Clear();
        map.Provinces.Clear();
    }

    private void SetCurrentPlayer()
    {
        int playerIndex = map.Controllers.FindIndex(controller => controller == Map.CountryController.Local);
        if (playerIndex >= 0)
        {
            map.currentPlayer = playerIndex;
            Debug.Log($"Gracz ustawiony na kraj: {map.CurrentPlayer.Name}");
        }
        else
        {
            Debug.LogError("Nie udało się znaleźć gracza (CountryController.Local)");
        }
    }

    private void SetCountryPrioritiesAndOpinions()
    {
        int i = 0;

        foreach (Country country in map.Countries.Where(c => c.Id != 0))
        {
            country.Priority = i++;
            //foreach (var c in map.Countries.Where(c => c != country && c.Id != 0))
            //{
            //    c.Opinions.Add(country.Id, 0);
            //}
            Debug.Log($"Kraj ID: {country.Id}, Nazwa: {country.Name}");
        }
    }

    private void InitializeProvinces()
    {
        foreach (var p in map.Provinces)
        {
            if (p.Type == "land")
            {
                if (p.Owner_id == 0) p.addStatus(new Tribal(-1));
                p.calcStatuses();
                map.calcRecruitablePop(p.coordinates);
            }
        }
    }

    private void SetCountryAsPlayer(Transform nameTransform, int playerNumber)
    {
        TMP_Text countryNameText = nameTransform.GetComponentInChildren<TMP_Text>();
        if (countryNameText != null)
        {
            countryNameText.text = $"PLAYER";
        }
    }

    private void SetCountryAsAI(Transform nameTransform)
    {
        TMP_Text countryNameText = nameTransform.GetComponentInChildren<TMP_Text>();
        if (countryNameText != null)
        {
            countryNameText.text = "AI";
        }

    }

    private void OnCountryClicked(GameObject countryUI, Transform nameTransform, int countryId)
    {
        if (countryId < 0 || countryId >= controllers.Count)
        {
            Debug.LogError($"Błędny countryId: {countryId}. Musi być pomiędzy 0 a {controllers.Count - 1}.");
            return;
        }

        if (!countryPlayerAssignment.ContainsKey(countryId))
        {
            currentMaxPlayerNumber++;
            countryPlayerAssignment[countryId] = currentMaxPlayerNumber;

            SetCountryAsPlayer(nameTransform, currentMaxPlayerNumber);
            controllers[countryId] = CountryController.Local;

            Debug.Log($"Kraj {countryId} został ustawiony jako Gracz {currentMaxPlayerNumber}.");
		}
        else
        {
            int playerNumber = countryPlayerAssignment[countryId];
            countryPlayerAssignment.Remove(countryId);

            SetCountryAsAI(nameTransform);
            controllers[countryId] = CountryController.Ai;

            Debug.Log($"Kraj {countryId} został ustawiony jako AI.");

            if (playerNumber == currentMaxPlayerNumber)
            {
                currentMaxPlayerNumber--;
            }
        }
        showButton();
    }

    public void showButton()
    {
		Button button =  optionsTable.transform.Find("startgame").GetComponent<Button>();
        if (controllers.Contains(CountryController.Local))
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
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
    public static Resource ParseResource(string resource)
    {
        return resource.ToLower() switch
        {
            "iron" => Resource.Iron,
            "wood" => Resource.Wood,
            "gold" => Resource.Gold,
            _ => Resource.AP,
        };
    }
    private void createTestCountrys()
    {
        map.addCountry(new Country(9, "Temeria", (6, 6), Color.cyan, 1, map), CountryController.Local);
		map.addCountry(new Country(10, "Kaedwen", (9, 9), Color.green, 2, map), CountryController.Local);
        map.assignProvince(map.getProvince(7, 7), 9);
		map.assignProvince(map.getProvince(7, 6), 9);
        map.assignProvince(map.getProvince(6,6), 9);
		map.assignProvince(map.getProvince(8, 7), 9);
		map.assignProvince(map.getProvince(9, 8), 9);

		map.assignProvince(map.getProvince(9,9),10);
		map.assignProvince(map.getProvince(10, 9), 10);
		map.assignProvince(map.getProvince(8, 9), 10);
		map.assignProvince(map.getProvince(7, 9), 10);
		map.assignProvince(map.getProvince(8, 8), 10);
		map.assignProvince(map.getProvince(7, 8), 10);
		map.assignProvince(map.getProvince(6, 7), 10);
	}
}