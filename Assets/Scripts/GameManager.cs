using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Map map;

    // Loading map data before all scripts
    void Awake()
    {
        Load_data();
    }

    void Load_data()
    {
        map.Map_name = "map1";
        map.File_name = "map_prototype";
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

    /*void Save_data()
    {

    }
    */
}

/*
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Map map;

    public Map Map { get => map; set => map = value; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
*/