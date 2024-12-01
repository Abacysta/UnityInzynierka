using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class settings_menu : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private TMP_Dropdown res_dropdown;
    [SerializeField] private Slider sliderSFX, sliderMus;
    [SerializeField] private GameObject overlay;
    [SerializeField] private Toggle full_screen_toggle;

    private readonly List<Resolution> validResolutions = new();
    private bool isFullScreen = true;

    public void settingsInit() {

        SetInitalVideoSettings();
        SetInitialAudioSettings();
    }

    public void SetInitalVideoSettings()
    {
        if (PlayerPrefs.HasKey("scHeight") && PlayerPrefs.HasKey("scWidth")
            && PlayerPrefs.HasKey("isFullScreen")) {
            isFullScreen = PlayerPrefs.GetInt("isFullScreen") == 1;
            Resolution r = new() {
                width = PlayerPrefs.GetInt("scWidth"),
                height = PlayerPrefs.GetInt("scHeight")
            };
            SetRes(r);
        }
        else
        {
            FindValidResolutions();
            isFullScreen = true;
            SetRes(validResolutions.Count - 1);
        }
    }

    public void SetInitialAudioSettings()
    {
        if (!PlayerPrefs.HasKey("volSfx")) PlayerPrefs.SetFloat("volSfx", 75f);
        if (!PlayerPrefs.HasKey("volMus")) PlayerPrefs.SetFloat("volMus", 75f);

        float volSfx = PlayerPrefs.GetFloat("volSfx");
        float volMus = PlayerPrefs.GetFloat("volMus");

        setSFX(volSfx);
        setMus(volMus);

        sliderSFX.value = volSfx;
        sliderMus.value = volMus;
    }

    void Start() {
        if (!PlayerPrefs.HasKey("volSfx")) PlayerPrefs.SetFloat("volSfx", 75);
        if (!PlayerPrefs.HasKey("volMus")) PlayerPrefs.SetFloat("volMus", 75);

        if (!PlayerPrefs.HasKey("scHeight") || !PlayerPrefs.HasKey("scWidth") || !PlayerPrefs.HasKey("isFullScreen")) {
            PlayerPrefs.SetInt("scHeight", Screen.currentResolution.height);
            PlayerPrefs.SetInt("scWidth", Screen.currentResolution.width);
            PlayerPrefs.SetInt("isFullScreen", isFullScreen ? 1 : 0);
        }

        sliderMus.onValueChanged.AddListener(setMus);
        sliderSFX.onValueChanged.AddListener(setSFX);

        if (full_screen_toggle != null) {
            full_screen_toggle.isOn = isFullScreen;
            full_screen_toggle.onValueChanged.AddListener(ToggleFullScreen);
        }

        FindValidResolutions();
        PopulateDropdown();
    }

    void PopulateDropdown()
    {
        res_dropdown.ClearOptions();
        List<string> options = new();

        foreach (Resolution res in validResolutions) {
            options.Add($"{res.width} x {res.height}");
        }

        int screenHeight = PlayerPrefs.GetInt("scHeight");
        int screenWidth = PlayerPrefs.GetInt("scWidth");

        res_dropdown.AddOptions(options);
        int currentIndex = validResolutions.FindIndex(res =>
            res.width == screenWidth && res.height == screenHeight);
        res_dropdown.value = currentIndex != -1 ? currentIndex : validResolutions.Count - 1;
        res_dropdown.RefreshShownValue();

        res_dropdown.onValueChanged.AddListener(SetRes);
    }

    public void toggleMenu(GameObject menu) {
        if (overlay != null) overlay.SetActive(!overlay.activeSelf);
        menu.SetActive(!menu.activeSelf);
    }

    public void toTitle() {
        SceneManager.LoadScene(0);
    }

    public void SetRes(int idx)
    {
        Resolution r = validResolutions[idx];
        SetRes(r);
    }

    public void SetRes(Resolution r) {
        Screen.SetResolution(r.width, r.height, isFullScreen ? 
            FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed);
        PlayerPrefs.SetInt("scHeight", r.height);
        PlayerPrefs.SetInt("scWidth", r.width);
        PlayerPrefs.SetInt("isFullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void setSFX(float percent) {
        float vol = PercentToDecibels(percent);
        mixer.SetFloat("sfx_vol", vol);
        PlayerPrefs.SetFloat("volSfx", percent);
        PlayerPrefs.Save();
    }

    public void setMus(float percent) {
        float vol = PercentToDecibels(percent);
        mixer.SetFloat("mus_vol", vol);
        PlayerPrefs.SetFloat("volMus", percent);
        PlayerPrefs.Save();
    }

    public void ToggleFullScreen(bool x) {
        isFullScreen = x;
        SetRes(Screen.currentResolution);
    }

    private float PercentToDecibels(float percent)
    {
        return Mathf.Lerp(-80f, 0f, percent / 100f);
    }

    private void FindValidResolutions()
    {
        validResolutions.Clear();
        foreach (var res in Screen.resolutions)
        {
            if (Mathf.Approximately((float)res.width / res.height, 16f / 9f) &&
                !validResolutions.Exists(r => r.width == res.width && r.height == res.height))
            {
                validResolutions.Add(res);
            }
        }
        if (validResolutions.Count == 0) validResolutions.Add(Screen.currentResolution);
    }
}
