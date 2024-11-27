using Assets.classes;
using Assets.classes.subclasses;
using System.Collections.Generic;
using System.Linq;
using static Assets.classes.subclasses.Constants;
using UnityEngine;

namespace Assets.map.scripts
{
    public class random_events_manager : MonoBehaviour
    {
        [SerializeField] private Map map;
        [SerializeField] private dialog_box_manager dialog_box;
        [SerializeField] private camera_controller camera_controller;

        private System.Random random;

        public int chance { get => random.Next(100); }

        void Start()
        {
            random = new System.Random();
        }

        public void getRandomEvent(Country country)
        {
            int countryPopulation = country.Provinces.Sum(p => p.Population);
            int c = chance;

            if (countryPopulation < 500 && c < 25)
            {   //0-500
                getRandomLocalEvent(country);
            }
            else if (countryPopulation < 2000)
            {   //500-2000
                if (c < 1)
                {
                    getRandomGlobalEvent(country);
                }
                else if (c < 35)
                {
                    getRandomLocalEvent(country);
                }
            }
            else if (countryPopulation < 10000)
            {   //2000-10000
                if (c < 5)
                {
                    getRandomGlobalEvent(country);
                }
                else if (c < 45)
                {
                    getRandomLocalEvent(country);
                    if (c < 15)
                    {
                        getRandomLocalEvent(country);
                    }
                }
            }
            else
            {   //10000+
                getRandomLocalEvent(country);
                if (c < 5)
                {
                    getRandomGlobalEvent(country);
                }
            }
        }

        private void getRandomLocalEvent(Country country)
        {
            var usedProvinces = new HashSet<Province>();
            var randomCountryProvince = country.Provinces.ElementAt(random.Next(country.Provinces.Count));

            var events = new List<Event_.LocalEvent>
            {
                randomCountryProvince.ResourcesT == Resource.Gold
                    ? new Event_.LocalEvent.GoldRush(randomCountryProvince, dialog_box, camera_controller)
                    : new Event_.LocalEvent.ProductionBoom(randomCountryProvince, dialog_box, camera_controller),

                new Event_.LocalEvent.ProductionBoom(randomCountryProvince, dialog_box, camera_controller),
                new Event_.LocalEvent.BonusRecruits(randomCountryProvince, dialog_box, camera_controller),
                new Event_.LocalEvent.WorkersStrike1(randomCountryProvince, dialog_box, camera_controller),
                new Event_.LocalEvent.WorkersStrike2(randomCountryProvince, dialog_box, camera_controller),
                new Event_.LocalEvent.WorkersStrike3(randomCountryProvince, dialog_box, camera_controller, map),

                randomCountryProvince.Population > 1000
                    ? new Event_.LocalEvent.PlagueFound(randomCountryProvince, dialog_box, camera_controller)
                    : new Event_.LocalEvent.WorkersStrike3(randomCountryProvince, dialog_box, camera_controller, map),

                new Event_.LocalEvent.DisasterEvent(randomCountryProvince, dialog_box, camera_controller),
                new Event_.LocalEvent.StrangeRuins1(randomCountryProvince, dialog_box, camera_controller, map),
                new Event_.LocalEvent.StrangeRuins2(randomCountryProvince, dialog_box, camera_controller)
            };

            float chanceThreshold = 80f;

            while (chance < chanceThreshold)
            {
                if (usedProvinces.Count >= country.Provinces.Count) break;

                do
                {
                    randomCountryProvince = country.Provinces.ElementAt(random.Next(country.Provinces.Count));
                }
                while (usedProvinces.Contains(randomCountryProvince));

                usedProvinces.Add(randomCountryProvince);

                var randomEvent = events[random.Next(events.Count)];
                country.Events.Add(randomEvent);

                chanceThreshold /= 2;

                if (chanceThreshold < 1) break;
            }
        }

        private void getRandomGlobalEvent(Country country)
        {
            int c = chance;
            int countryPopulation = country.Provinces.Sum(p => p.Population);
            var countryHappiness = country.Provinces.Average(p => p.Happiness);
            
            if (c < 5)
            {
                if (countryPopulation > 10000)
                {
                    country.Events.Add(new Event_.GlobalEvent.Plague(country, dialog_box, camera_controller));
                }
                else
                {
                    country.Events.Add(new Event_.GlobalEvent.FloodEvent(country, dialog_box, camera_controller));
                }
            }
            else if (c < 10)
            {
                country.Events.Add(new Event_.GlobalEvent.FloodEvent(country, dialog_box, camera_controller));
            }
            else if (c < 15)
            {
                country.Events.Add(new Event_.GlobalEvent.TechnologicalBreakthrough(country, dialog_box, camera_controller));
            }
            else if (c < 20)
            {
                country.Events.Add(new Event_.GlobalEvent.FireEvent(country, dialog_box, camera_controller));
            }
            else if (c < 25)
            {
                country.Events.Add(new Event_.GlobalEvent.Earthquake(country, dialog_box, camera_controller));
            }
            else if (c < 30)
            {
                country.Events.Add(new Event_.GlobalEvent.Misfortune(country, dialog_box, camera_controller));
            }
            else if (c < 50)
            {
                if (IsWarNearbyCountry(country))
                {
                    country.Events.Add(new Event_.GlobalEvent.EconomicRecession(country, dialog_box, camera_controller));
                }
                else
                {
                    country.Events.Add(new Event_.GlobalEvent.Misfortune(country, dialog_box, camera_controller));
                }
            }
            else if (c < 70)
            {
                if (countryHappiness < 30)
                {
                    country.Events.Add(new Event_.GlobalEvent.Discontent(country, dialog_box, camera_controller));
                }
                else
                {
                    if (!IsWarNearbyCountry(country))
                    {
                        country.Events.Add(new Event_.GlobalEvent.Happiness(country, dialog_box, camera_controller));
                    }
                }
            }
        }

        private bool IsWarNearbyCountry(Country country)
        {
            return country.AtWar || country.Provinces
                .Any(province => map.getPossibleMoveCells(province.X, province.Y, 1, 0.5f)
                    .Any(neighborProvincesCell =>
                        map.getProvince(neighborProvincesCell.Item1, neighborProvincesCell.Item2)
                        .Owner_id != country.Id && map.Countries[map.getProvince(neighborProvincesCell.Item1, neighborProvincesCell.Item2).Owner_id].AtWar));
        }

        public bool checkRebellion(Province province)
        {
            if (province.Owner_id != 0 && map.Countries[province.Owner_id].Capital != province.coordinates)
            {
                int happ = province.Happiness;
                if (happ < -500)
                {//40
                    if (chance > 2 * happ)
                    {
                        startRebellion(province);
                        return true;
                    }
                }
            }
            return false;
        }
        
        private void startRebellion(Province province)
        {
            int count = province.RecruitablePopulation + (int)((province.Population - province.RecruitablePopulation) * 0.05);

            if (count > 0 && !map.Armies.Any(a => a.Position == province.coordinates && a.OwnerId == 0))
            {
                Army rebels = new Army(0, count, DEFAULT_CORD, province.coordinates);
                map.addArmy(rebels);
                Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
                province.addStatus(new Occupation(country.techStats.occTime, 0));
                province.OccupationInfo = new OccupationInfo(true, country.techStats.occTime, 0);
                TurnAction rebellion = new TurnAction.army_move(DEFAULT_CORD, province.coordinates, count, rebels);
                rebellion.execute(map);
                province.Happiness = 45;
            }
        }
    }
}