using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class video_end : MonoBehaviour
{
    public VideoPlayer player;
    
    // Start is called before the first frame update
    

    void Start()
    {
        player.loopPointReached += vidEnd;
    }

    void vidEnd(UnityEngine.Video.VideoPlayer player) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
