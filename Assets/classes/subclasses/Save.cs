using Assets.classes.Tax;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.classes.subclasses {
    [Serializable]
    internal class Save {
        public string map_name;
        public List<SaveProvince> provinces;
        public List<SaveCountry> countries;
        public List<SaveArmy> armies;
        public List<Map.CountryController> controllers;
        public HashSet<SaveRelation> relations;
        public Save(Map map) { 
            this.map_name = map.name;
            this.provinces = new List<SaveProvince>();
            this.countries = new List<SaveCountry>();
            this.armies = new List<SaveArmy>();
            this.controllers = new List<Map.CountryController>(map.Controllers);
            this.relations = new HashSet<SaveRelation>();
            foreach(var c in map.Countries) {
                countries.Add(new SaveCountry(c));
            }
            foreach(var p in map.Provinces) {
                provinces.Add(new SaveProvince(p));
            }
            foreach(var a in map.Armies) {
                armies.Add(new SaveArmy(a));
            }
            foreach(var r in map.Relations) {
                relations.Add(new SaveRelation(r));
            }
        }


    }
    [Serializable]
    internal class SaveCountry {
        public int id;
        public string name;
        public int prio;
        public (int, int) capital;
        public Dictionary<Resource, float> resources;
        public Dictionary<Technology, int> technology;
        public SaveColor color;
        public HashSet<(int, int)> seenTiles;
        public List<Event_> events;
        public Dictionary<int, int> opinions;
        public int tax;
        public SaveCountry(Country country) {
            this.id = country.Id;
            this.name = country.Name;
            this.prio = country.Priority;
            this.capital = country.Capital;
            this.resources = country.Resources;
            this.technology = country.Technology_;
            this.color = new(country.Color);
            this.seenTiles = country.SeenTiles;
            this.events = country.Events;
            this.opinions = country.Opinions;
            if (country.Tax is LowTaxes) tax = 0;
            else if (country.Tax is HighTaxes) tax = 2;
            else if (country.Tax is WarTaxes) tax = 3;
            else if (country.Tax is InvesmentTaxes) tax = 4;
            else tax = 2;
        }
    }
    [Serializable]
    internal class SaveProvince {
        public string name;
        public (int, int) coordinates;
        public string resource;
        public float resourceAmount;
        public int population;
        public int happinesss;
        public bool iscoast;
        public int owner;
        public Province.TerrainType terrain;
        public List<Status> status;
        public List<Building> buildings;
        public SaveProvince(Province prov) {
            name = prov.Name;
            coordinates = (prov.X, prov.Y);
            resource = prov.Resources;
            resourceAmount = prov.Resources_amount;
            population = prov.Population;
            happinesss = prov.Happiness;
            iscoast = prov.Is_coast;
            terrain = prov.Terrain;
            status = prov.Statuses;
            buildings = prov.Buildings;
        }
    }
    [Serializable]
    internal class SaveColor {
        public float r, g, b, a;
        public SaveColor(Color color) {
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
            this.a = color.a;
        }
        public Color toColor() {
            return new Color(r, g, b, a);
        }
    }
    [Serializable]
    internal class SaveArmy {
        public int ownerId;
        public int count;
        public (int, int) position;
        public (int, int) destination;
        public SaveArmy(Army army) {
            this.ownerId = army.OwnerId;
            this.count = army.Count;
            position = army.Position;
            destination = army.Destination;
        }
    }
    [Serializable]
    internal class SaveRelation {
        public (int, int) countries;
        public Relation.RelationType type;
        public int? duration;
        public int? amount;
        public HashSet<int> sideA, sideD;
        public SaveRelation(Relation relation) {
            countries = (relation.Sides[0].Id, relation.Sides[1].Id);
            type = relation.type;
            sideA = null;
            sideD=null;
            amount=null;
            duration= null;
            if(relation is Relation.War) {
                var war = relation as Relation.War;
                sideA = war.participants1.Select(p=> p.Id).ToHashSet();
                sideD = war.participants2.Select(p => p.Id).ToHashSet();
            }
            else if(relation is Relation.Subsidies) {
                var subs = relation as Relation.Subsidies;
                duration = subs.Duration;
                amount = subs.Amount;
            }
        }
    }
    [Serializable]
    internal class SaveStatus {
        public int duration;
        public int id;
        public SaveStatus(Status status) {
            duration = status.duration;
            id = status.id;
        }
    }
    [Serializable]
    internal class SaveEvent {
        public bool? type;
        public int id;
        public string msg;
        public int? country;
        public (int, int)? province;
        public int? from, to;
        public int? amount, duration;
        public SaveEvent(Event_ ev){
            country = null;
            province = null;
            from = null;
            to = null;
            amount = null;
            duration = null;
            if (ev is Event_.GlobalEvent) type = true;
            else if (ev is Event_.LocalEvent) type = false;
            else type = null;
            msg = ev.msg;
            switch (type) {
                case true:
                    country = (ev as Event_.GlobalEvent).country.Id;
                    break;
                case false:
                    province = ((int, int)?)(ev as Event_.LocalEvent).province.coordinates;
                    break;
                default:
                    from = (int?)(ev as Event_.DiploEvent).from.Id;
                    to = (int?)(ev as Event_.DiploEvent).to.Id;
                    break;
            }
        }
    }
}
