using UnityEngine;
using UnityEngine.SceneManagement;

public class main_music_manager : MonoBehaviour
{
    private static main_music_manager instance;
    private AudioSource main_mus;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        main_mus = GetComponent<AudioSource>();
    }

    void Start()
    {
        main_mus.Play();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "game_map")
        {
            if (main_mus.isPlaying) main_mus.Stop();
            Destroy(gameObject);
        }
        else
        {
            if (!main_mus.isPlaying) main_mus.Play();
        }
    }
}
