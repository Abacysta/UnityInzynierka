using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class settings_menu : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private TMP_Dropdown ResList;
    [SerializeField] private Slider sliderSFX, sliderMus;
    [SerializeField] private GameObject[] toBlock;
    [SerializeField] private GameObject overlay;

    Resolution[] res;

    private float volSfx;
    private float volMus;
    private float screenHeight;
    private float screenWidth;

    public void settingsInit() {
        if(!PlayerPrefs.HasKey("volSfx")) PlayerPrefs.SetFloat("volSfx", 0);
        if(!PlayerPrefs.HasKey("volMus")) PlayerPrefs.SetFloat("volMus", 0);

        volSfx = PlayerPrefs.GetFloat("volSfx");
        volMus = PlayerPrefs.GetFloat("volMus");

        if(!PlayerPrefs.HasKey("scHeight") || !PlayerPrefs.HasKey("scWidth")) {
            PlayerPrefs.SetInt("scHeight", Screen.currentResolution.height);
            PlayerPrefs.SetInt("scWidth", Screen.currentResolution.width);
        }

        screenHeight = PlayerPrefs.GetInt("scHeight");
        screenWidth = PlayerPrefs.GetInt("scWidth");

        setSFX(volSfx);
        setMus(volMus);

        sliderSFX.value = volSfx;
        sliderMus.value = volMus;
    }

    void Start() {
        if(!PlayerPrefs.HasKey("volSfx")) PlayerPrefs.SetFloat("volSfx", 0);
        if(!PlayerPrefs.HasKey("volMus")) PlayerPrefs.SetFloat("volMus", 0);

        volSfx = PlayerPrefs.GetFloat("volSfx");
        volMus = PlayerPrefs.GetFloat("volMus");

        if(!PlayerPrefs.HasKey("scHeight") || !PlayerPrefs.HasKey("scWidth")){
            PlayerPrefs.SetInt("scHeight", Screen.currentResolution.height);
            PlayerPrefs.SetInt("scWidth", Screen.currentResolution.width);
        }

        screenHeight = PlayerPrefs.GetInt("scHeight");
        screenWidth = PlayerPrefs.GetInt("scWidth");

        res = Screen.resolutions;
        ResList.ClearOptions();
        List<string> oList = new List<string>();
        int idx=0;

        for(int i = 0; i < res.Length; i++) {
            oList.Add(res[i].width + "x" + res[i].height);
            if(res[i].width==screenWidth && res[i].height==screenHeight) idx= i;
        }

        ResList.AddOptions(oList);
        ResList.value = idx;
        ResList.RefreshShownValue();
    }

    public void toggleMenu(GameObject menu) {
        /*
        if(toBlock != null) {
            foreach(var o in toBlock) {
                Button[] b = o.GetComponentsInChildren<Button>();

                foreach(var bb in b) {
                    bb.interactable = menu.activeSelf;
                }
            }
        }
        */
        if(overlay!=null) overlay.SetActive(!overlay.activeSelf);
        menu.SetActive(!menu.activeSelf);
    }

    public void toTitle() {
        SceneManager.LoadScene(0);
    }

    public void SetRes(int idx) {
        Resolution r = res[idx];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("scHeight", r.height);
        PlayerPrefs.SetInt("scWidth", r.width);
    }

    public void setSFX(float vol) {
        mixer.SetFloat("sfx_vol", vol);
        PlayerPrefs.SetFloat("volSfx", vol);
    }

    public void setMus(float vol) {
        mixer.SetFloat("mus_vol", vol);
        PlayerPrefs.SetFloat("volMus", vol);
    }
}
