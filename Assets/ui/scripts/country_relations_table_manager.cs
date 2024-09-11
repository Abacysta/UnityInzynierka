using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Assets.classes.Relation;

public class country_relations_table_manager : MonoBehaviour
{
    [SerializeField] private Map map;

    [SerializeField] private GameObject country_row;
    [SerializeField] private GameObject relation_type_img;
    [SerializeField] private Transform country_table_content;

    [SerializeField] private Button sort_by_country_name_button;
    [SerializeField] private Button sort_by_their_opinion_button;
    [SerializeField] private Button sort_by_our_opinion_button;

    [SerializeField] private Sprite war_sprite;
    [SerializeField] private Sprite alliance_sprite;
    [SerializeField] private Sprite truce_sprite;
    [SerializeField] private Sprite vassalage_sprite;
    [SerializeField] private Sprite subsidies_sprite;
    [SerializeField] private Sprite military_access_sprite;

    private List<Country> sortedCountries;
    private string currentSortCriteria = "country_name";
    private bool isAscending = true;

    private void InitializeTestData()
    {
        sortedCountries = new List<Country>
        {
            new Country(2, "Berbers", (2, 2), Color.cyan, map),
            new Country(3, "Egyptians", (3, 3), Color.blue, map),
            new Country(4, "Vikings", (4, 4), Color.green, map),
            new Country(5, "Huns", (5, 5), Color.yellow, map)
        };

        var opinions = new Dictionary<int, int>
        {
            { 1, 0 }, { 3, 2 }, { 4, -2 }, { 5, 1 }
        };
        sortedCountries[0].Opinions = new Dictionary<int, int>(opinions);

        opinions = new Dictionary<int, int>
        {
            { 1, 1 }, { 2, -3 }, { 4, 2 }, { 5, 0 } 
        };
        sortedCountries[1].Opinions = new Dictionary<int, int>(opinions);

        opinions = new Dictionary<int, int>
        {
            { 1, -1 }, { 2, 2 }, { 3, 0 }, { 5, 3 }
        };
        sortedCountries[2].Opinions = new Dictionary<int, int>(opinions);

        opinions = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, -1 }, { 3, 2 }, { 4, 3 }
        };
        sortedCountries[3].Opinions = new Dictionary<int, int>(opinions);
    }

    void Start()
    {
        sort_by_country_name_button.onClick.AddListener(() => ToggleSort("country_name"));
        sort_by_their_opinion_button.onClick.AddListener(() => ToggleSort("their_opinion"));
        sort_by_our_opinion_button.onClick.AddListener(() => ToggleSort("our_opinion"));

        //sortedCountries = new List<Country>(map.Countries);
        InitializeTestData();
        SortData("country_name");
    }

    void OnEnable()
    {
        //sortedCountries = new List<Country>(map.Countries);
        InitializeTestData();
        SortData(currentSortCriteria);
    }

    private void DisplayTable()
    {
        foreach (Transform child in country_table_content)
        {
            Destroy(child.gameObject);
        }

        foreach (var country in sortedCountries)
        {
            if (country.Id != map.currentPlayer && country.Id != 0)
            {
                GameObject rowObj = Instantiate(country_row, country_table_content);

                Image country_coat_of_arms = rowObj.transform.Find("country/coat_of_arms/cn_in_country_color_img").GetComponent<Image>();
                country_coat_of_arms.color = country.Color;

                rowObj.transform.Find("country/country_name_text").GetComponent<TMP_Text>().text = country.Name;

                TMP_Text theirOpinionText = rowObj.transform.Find("their_opinion_text").GetComponent<TMP_Text>();
                TMP_Text ourOpinionText = rowObj.transform.Find("our_opinion_text").GetComponent<TMP_Text>();

                int theirOpinion = country.Opinions.ContainsKey(map.currentPlayer) ? country.Opinions[map.currentPlayer] : 0;
                int ourOpinion = map.CurrentPlayer.Opinions.ContainsKey(country.Id) ? map.CurrentPlayer.Opinions[country.Id] : 0;

                SetOpinionText(theirOpinionText, theirOpinion);
                SetOpinionText(ourOpinionText, ourOpinion);

                /*
                Transform relationsContainer = rowObj.transform.Find("relations_container");

                List<Relation> relations = map.Relations.Where(r => r.Countries.Contains(country) && r.Countries.Contains(map.CurrentPlayer)).ToList();

                foreach (var relation in relations)
                {
                    Sprite relationSprite = GetResourceSprite(relation.Type);
                    if (relationSprite != null)
                    {
                        GameObject relationImageObj = Instantiate(relation_type_img, relationsContainer);
                        relationImageObj.GetComponent<Image>().sprite = relationSprite;
                    }
                }
                */
            }
        }
    }

    void SetOpinionText(TMP_Text textElement, int opinion)
    {
        textElement.color = opinion < 0 ? Color.red : Color.green;
        textElement.text = opinion == 0 ? "0" : (opinion > 0 ? "+" + opinion : opinion.ToString());
    }

    private void ToggleSort(string sortBy)
    {
        if (currentSortCriteria == sortBy)
        {
            isAscending = !isAscending;
        }
        else
        {
            currentSortCriteria = sortBy;
            isAscending = true;
        }

        SortData(currentSortCriteria);
    }

    private void SortData(string sortBy)
    {
        switch (sortBy)
        {
            case "country_name":
                sortedCountries = isAscending
                    ? sortedCountries.OrderBy(p => ExtractNumberFromName(p.Name)).ThenBy(p => p.Name).ToList()
                    : sortedCountries.OrderByDescending(p => ExtractNumberFromName(p.Name)).ThenByDescending(p => p.Name).ToList();
                break;
            case "their_opinion":
                sortedCountries = isAscending
                    ? sortedCountries.OrderBy(p => p.Opinions.ContainsKey(map.currentPlayer) ? p.Opinions[map.currentPlayer] : 0).ToList()
                    : sortedCountries.OrderByDescending(p => p.Opinions.ContainsKey(map.currentPlayer) ? p.Opinions[map.currentPlayer] : 0).ToList();
                break;
            case "our_opinion":
                sortedCountries = isAscending
                    ? sortedCountries.OrderBy(p => map.CurrentPlayer.Opinions.ContainsKey(p.Id) ? map.CurrentPlayer.Opinions[p.Id] : 0).ToList()
                    : sortedCountries.OrderByDescending(p => map.CurrentPlayer.Opinions.ContainsKey(p.Id) ? map.CurrentPlayer.Opinions[p.Id] : 0).ToList();
                break;
        }

        DisplayTable();
    }

    int ExtractNumberFromName(string name)
    {
        var number = new string(name.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());

        return int.TryParse(number, out int result) ? result : 0;
    }

    Sprite GetResourceSprite(RelationType relationType)
    {
        switch (relationType)
        {
            case RelationType.War:
                return war_sprite;
            case RelationType.Alliance:
                return alliance_sprite;
            case RelationType.Truce:
                return truce_sprite;
            case RelationType.Vassalage:
                return vassalage_sprite;
            case RelationType.Subsidies:
                return subsidies_sprite;
            case RelationType.MilitaryAccess:
                return military_access_sprite;
            default:
                return null;
        }
    }
}