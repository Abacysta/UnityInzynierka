using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class country_info : MonoBehaviour
{
    public TMP_Text txt;
    public Map map;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("displayInfo", 0.5f, 0.15f);
    }

    public void displayInfo() {
        txt.SetText(
            "name:" + map.Countries[1].Name + '\n' +
            "wood: " + map.Countries[1].Resources[Resource.Wood] + '\n' +
            "AP: " + map.Countries[1].Resources[Resource.AP]
            );
    }
}
