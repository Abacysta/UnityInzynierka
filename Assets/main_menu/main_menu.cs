using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour
{
    [SerializeField] private settings_menu settings;

    void Start()
    {
        settings.gameObject.SetActive(false);
        settings.settingsInit();
    }

    public void StartNewGame() {
        SceneManager.LoadScene(1);
    }

    public void ExitGame() {
        Debug.Log("manual_quit");
        Application.Quit();
    }
}
