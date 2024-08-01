using UnityEngine;

public class game_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private int RecruitablePopulationFactor = 5;

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
        if (map.Provinces == null)
        {
            Debug.LogError("Provinces data is null!");
            return;
        }

        foreach (Province province in map.Provinces)
        {
            if (province != null)
            {
                Debug.Log($"Processing Province: {province.Name}, Population: {province.Population}, Type: {province.Type}, Can Recruit: {province.Is_possible_to_recruit}");
                
                if (province.Type == "land" && province.Is_possible_to_recruit)
                {
                    province.RecruitablePopulation = province.Population / RecruitablePopulationFactor;
                }
                else
                {
                    province.RecruitablePopulation = 0;
                }

                Debug.Log($"Recruitable Population for {province.Name}: {province.RecruitablePopulation}");
            }
            else
            {
                Debug.LogWarning("Encountered a null province in the list!");
            }
        }
    }
}
