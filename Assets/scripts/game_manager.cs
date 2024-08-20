using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class game_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private int turnCnt = 0;
    [SerializeField] TMP_Text turnCntTxt;
    [SerializeField] private AudioSource turn_sound;
    [SerializeField] private float RecruitablePopulationFactor = 0.2f;
    [SerializeField] private float PopulationFactor = 0.1f;
    [SerializeField] private int HappinessFactor = 5;
    [SerializeField] private float ArmyFactor = 0.1f;
    [SerializeField] private fog_of_war fog_Of_War;

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
    {
        StartCoroutine(TurnSimulationCoroutine());
    }

    private IEnumerator TurnSimulationCoroutine()
    {//id 0 is a dummy

        turn_sound.Play();

        foreach(var p in map.Provinces) {
            map.growPop(p.coordinates);
            map.calcRecruitablePop(p.coordinates);
            map.calcPopExtremes();
            p.calcStatuses();
        }

        for(int i = 1; i < map.Countries.Count; i++) {
            Country country = map.Countries[i];
            float tax = 0;
            Dictionary<Resource, float> resources = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.Wood, 0 },
                { Resource.Iron, 0 },
                { Resource.SciencePoint, 0 },
                { Resource.AP, 0 }
            };

            foreach(var p in map.Countries[i].Provinces) {
                var province = map.getProvince(p);
                tax += province.Population / 100 * 1f * province.Tax_mod;//0.1f = temp 100% tax rate
                resources[province.ResourcesT] += province.ResourcesP;
                resources[Resource.AP] += 0.1f;
            }

            tax *= country.techStats.taxFactor;
            resources[Resource.Gold] *= country.techStats.prodFactor;
            resources[Resource.Wood] *= country.techStats.prodFactor;
            resources[Resource.Iron] *= country.techStats.prodFactor;
            country.modifyResource(Resource.Gold, tax);

            foreach(var res in resources) {
                country.modifyResource(res.Key, res.Value);
            }

            country.setResource(Resource.AP, resources[Resource.AP]);
        }

        yield return new WaitForSeconds(2f);
        map.moveArmies();
        yield return new WaitForSeconds(2f);

        foreach(var c in map.Countries) { 
            map.mergeArmies(c);
        }

        turnCntTxt.SetText("" + ++turnCnt);
        fog_Of_War.StartTurn();
    }
}
