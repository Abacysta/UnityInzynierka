using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class player_table : MonoBehaviour
{
    [SerializeField] private GameObject dummy;
    [SerializeField] private GameObject playerTable;
    private List<Map.CountryController> controllers = new List<Map.CountryController>();
    private List<Country> countries = new List<Country>();

    public List<Map.CountryController> Controllers { get => controllers; set => controllers = value; }
    public List<Country> Countries { get => countries; set => countries = value; }

    private GameObject currentPlayerSelection = null;

    public class Country
    {
        public int owner_id;
        public string nazwa;
        public int[] color;
        public int herb;
        public int[] capitol;
    }

    public class GameState
    {
        public List<Country> states;
    }

    void Start()
    {
        string json = LoadJsonFromFile("Assets/Resources/map_prototype_4.json");
        GameState gameState = JsonConvert.DeserializeObject<GameState>(json);

        showCountries(gameState.states);
    }

    private string LoadJsonFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError("Plik nie zosta³ znaleziony: " + filePath);
            return null;
        }
    }
    private void SetCountryAsPlayer(Transform nameTransform)
    {
        TMP_Text countryNameText = nameTransform.GetComponentInChildren<TMP_Text>();
        if (countryNameText != null)
        {
            countryNameText.text = "GRACZ";
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
    private void OnCountryClicked(GameObject countryUI, Transform nameTransform)
    {
        if (currentPlayerSelection == null)
        {
            SetCountryAsPlayer(nameTransform);
            currentPlayerSelection = countryUI;
        }
        else if (currentPlayerSelection == countryUI)
        {
            // odklikniecie 
            SetCountryAsAI(nameTransform);
            currentPlayerSelection = null;
        }
        else
        {
            // Zamien biezacego
            SetCountryAsAI(currentPlayerSelection.transform.Find("controller"));
            SetCountryAsPlayer(nameTransform);
            currentPlayerSelection = countryUI;
        }
    }

    public void showCountries(List<Country> countries)
    {
        float yOffset = 0f;  

        foreach (Country country in countries)
        {
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
                    countryNameText.text = country.nazwa;
                }
            }

            Image nameBackgroundImage = nameTransform?.GetComponent<Image>();
            if (nameBackgroundImage != null)
            {
                nameBackgroundImage.color = new Color(
                    country.color[0] / 255f,
                    country.color[1] / 255f,
                    country.color[2] / 255f,
                    70f / 255f 
                );
            }

            Transform emblemTransform = countryUI.transform.Find("emblem");
            if (emblemTransform != null)
            {
                Image emblemImage = emblemTransform.GetComponent<Image>();
                if (emblemImage != null)
                {

                    string emblemPath = "sprites/coat_" + country.herb; 
                    Sprite emblemSprite = Resources.Load<Sprite>(emblemPath);
                    if (emblemSprite != null)
                    {
                        emblemImage.sprite = emblemSprite;
                        emblemImage.color = new Color(
                            country.color[0] / 255f,
                            country.color[1] / 255f,
                            country.color[2] / 255f
                        );
                    }
                    else
                    {
                        Debug.LogError("Nie znaleziono sprite'a pod œcie¿k¹: " + emblemPath);
                    }
                }
            }


            Transform controllerTransform = countryUI.transform.Find("controller");
            if (controllerTransform != null)
            {
                Button countryButton = controllerTransform.GetComponent<Button>();
                if (countryButton != null)
                {
                    countryButton.onClick.AddListener(() => OnCountryClicked(countryUI, controllerTransform));
                    Debug.Log("Przypisane");
                }
            }
        }
    }
}
