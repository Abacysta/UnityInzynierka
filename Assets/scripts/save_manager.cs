using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class save_manager : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private game_manager game_manager;

    private Save toSave;

    internal Save ToSave { get => toSave; set => toSave = value; }

    public void SaveGame(string name)
    {
        var path = Application.persistentDataPath + "/" + name + ".save";
        BinaryFormatter form = new BinaryFormatter();
        using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            form.Serialize(stream, new Save(map));
        }
        Debug.Log($"Game saved to: {path}");
    }
    //deprecated, only for TESTING
    public void SaveGameJson()
    {
        var path = Application.persistentDataPath + "/save.json";
        string jsonData = JsonConvert.SerializeObject(toSave, Formatting.Indented); // 'Indented' for pretty-printing

        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.Write(jsonData);
        }

        Debug.Log($"Game saved to: {path}");
    }

    public void LoadGame(string name)
    {
        var path = Application.persistentDataPath + "/" + name + ".save";
        Debug.Log("loading " + path);
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Save data = (Save)binaryFormatter.Deserialize(stream);
                game_manager.LoadGameFromSave(data);
            }
        }
        else
        {
            Debug.LogError("Save file not found or loading failed.");
        }
    }
    //deprecated, only for TESTING
    public void LoadGameJson(string name)
    {
        var path = Application.persistentDataPath + "/" + name + ".json"; // Save file with .json extension
        Debug.Log("loading " + path);

        if (File.Exists(path))
        {  // Check if the file exists
            using (StreamReader reader = new StreamReader(path))
            {
                string jsonData = reader.ReadToEnd();  // Read all text from the file

                // Deserialize the JSON string into a Save object
                Save data = JsonConvert.DeserializeObject<Save>(jsonData);
                game_manager.LoadGameFromSave(data);
            }
        }
        else
        {
            Debug.LogError("Save file not found or loading failed.");
        }
    }

    public string[] GetSaveGames()
    {
        var saves = new List<string>();
        if (Directory.Exists(Application.persistentDataPath))
        {
            return Directory.GetFiles(Application.persistentDataPath, "*.save").Select(s => s.Substring(Application.persistentDataPath.Length + 1).Replace(".save", "")).ToArray();
        }
        else
        {
            Debug.Log("Data directory is broken");
            return null;
        }
    }

    public bool ExistsSaveGame(string name)
    {
        var path = Path.Combine(Application.persistentDataPath, name + ".save");
        return File.Exists(path);
    }

    public bool DeleteSaveGame(string name)
    {
        var path = Path.Combine(Application.persistentDataPath, name + ".save");
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            else return false;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }
}
