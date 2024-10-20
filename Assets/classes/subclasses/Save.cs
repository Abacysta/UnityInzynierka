using Assets.classes.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.classes.subclasses {
    interface ILoadable {
        object load();
    }

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
        public static void loadDataFromSave(Save data, Map toLoad) {
            toLoad.name = data.map_name;
            List<Province> loadProvinces = new();
            List<Country> loadCountries = new();
            List<Army> loadArmies = new();
            List<Map.CountryController> loadControllers = new();
            HashSet<Relation> loadRelations = new();
            //needs to go provinces->countries->armies->relations->events otherwise funny stuff happens
            foreach (var p in data.provinces) {
                toLoad.Provinces.Add(p.load());
            }
            foreach(var c in data.countries.OrderBy(c=>c.id)) {
                toLoad.Countries.Add(c.load(toLoad));
            }
            foreach(var a in data.armies) {
                toLoad.Armies.Add(a.load());
            }
            foreach (var r in data.relations) {
                toLoad.Relations.Add(r.load(toLoad));
            }
            foreach (var cc in data.controllers) {
                toLoad.Controllers.Add(cc);
            }
            foreach(var a in toLoad.Armies) {
                toLoad.reloadArmyView(a);
            }
            for (int i = 0; i < toLoad.Controllers.Count; i++) {
                if (toLoad.Controllers[i] == Map.CountryController.Local) {
                    toLoad.currentPlayer = i;
                    break;
                }
            }
            data = null;
        }

    }
    [Serializable]
    internal class SaveCountry{
        public int id;
        public string name;
        public int prio;
        public (int, int) capital;
        public Dictionary<Resource, float> resources;
        public Dictionary<Technology, int> technology;
        public SaveColor color;
        public int coat;
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
            //this.events = country.Events;
            this.opinions = country.Opinions;
            this.coat = country.Coat;
            if (country.Tax is LowTaxes) tax = 0;
            else if (country.Tax is HighTaxes) tax = 2;
            else if (country.Tax is WarTaxes) tax = 3;
            else if (country.Tax is InvesmentTaxes) tax = 4;
            else tax = 1;
        }
        public Country load(Map map) {
            Country loaded = new(id, name, capital, color.toColor(), coat, map);
            foreach (var rT in resources) {
                loaded.setResource(rT.Key, rT.Value);
            }
            foreach (var tT in technology) {
                loaded.Technology_[tT.Key] = tT.Value;
            }
            foreach (var sT in seenTiles) {
                loaded.SeenTiles.Add(sT);
            }
            foreach(var oP in opinions) {
                loaded.Opinions.Add(oP.Key, oP.Value);
            }
            switch (tax) {
                case 0:
                    loaded.Tax = new LowTaxes();
                    break;
                case 2:
                    loaded.Tax = new HighTaxes();
                    break;
                case 3:
                    loaded.Tax = new WarTaxes();
                    break;
                case 4:
                    loaded.Tax = new InvesmentTaxes();
                    break;
                default:
                    loaded.Tax = new MediumTaxes();
                    break;

            }
            foreach(var p in map.Provinces.Where(p=>p.Owner_id == id)) {
                loaded.Provinces.Add(p);
            }
            loaded.Priority = prio;
            return loaded;
        }
    }
    [Serializable]
    internal class SaveProvince {
        public string? id;
        public string name;
        public string type;
        public (int, int) coordinates;
        public string resource;
        public float resourceAmount;
        public int population;
        public int recruitable;
        public int happinesss;
        public bool iscoast;
        public int owner;
        public Province.TerrainType terrain;
        public List<Status> status;
        public List<Building> buildings;
        public SaveProvince(Province prov) {
            id = prov.Id;
            type = prov.Type;
            name = prov.Name;
            coordinates = (prov.X, prov.Y);
            resource = prov.Resources;
            resourceAmount = prov.Resources_amount;
            population = prov.Population;
            recruitable = prov.RecruitablePopulation;
            happinesss = prov.Happiness;
            iscoast = prov.Is_coast;
            terrain = prov.Terrain;
            status = prov.Statuses;
            buildings = prov.Buildings;
        }

        public Province load() {
            Province loaded = new Province(id, name, coordinates.Item1, coordinates.Item2, type, terrain, resource, resourceAmount, population, recruitable, happinesss, iscoast, owner);
            loaded.Statuses = status;
            loaded.Buildings = buildings;
            return loaded;
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
        public Army load() {
            Army loaded = new Army(ownerId, count, position, destination);
            return loaded;
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
                sideA = new HashSet<int>(war.participants1.Select(p=> p.Id));
                sideD = new HashSet<int>(war.participants2.Select(p => p.Id));
            }
            else if(relation is Relation.Subsidies) {
                var subs = relation as Relation.Subsidies;
                duration = subs.Duration;
                amount = subs.Amount;
            }
        }

        public Relation load(Map map) {
            Relation loaded = null;
            switch (type) {
                case Relation.RelationType.War:
                    loaded = new Relation.War(map.Countries[countries.Item1], map.Countries[countries.Item2]);
                    foreach(var part in sideA) {
                        (loaded as Relation.War).participants1.Add(map.Countries[part]);
                    }
                    foreach(var part in sideD) {
						(loaded as Relation.War).participants2.Add(map.Countries[part]);
					}
                    break;
                case Relation.RelationType.Truce:
                    loaded = new Relation.Truce(map.Countries[countries.Item1], map.Countries[countries.Item2], (int)duration);
                    break;
                case Relation.RelationType.Subsidies:
                    loaded = new Relation.Subsidies(map.Countries[countries.Item1], map.Countries[countries.Item2], (int)amount, (int)duration);
                    break;
                case Relation.RelationType.MilitaryAccess:
                    loaded = new Relation.MilitaryAccess(map.Countries[countries.Item1], map.Countries[countries.Item2]);
                    break;
                case Relation.RelationType.Alliance:
                    loaded = new Relation.Alliance(map.Countries[countries.Item1], map.Countries[countries.Item2]);
                    break;
                case Relation.RelationType.Vassalage:
                    loaded = new Relation.Vassalage(map.Countries[countries.Item1], map.Countries[countries.Item2]);
                    break;
            }
            return loaded;
        }
    }
    [Serializable]
    internal class SaveStatus {
        public int duration;
        public int id;
        Status.StatusType type;
        public int? occupier;
        public SaveStatus(Status status) {
            duration = status.duration;
            id = status.id;
            type = status.type;
            if (status is Occupation) {
                occupier = (status as Occupation).Occupier_id;
            }
            else occupier = null;
        }
        public Status load() {
            Status loaded;
            switch (id) {
                case 1:
                    loaded = new TaxBreak(duration);
                    break;
                case 2:
                    loaded = new Festivities(duration);
                    break;
                case 3:
                    loaded = new ProdBoom(duration);
                    break;
                case 4:
                    loaded = new ProdDown(duration);
                    break;
                case 5:
                    loaded = new Illness(duration);
                    break;
                case 6:
                    loaded = new Disaster(duration);
                    break;
                case 7:
                    loaded = new Occupation(duration, (int)occupier);
                    break;
                case 8:
                    loaded = new RecBoom(duration);
                    break ;
                case 9:
                    loaded = new Flood(duration);
                    break;
                case 10:
                    loaded = new Fire(duration);
                    break;
                default:
                    loaded = new Tribal(duration);
                    break;

            }
            return loaded;

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
