using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class sound_manager : MonoBehaviour
{
    public static sound_manager instance { get; private set; }
    /// <summary>
    /// game is id 2 so the rest is a part of main menu
    /// </summary>
    private bool main;
    private short musId = 0;
    [SerializeField] private AudioClip button_sound;
    [SerializeField] private AudioClip switch_sound;
    [SerializeField] private AudioClip army_sound;
    [SerializeField] private List<AudioClip> menu_mus;
    [SerializeField] private List<AudioClip> game_mus;
    [SerializeField] private AudioSource speaker;
    [SerializeField] private AudioSource sfx;
    private void Awake() {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
    }

    private void Start() {
        main = SceneManager.GetActiveScene().buildIndex != 2;
        if(menu_mus != null && game_mus != null && menu_mus.Count > 0 && game_mus.Count > 0) {
            StartCoroutine(startPlaylist());
        }
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    //when scene changes(or is initially loaded) randomly selects first song in a playlist
    private void OnSceneLoaded(Scene scene, LoadSceneMode load) {
        bool ismenu = scene.buildIndex != 2;
        if(main != ismenu) {
            main = ismenu;
            musId=(short)Random.Range(0, main ? menu_mus.Count : game_mus.Count);
            if(speaker.isPlaying) speaker.Stop();
        }
        switch(scene.buildIndex) {
            case 0: {
                GameObject parent = GameObject.Find("main_menu/buttons");
                if(parent != null) {
                    UnityEngine.UI.Button[] arr = parent.GetComponentsInChildren<UnityEngine.UI.Button>();
                    foreach(UnityEngine.UI.Button b in arr) {
                        b.onClick.AddListener(playButton);
                    }
                }
                break;
            }
            case 1: {
                GameObject.Find("game_options/startgame").GetComponent<Button>().onClick.AddListener(playButton);
                break;
            }
            case 2: {
                GameObject ui = GameObject.Find("ui");
                if(ui != null) {
                    ui.transform.Find("save_button").GetComponent<Button>().onClick.AddListener(playButton);
                    ui.transform.Find("settings_button").GetComponent<Button>().onClick.AddListener(playButton);
                    var modes = ui.transform.Find("mapmodes").GetComponentsInChildren<Button>();
                    foreach(var mode in modes) {
                        mode.onClick.AddListener(playSwitch);
                    }
                    var upperB = ui.transform.Find("country_tab").GetComponentsInChildren<Button>();
                    foreach(var b in upperB) { 
                        b.onClick.AddListener(sound_manager.instance.playButton);
                    }
                    var cInterface = ui.transform.Find("country_interface");
                    cInterface.transform.Find("title_bar").GetComponentInChildren<Button>().onClick.AddListener(playButton);
                    var taxB = cInterface.transform.Find("tab_panels/production_panel/tax_area").GetComponentsInChildren<Toggle>();
                    foreach(var t in taxB) { 
                        t.onValueChanged.AddListener(sound_manager.instance.playSwitch);
                    }
                }
                break;
            }
        }
    }
    public void playButton() {
        sfx.Stop();
        sfx.clip = button_sound;
        sfx.Play();
    }
    public void playSwitch() {
        sfx.Stop();
        sfx.clip = switch_sound; sfx.Play();
    }
    public void playSwitch(bool isOn) {
        playSwitch();
    }
    public void playArmy() {
        sfx.Stop();
        sfx.clip = army_sound; sfx.Play();
    }
    //plays songs in order
    private IEnumerator startPlaylist() {
        while(true) {
            if(!speaker.isPlaying) {
                speaker.clip = main ? menu_mus[musId] : game_mus[musId];
                speaker.Play();
                musId = (short)((musId + 1) % (main ? menu_mus.Count : game_mus.Count));
            }
            yield return new WaitForSeconds(1);
        }
    }
}
