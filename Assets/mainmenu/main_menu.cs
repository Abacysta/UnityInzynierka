using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour
{
    public settings_menu settings;
    // Start is called before the first frame update
    void Start()
    {
        settings.settingsInit();
        Screen.fullScreen = false;
    }

    // Update is called once per frame
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
