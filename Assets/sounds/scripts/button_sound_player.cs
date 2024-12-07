using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class button_sound_player : MonoBehaviour
{
    [SerializeField] private AudioClip buttonSound;

    private AudioSource audioSource;
    private HashSet<Button> registeredButtons = new();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = buttonSound;

        CheckAndAddListenersToButtons();
        InvokeRepeating("CheckAndAddListenersToButtons", 1f, 1f);
    }

    void CheckAndAddListenersToButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>();

        foreach (Button button in buttons)
        {
            if (!registeredButtons.Contains(button))
            {
                button.onClick.AddListener(OnButtonClick);
                registeredButtons.Add(button);
            }
        }
    }

    void OnButtonClick()
    {
        audioSource.Play();
    }
}
