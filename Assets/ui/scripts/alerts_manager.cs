using Assets.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    internal class alerts_manager : MonoBehaviour{
        private Country curr;
        [SerializeField] private GameObject dummy;
        [SerializeField] private GameObject counter;
        public List<Event_> sortedevents = new List<Event_>();
        internal class eventComparer:IComparer<Event_> {
            public int Compare(Event_ a, Event_ b) {
                int aprio = prio(a);
                int bprio = prio(b);
                return aprio.CompareTo(bprio);
            }
            private int prio(Event_ e) {
                if (e is Event_.DiploEvent)
                    return 1;
                if(e is Event_.GlobalEvent)
                    return 2;
                if(e is Event_.LocalEvent)
                    return 3;
                return 4;
            }
        }

        public void loadEvents(Country country) {
            curr = country;
            sortedevents = new List<Event_>(country.Events);
            sortedevents.Sort(new eventComparer());
            reloadAlerts();
        }

        public void reloadAlerts() {
            foreach(Transform child in transform) {
                if (child.name != "dummy" && child.name != "counter")
                {
                    Destroy(child.gameObject);
                }
            }
            if (sortedevents != null) {
                dummy.SetActive(true);
                Vector3 pos = dummy.transform.position;
                var i = 0;
                foreach (Event_ e in sortedevents) {
                    if (i > 10) {
                        break;
                    }   
                    Vector3 newPos = pos + new Vector3(dummy.GetComponent<RectTransform>().sizeDelta.x * ++i, 0, 0);
                    GameObject alert = Instantiate(dummy, newPos, Quaternion.identity);
                    alert.transform.SetParent(dummy.transform.parent);
                    alert.name = "alert" + i;
                    alert.GetComponentInChildren<Button>().onClick.AddListener(() => e.call());
                    setAlertView(alert, e);
                    setSprite(alert, e);

                    help_tooltip_trigger trigger = alert.AddComponent<help_tooltip_trigger>();
                    trigger.TooltipText = "Event";

                    alert.SetActive(true);
                }
                if (sortedevents.Count > 10) {
                    displayCounter(sortedevents.Count - 11, dummy.GetComponent<RectTransform>().sizeDelta.x * (i + 1));
                }
                else {
                    counter.SetActive(false);
                }
                dummy.SetActive(false);
            }
        }

        private void displayCounter(int additional, float coordinate) {
            TMP_Text txt = counter.GetComponentInChildren<TMP_Text>();
            txt.text = "+" + additional;
            counter.transform.position = new Vector3(coordinate, 0, 0) + dummy.transform.position;
            counter.SetActive(true);
        }

        private void setAlertView(GameObject alert, Event_ event_) {
            var img = alert.transform.Find("Button").GetComponent<Image>();
            Debug.Log(img != null ? "foundalert" : "notfoundalert");
            if (img != null) {
                Color org = img.color;
                float h, s, v;
                Color.RGBToHSV(org, out h, out s, out v);
                if (event_ is Event_.DiploEvent)
                    h = 0.5f;
                if (event_ is Event_.GlobalEvent)
                    h = 0.8f;
                if (event_ is Event_.LocalEvent)
                    h = 0.2f;
                Color neww = Color.HSVToRGB(h, 1f, 1f);
                img.color = neww;
            }
        }

        private void setSprite(GameObject alert, Event_ event_) {
            var sprite = alert.transform.Find("Button").Find("img").GetComponent<Image>();
            if (sprite != null) {
                Sprite set;
                if (event_ is Event_.DiploEvent.WarDeclared || event_ is Event_.DiploEvent.VassalRebel
                    || event_ is Event_.GlobalEvent.Plague || event_ is Event_.LocalEvent.PlagueFound)
                    set = Resources.Load<Sprite>("sprites/alerts/danger");
                else if (event_ is Event_.DiploEvent)
                    set = Resources.Load<Sprite>("sprites/alerts/diplo");
                else if (event_ is Event_.GlobalEvent.Happiness || event_ is Event_.GlobalEvent.TechnologicalBreakthrough)
                    set = Resources.Load<Sprite>("sprites/alerts/positive");
                else set = null;
                if(set != null)
                    sprite.sprite = set;
            }
        }
    }
}
