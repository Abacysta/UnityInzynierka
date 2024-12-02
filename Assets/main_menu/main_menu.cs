using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour
{
    public settings_menu settings;

    void Start()
    {
        settings.gameObject.SetActive(false);
        settings.settingsInit();
    }

    public void StartNewGame() {
        SceneManager.LoadScene(1);
    }

    public void LoadGame() {
        SceneManager.LoadScene(2);
    }

    public void ExitGame() {
        Debug.Log("manual_quit");
        Application.Quit();
    }
}
