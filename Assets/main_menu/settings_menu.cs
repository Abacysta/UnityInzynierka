using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class settings_menu : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private TMP_Dropdown res_dropdown;
    [SerializeField] private Slider sliderSFX, sliderMus;
    [SerializeField] private GameObject overlay;
    [SerializeField] private Toggle full_screen_toggle;

    private readonly List<Resolution> validResolutions = new();

    public void settingsInit() {

        SetInitialVideoSettings();
        SetInitialAudioSettings();
    }

    private void SetInitialVideoSettings()
    {
        bool isFullScreen = true;
        FindValidResolutions();

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
            SetRes(validResolutions.Count - 1);
        }

        SetFullScreenMode(isFullScreen);

        if (full_screen_toggle != null)
        {
            full_screen_toggle.isOn = isFullScreen;
            full_screen_toggle.onValueChanged.AddListener(ToggleFullScreen);
        }

        PopulateDropdown();
    }

    private void SetInitialAudioSettings()
    {
        if (!PlayerPrefs.HasKey("volSfx")) PlayerPrefs.SetFloat("volSfx", 75f);
        if (!PlayerPrefs.HasKey("volMus")) PlayerPrefs.SetFloat("volMus", 75f);

        float volSfx = PlayerPrefs.GetFloat("volSfx");
        float volMus = PlayerPrefs.GetFloat("volMus");

        setSFX(volSfx);
        setMus(volMus);

        sliderSFX.value = volSfx;
        sliderMus.value = volMus;

        sliderMus.onValueChanged.AddListener(setMus);
        sliderSFX.onValueChanged.AddListener(setSFX);
    }

    private void PopulateDropdown()
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
        if (idx < 0 || idx >= validResolutions.Count) return;

        Resolution r = validResolutions[idx];
        SetRes(r);
    }

    public void SetRes(Resolution r) {
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("scHeight", r.height);
        PlayerPrefs.SetInt("scWidth", r.width);
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

    public void ToggleFullScreen(bool isFullScreenMode) {
        SetFullScreenMode(isFullScreenMode);
    }

    private void SetFullScreenMode(bool isFullScreenMode)
    {
        if (Screen.fullScreenMode == (isFullScreenMode ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed)) {
            return;
        }

        Screen.fullScreenMode = isFullScreenMode
        ? FullScreenMode.ExclusiveFullScreen
        : FullScreenMode.Windowed;

        PlayerPrefs.SetInt("isFullScreen", isFullScreenMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    private float PercentToDecibels(float percent)
    {
        percent = Mathf.Clamp(percent, 0.0001f, 100f);
        float linearValue = Mathf.Pow(percent / 100f, 0.4f);
        float minDb = -80f;
        float maxDb = 0f;

        return Mathf.Lerp(minDb, maxDb, linearValue);
    }

    private void FindValidResolutions()
    {
        validResolutions.Clear();
        float aspectRatio = 16f / 9f;

        foreach (var res in Screen.resolutions)
        {
            float currentAspectRatio = (float)res.width / res.height;
            if (Mathf.Abs(currentAspectRatio - aspectRatio) < 0.01f &&
                !validResolutions.Exists(r => r.width == res.width && r.height == res.height))
            {
                validResolutions.Add(res);
            }
        }

        if (validResolutions.Count == 0) validResolutions.Add(Screen.currentResolution);
    }
}
