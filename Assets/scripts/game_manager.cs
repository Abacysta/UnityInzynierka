using UnityEngine;

public class game_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private float RecruitablePopulationFactor = 0.2f;
    [SerializeField] private float PopulationFactor = 0.1f;
    [SerializeField] private int HappinessFactor = 5;

    // Loading map data before all scripts
    void Awake()
    {
        LoadData();
    }

    void LoadData()
    {
        map.Map_name = "map1";
        map.File_name = "map_prototype_3";
        TextAsset jsonFile = Resources.Load<TextAsset>(map.File_name);

        if (jsonFile != null)
        {
            string jsonContent = "{\"provinces\":" + jsonFile.text + "}";
            JsonUtility.FromJsonOverwrite(jsonContent, map);
        }
        else
        {
            Debug.LogError("JSON map file not found in Resources!");
        }
    }

    public void CalculateRecruitablePopulation()
    {
        foreach (Province province in map.Provinces)
        {
            if (province != null)
            {
                if (province.Type == "land" && province.Is_possible_to_recruit)
                {
                    province.RecruitablePopulation = (int)(province.Population * RecruitablePopulationFactor);
                }
                else
                {
                    province.RecruitablePopulation = 0;
                }
            }
        }
    }
    private void TurnIncreasePopulation()
    {
        foreach (Province province in map.Provinces)
        {
            if (province != null)
            {
                if (province.Type == "land")
                {
                    province.Population = province.Population + (int)(province.Population * PopulationFactor);
                }
            }
        }
    }
    private void TurnDecreaseOccupation()
    {
        foreach (Province province in map.Provinces)
        {
            if (province != null)
            {
                if (province.Occupation)
                {
                    province.Occupation_count -= 1;
                    if(province.Occupation_count == 0)
                    {
                        province.Occupation = false;
                    }
                }
            }
        }
    }
    private void TurnHappinessIncrease()
    {
        foreach (Province province in map.Provinces)
        {
            if (province != null)
            {
                if (province.Happiness < 100)
                {
                    province.Happiness += HappinessFactor;
                }
                else if (province.Happiness > province.Happiness - HappinessFactor && province.Happiness <= 100)
                {
                    province.Happiness = 100;
                }
            }
        }
    }
    public void TurnSimulation()
    {
        TurnDecreaseOccupation();
        TurnHappinessIncrease();
        TurnIncreasePopulation();
        CalculateRecruitablePopulation();
    }
}
