using Assets.classes;
using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.classes.actionContainer;

namespace Assets.map.scripts {
    public class random_events_manager : MonoBehaviour{
        [SerializeField] private Map map;
        [SerializeField] private dialog_box_manager dialog_box;
        [SerializeField] private camera_controller camera;
        private System.Random random;
        private void Start() {
            random = new System.Random();
        }
        public int chance { get => random.Next(100); }

        public void getRandomEvent(Country country) { 
            var pops = country.Provinces.Sum(p => p.Population);
            var c = chance;
            if(pops < 500 && c < 25) {//0-500
                getRandomLocalEvent(country);
            }
            else if(pops < 2000) {//500-2000
                if(c < 1) {
                    getRandomGlobalEvent(country);
                }
                else if(c < 35) {
                    getRandomLocalEvent(country);
                }
            }
            else if(pops < 10000) {//2000-10000
                if(c < 5) {
                    getRandomGlobalEvent(country);
                }
                else if(c < 45) {
                    getRandomLocalEvent(country);
                    if(c < 15) {
                        getRandomLocalEvent(country);
                    }
                }
            }
            else {//10000+
                getRandomLocalEvent(country);
                if(c < 5) {
                    getRandomGlobalEvent(country);
                }
            }
        }

        private void getRandomLocalEvent(Country country) {
            var province = country.Provinces.ElementAt(random.Next(country.Provinces.Count));
            var c = chance;
            if(c < 3) {
                if(province.ResourcesT == Resource.Gold) {
                    country.Events.Add(new classes.Event_.LocalEvent.GoldRush(province, dialog_box, camera));
                }
                else {
                    country.Events.Add(new classes.Event_.LocalEvent.ProductionBoom1(province, dialog_box, camera));
                }
            }
            //itd
        }
        private void getRandomGlobalEvent(Country country) { 
            var c = chance;
            if(c < 4) {
                country.Events.Add(new classes.Event_.GlobalEvent.Plague(country, dialog_box, camera));
            }
            //itd
        }

        public bool checkRebellion(Province province) {
            if(province.Owner_id != 0 && map.Countries[province.Owner_id].Capital != province.coordinates) {
                int happ = province.Happiness;
                if(happ < 40) {
                    if(chance > 2 * happ) {
                        startRebellion(province);
                        return true;
                    }
                }
            }
            return false;
        }
        private void startRebellion(Province province) {
            int count = province.RecruitablePopulation + (int)((province.Population - province.RecruitablePopulation) * 0.05);
            if (count > 0 && !map.Armies.Any(a=>a.Position == province.coordinates && a.OwnerId == 0)) {
                Army rebels = new Army(0, count, (-1, -1), province.coordinates);
                map.addArmy(rebels);
                Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
                province.addStatus(new Occupation(country.techStats.occTime, 0));
                province.OccupationInfo = new OccupationInfo(true, country.techStats.occTime, 0);
                TurnAction rebellion = new TurnAction.army_move((-1, -1), province.coordinates, count, rebels);
                rebellion.execute(map);
                province.Happiness = 45;
            }
        }
    }
}
