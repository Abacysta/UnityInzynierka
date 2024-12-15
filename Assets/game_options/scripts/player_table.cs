using Assets.classes.subclasses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using static Assets.classes.subclasses.Constants.ProvinceConstants;
using UnityEngine;
using UnityEngine.UI;
using static Map;

public class player_table : MonoBehaviour
{
    [SerializeField] private GameObject dummy;
    [SerializeField] private GameObject playerTable;
    [SerializeField] private Map map;
    [SerializeField] private map_options optionsTable;
    [SerializeField] private map_preview map_Preview;
    private List<CountryController> controllers = new List<CountryController>();
    private List<CountryData> currentStates = new List<CountryData>();
    private List<Province> provinces = new List<Province>();

    private int currentMaxPlayerNumber = 0;
    private Dictionary<int, int> countryPlayerAssignment = new Dictionary<int, int>();

    public Map Map { get => map; set => map = value; }
    public List<CountryController> Controllers { get => controllers; set => controllers = value; }
    public GameObject PlayerTable { get => playerTable; set => playerTable = value; }
    public GameObject Dummy { get => dummy; set => dummy = value; }
    public map_options OptionsTable { get => optionsTable; set => optionsTable = value; }

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
        public List<CountryData> countries;
        public List<Province> provinces;
    }

    public void LoadMap(string mapName)
    {
        string json = LoadJsonFromFile(mapName);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning($"Nie udało się załadować mapy z pliku, próba załadowania z Resources: {mapName}");
            json = LoadJsonFromResources(mapName);
        }

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError($"Nie udało się załadować mapy: {mapName}");
            return;
        }
        GameState gameState;
        try
        {
            gameState = JsonConvert.DeserializeObject<GameState>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Błąd podczas deserializacji mapy: {e.Message}");
            return;
        }
        currentStates = gameState.countries;
        provinces = gameState.provinces;

        ShowCountries(currentStates);
        ShowButton();
        map_Preview.Provinces = provinces;
        map_Preview.Reload();
        controllers.Clear();
        controllers = Enumerable.Repeat(CountryController.Ai, currentStates.Count).ToList();
    }

    private string LoadJsonFromResources(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("Nazwa mapy jest pusta lub null.");
            return null;
        }

        TextAsset textAsset = Resources.Load<TextAsset>($"Maps/{mapName}");
        if (textAsset == null)
        {
            Debug.LogError($"Plik mapy nie został znaleziony w Resources: {mapName}");
            return null;
        }
        return textAsset.text;
    }


    private string LoadJsonFromFile(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("Nazwa mapy jest pusta lub null.");
            return null;
        }

        string path = Application.isEditor
            ? Path.Combine(Application.dataPath, "Resources/Maps", $"{mapName}.json")
            : Path.Combine(Application.dataPath, "../Maps", $"{mapName}.json");

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        else
        {
            Debug.LogError($"Plik mapy nie znaleziony: {path}");
            return null;
        }
    }



    private Color ToColor(int[] color)
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

        foreach (var provinceData in provinces)
        {
            Province.TerrainType terrain;
            if (provinceData.IsLand)
            {
                string terrainStr = provinceData.Terrain.ToString().ToLower();
                switch (terrainStr)
                {
                    case "forest":
                        terrain = Province.TerrainType.Forest;
                        break;
                    case "desert":
                        terrain = Province.TerrainType.Desert;
                        break;
                    case "lowlands":
                        terrain = Province.TerrainType.Lowlands;
                        break;
                    case "tundra":
                        terrain = Province.TerrainType.Tundra;
                        break;
                    default:
                        Debug.LogWarning($"Nieznany typ terenu: {terrainStr}, ustawiam tundra jako domyślny.");
                        terrain = Province.TerrainType.Tundra;
                        break;
                }
            }
            else
            {
                terrain = Province.TerrainType.Ocean;
            }
            Province newProvince = new Province(
                provinceData.Name,
                provinceData.X,
                provinceData.Y,
                provinceData.IsLand,
                terrain,
                ParseResource(provinceData.ResourceType.ToString().ToLower()),
                (int)provinceData.ResourceAmount,
                provinceData.Population,
                provinceData.Happiness,
                provinceData.IsCoast
            );
            newProvince.ResourceAmount = provinceData.ResourceAmount * map.ResourceRate/100;
            map.Provinces.Add(newProvince);
        }

        map.AddCountry(new Country(0, "", DEFAULT_CORD, new Color(0.8392f, 0.7216f, 0.4706f), 1, map), Map.CountryController.Ai);

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
                ToColor(state.color),
                state.coat,
                map
            );

            map.AddCountry(newCountry, CountryController.Ai);
            map.AssignProvince(newCountry.Capital, newCountry.Id);
            Debug.Log($"Dodano kraj: {newCountry.Name}, ID: {newCountry.Id}");
        }

        for (int i = 1; i <= controllers.Count; i++)
        {
            map.Controllers[i] = controllers[i - 1];
        }
        map.InitCountryOpinions();
		SetCurrentPlayer();
        SetCountryPriorities();
        InitializeProvinces();
        map.CalcPopulationExtremes();
        map.TurnCnt = 0;
        Debug.Log("Game setup complete. Ready to start the game. " + map.Countries.Count + " countries present.");
    }

    private void ClearMap()
    {
        map.Armies.Clear();
        map.ArmyViews.Clear();
        map.Countries.Clear();
        map.Controllers.Clear();
        map.Provinces.Clear();
        map.Controllers.Clear();
        map.Relations.Clear();
    }

    private void SetCurrentPlayer()
    {
        int playerIndex = map.Controllers.FindIndex(controller => controller == Map.CountryController.Local);
        if (playerIndex >= 0)
        {
            map.CurrentPlayerId = playerIndex;
            Debug.Log($"Gracz ustawiony na kraj: {map.CurrentPlayer.Name}");
        }
        else
        {
            Debug.LogError("Nie udało się znaleźć gracza (CountryController.Local)");
        }
    }

    private void SetCountryPriorities()
    {
        var countries = map.Countries.Where(c => c.Id != 0).ToList();

        System.Random random = new();

        var priorities = Enumerable.Range(0, countries.Count).ToList();
        priorities = priorities.OrderBy(p => random.Next()).ToList();

        for (int i = 0; i < countries.Count; i++)
        {
            countries[i].Priority = priorities[i];
            Debug.Log($"Country: {countries[i].Name}, Priority: {countries[i].Priority}");
        }
    }

    private void InitializeProvinces()
    {
        foreach (var p in map.Provinces)
        {
            if (p.IsLand)
            {
                if (p.OwnerId == 0) p.AddStatus(new Tribal(-1));
                p.CalcStatuses();
                p.CalcRecruitablePopulation(map);
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
        ShowButton();
    }

    public void ShowButton()
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

    public void ShowCountries(List<CountryData> states)
    {
        if (states == null || states.Count == 0)
        {
            Debug.LogError("Lista krajów (states) jest pusta lub null.");
            return;
        }
        foreach (Transform child in playerTable.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < states.Count; i++)
        {
            CountryData state = states[i];

            GameObject countryUI = Instantiate(dummy, playerTable.transform);
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
    private void CreateTestCountrys()
    {
        map.AddCountry(new Country(9, "Temeria", (6, 6), Color.cyan, 1, map), CountryController.Local);
		map.AddCountry(new Country(10, "Kaedwen", (9, 9), Color.green, 2, map), CountryController.Local);
        map.AssignProvince(map.GetProvince(7, 7), 9);
		map.AssignProvince(map.GetProvince(7, 6), 9);
        map.AssignProvince(map.GetProvince(6,6), 9);
		map.AssignProvince(map.GetProvince(8, 7), 9);
		map.AssignProvince(map.GetProvince(9, 8), 9);

		map.AssignProvince(map.GetProvince(9,9),10);
		map.AssignProvince(map.GetProvince(10, 9), 10);
		map.AssignProvince(map.GetProvince(8, 9), 10);
		map.AssignProvince(map.GetProvince(7, 9), 10);
		map.AssignProvince(map.GetProvince(8, 8), 10);
		map.AssignProvince(map.GetProvince(7, 8), 10);
		map.AssignProvince(map.GetProvince(6, 7), 10);
	}
}