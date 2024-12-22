using Assets.classes;
using Assets.classes.subclasses;
using System.Collections.Generic;
using System.Linq;
using static Assets.classes.subclasses.Constants.ProvinceConstants;
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

        public void GetRandomEvent(Country country)
        {
            int countryPopulation = country.Provinces.Sum(p => p.Population);
            int c = chance;

            if (countryPopulation < 500 && c < 25)
            {   //0-500
                GetRandomLocalEvent(country);
            }
            else if (countryPopulation < 2000)
            {   //500-2000
                if (c < 1)
                {
                    GetRandomGlobalEvent(country);
                }
                else if (c < 35)
                {
                    GetRandomLocalEvent(country);
                }
            }
            else if (countryPopulation < 10000)
            {   //2000-10000
                if (c < 5)
                {
                    GetRandomGlobalEvent(country);
                }
                else if (c < 45)
                {
                    GetRandomLocalEvent(country);
                    if (c < 15)
                    {
                        GetRandomLocalEvent(country);
                    }
                }
            }
            else
            {   //10000+
                GetRandomLocalEvent(country);
                if (c < 5)
                {
                    GetRandomGlobalEvent(country);
                }
            }
        }

        private void GetRandomLocalEvent(Country country)
        {
            var usedProvinces = new HashSet<Province>();
            var randomCountryProvince = country.Provinces.ElementAt(random.Next(country.Provinces.Count));

            var events = new List<Event_.LocalEvent>
            {
                randomCountryProvince.ResourceType == Resource.Gold
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

        private void GetRandomGlobalEvent(Country country)
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
                .Any(province => map.GetPossibleMoveCells(province.X, province.Y, 1, 0.5f)
                    .Any(neighborProvincesCell =>
                        map.GetProvince(neighborProvincesCell.Item1, neighborProvincesCell.Item2)
                        .OwnerId != country.Id && map.Countries[map.GetProvince(neighborProvincesCell.Item1, neighborProvincesCell.Item2).OwnerId].AtWar));
        }

        public bool CheckRebellion(Province province)
        {
            if (province.OwnerId != 0 && map.Countries[province.OwnerId].Capital != province.Coordinates)
            {
                int happ = province.Happiness;
                if (happ < 40)
                {//40
                    if (chance > 2 * happ)
                    {
                        StartRebellion(province);
                        return true;
                    }
                }
            }
            return false;
        }
        
        private void StartRebellion(Province province)
        {
            int count = province.RecruitablePopulation + (int)((province.Population - province.RecruitablePopulation) * 0.05);

            if (count > 0 && !map.Armies.Any(a => a.Position == province.Coordinates && a.OwnerId == 0))
            {
                Army rebels = new Army(0, count, DEFAULT_CORD, province.Coordinates);
                map.AddArmy(rebels);
                Country country = map.Countries.FirstOrDefault(c => c.Id == province.OwnerId);
                province.AddStatus(new Occupation(country.TechStats.OccTime, 0));
                province.OccupationInfo = new OccupationInfo(true, country.TechStats.OccTime, 0);
                TurnAction rebellion = new TurnAction.ArmyMove(DEFAULT_CORD, province.Coordinates, count, rebels);
                rebellion.Execute(map);
                province.Happiness = 45;
            }
        }
    }
}