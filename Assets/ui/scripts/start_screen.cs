using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.ui.scripts {
    internal class start_screen:MonoBehaviour {
        [SerializeField] private alerts_manager alerts;
        [SerializeField] private GameObject[] toToggle;
        [SerializeField] private Map map;
        public void welcomeScreen() {
            GameObject window = transform.GetChild(0).gameObject;
            foreach(var obj in toToggle) {
                obj.SetActive(false);
            }
            window.SetActive(true);
            window.transform.Find("window").GetComponentInChildren<TMP_Text>().text = "You're playing as " + map.CurrentPlayer.Name;//map.CurrentPlayer.Name;
            var button = window.transform.Find("window").GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => alerts.loadEvents(map.CurrentPlayer));
            button.onClick.AddListener(() => alerts.reloadAlerts());
            button.onClick.AddListener(() => window.SetActive(false));
            button.onClick.AddListener(() => unHide());
        }

        public void unHide() {
            foreach(var obj in toToggle) {
                obj.SetActive(true);
            }
        }
    }
}
