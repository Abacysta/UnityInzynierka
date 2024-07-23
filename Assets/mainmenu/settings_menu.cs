using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class settings_menu : MonoBehaviour
{

    public AudioMixer mixer;

    public TMP_Dropdown ResList;

    Resolution[] res;

    void Start() {
        res = Screen.resolutions;
        ResList.ClearOptions();
        List<string> oList = new List<string>();
        int idx=0;
        for(int i = 0; i < res.Length; i++) {
            oList.Add(res[i].width + "x" + res[i].height);
            if(res[i].width==Screen.currentResolution.width && res[i].height==Screen.currentResolution.height) idx= i;
        }
        ResList.AddOptions(oList);
        ResList.value = idx;
        ResList.RefreshShownValue();

    }

    public void SetRes(int idx) {
        Resolution r = res[idx];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    public void setSFX(float vol) {
        mixer.SetFloat("sfx_vol", vol);
    }

    public void setMus(float vol) {
        mixer.SetFloat("mus_vol", vol);
    }
}
