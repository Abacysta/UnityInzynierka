using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class sound_manager : MonoBehaviour
{
    public static sound_manager instance { get; private set; }
    /// <summary>
    /// game is id 2 so the rest is a part of main menu
    /// </summary>
    private bool main;
    private short musId = 0;

    [SerializeField] private List<AudioClip> menu_mus;
    [SerializeField] private List<AudioClip> game_mus;
    [SerializeField] private AudioSource speaker;
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
