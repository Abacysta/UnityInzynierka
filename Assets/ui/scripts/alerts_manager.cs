using Assets.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts {
    internal class alerts_manager : MonoBehaviour{
        private Country curr;
        [SerializeField] private GameObject dummy;
        public List<Event_> sortedevents;
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
                if (child.name != "dummy")
                {
                    Destroy(child.gameObject);
                }
            }
            if (sortedevents != null) {
                dummy.SetActive(true);
                Vector3 pos = dummy.transform.position;
                var i = 0;
                foreach (Event_ e in sortedevents) {
                    Vector3 newPos = pos + new Vector3(dummy.GetComponent<RectTransform>().sizeDelta.x * ++i, 0, 0);
                    GameObject alert = Instantiate(dummy, newPos, Quaternion.identity);
                    alert.transform.SetParent(dummy.transform.parent);
                    alert.name = "alert" + i;
                    alert.GetComponentInChildren<Button>().onClick.AddListener(() => e.call());
                    alert.SetActive(true);
                }
                dummy.SetActive(false);
            }
        }
    }
}
