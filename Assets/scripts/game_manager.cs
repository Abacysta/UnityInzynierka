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

    
    public void TurnSimulation()
    {//id 0 is a dummy
        for(int i = 1; i < map.Countries.Count; i++) { 
            float APsum = 0;

            foreach(var p in map.Countries[i].Provinces) {
                if(map.getProvince(p).ResourcesT != Resource.AP){ 
                    map.Countries[i].modifyResource(map.calcResources(p, i, 1));
                }
                else {
                    APsum+= map.getProvince(p).Resources_amount;
                }

            }
            map.Countries[i].setResource((Resource.AP, APsum));
        }

        foreach(var p in map.Provinces) {
            map.growPop(p.coordinates, 0.05f);
            map.calcRecruitablePop(p.coordinates, PopulationFactor);
            map.calcPopExtremes();
        }
    }
}
