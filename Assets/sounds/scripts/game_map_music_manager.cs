using UnityEngine;
using System.Collections.Generic;

public class game_map_music : MonoBehaviour
{
    [SerializeField] private AudioClip[] tracks;

    private AudioSource audioSource;
    private List<AudioClip> remainingTracks;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (tracks.Length > 0)
        {
            remainingTracks = new List<AudioClip>(tracks);

            PlayNextTrack();
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        if (remainingTracks.Count == 0)
        {
            remainingTracks = new List<AudioClip>(tracks);
        }

        int randomIndex = Random.Range(0, remainingTracks.Count);
        AudioClip nextTrack = remainingTracks[randomIndex];

        audioSource.clip = nextTrack;
        audioSource.Play();

        remainingTracks.RemoveAt(randomIndex);
    }
}
