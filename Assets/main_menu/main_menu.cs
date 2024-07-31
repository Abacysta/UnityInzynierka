using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour
{
    public settings_menu settings;

    void Start()
    {
        settings.settingsInit();
        Screen.fullScreen = false;
    }

    void Update()
    {
        
    }

    public void StartTestGame() {
        SceneManager.LoadScene(1);
    }

    public void StartOnlineGame() {
        SceneManager.LoadScene(2);
    }

    public void theEnd() {
        Debug.Log("manual_quit");
        Application.Quit();
    }
}
