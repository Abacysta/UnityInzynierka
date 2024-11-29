using Assets.classes.subclasses;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class game_data : MonoBehaviour
{
    public static game_data Instance { get; private set; }
    internal Save LoadedSave { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("main_menu");
    }
}
