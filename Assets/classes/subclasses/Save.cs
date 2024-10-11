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
        public List<Province> provinces;
        public List<SaveCountry> countries;
        public List<Army> armies;
        public List<Map.CountryController> controllers;
        public HashSet<Relation> relations;
        public Save(Map map) { 
            this.map_name = map.name;
            this.provinces = new List<Province>(map.Provinces);
            this.countries = new List<SaveCountry>();
            this.armies = new List<Army>(map.Armies);
            this.controllers = new List<Map.CountryController>(map.Controllers);
            this.relations = new HashSet<Relation>(map.Relations);
            foreach(var c in map.Countries) {
                countries.Add(new SaveCountry(c));
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
        public string msg;
        public int? country;
        public (int, int)? province;
        public int? from, to;
        public int? amount, duration;

    }
}
