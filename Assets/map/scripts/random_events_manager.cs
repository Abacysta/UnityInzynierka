using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.map.scripts {
    public class random_events_manager : MonoBehaviour{
        [SerializeField] private Map map;
        private System.Random random;
        private void Start() {
            random = new System.Random();
        }
        public int chance { get => random.Next(100); }

        public void getRandomEvent(Country country) {
            var province = random.Next(country.Provinces.Count);
            
        }
    }
}
