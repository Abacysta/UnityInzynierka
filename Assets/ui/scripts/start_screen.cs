using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.ui.scripts {
    internal class start_screen : MonoBehaviour {
        [SerializeField] private alerts_manager alerts;
        [SerializeField] private GameObject[] toToggle;
        [SerializeField] private Map map;

        public void WelcomeScreen() {
            GameObject window = transform.GetChild(0).gameObject;

            foreach(var obj in toToggle) {
                obj.SetActive(false);
            }

            window.SetActive(true);
            window.transform.Find("window").GetComponentInChildren<TMP_Text>().text = 
                "You're playing as " + map.CurrentPlayer.Name;

            var button = window.transform.Find("window").GetComponentInChildren<Button>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(sound_manager.instance.playButton);
            button.onClick.AddListener(() => alerts.LoadEvents(map.CurrentPlayer));
            button.onClick.AddListener(() => alerts.ReloadAlerts());
            button.onClick.AddListener(() => window.SetActive(false));
            button.onClick.AddListener(() => UnHide());
        }

        public void UnHide() {
            foreach(var obj in toToggle) {
                obj.SetActive(true);
            }
        }
    }
}
