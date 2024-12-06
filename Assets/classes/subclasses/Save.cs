using Assets.classes.Tax;
using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.classes.subclasses {
    

    [Serializable]
    internal class Save {
        public int turnCnt;
        public string map_name;
        public List<SaveProvince> provinces;
        public List<SaveCountry> countries;
        public List<SaveArmy> armies;
        public List<Map.CountryController> controllers;
        public HashSet<SaveRelation> relations;
        public int turnlimit;
        public int resourceRate;
        public Save(Map map) {
            this.turnCnt = map.turnCnt;
            this.map_name = map.name;
            this.provinces = new List<SaveProvince>();
            this.countries = new List<SaveCountry>();
            this.armies = new List<SaveArmy>();
            this.controllers = new List<Map.CountryController>(map.Controllers);
            this.relations = new HashSet<SaveRelation>();
            this.turnlimit = map.Turnlimit;
            this.resourceRate = map.ResourceRate;
            foreach(var c in map.Countries) {
                countries.Add(new SaveCountry(c));
            }
            foreach(var p in map.Provinces) {
                //Debug.Log("saving province: " + p.coordinates.ToString());
                provinces.Add(new SaveProvince(p));
            }
            foreach(var a in map.Armies) {
                armies.Add(new SaveArmy(a));
            }
            foreach(var r in map.Relations) {
                relations.Add(new SaveRelation(r));
            }
        }

        public Save() {
            provinces = new();
            countries = new();
            armies = new();
            controllers = new();
            relations = new();
        }

        public static void loadDataFromSave(Save data, Map toLoad, filter_modes mapView, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            toLoad.name = data.map_name;
            toLoad.turnCnt = data.turnCnt;
            toLoad.Turnlimit = data.turnlimit;
            toLoad.ResourceRate = data.resourceRate;
            List<Province> loadProvinces = new();
            List<Country> loadCountries = new();
            List<Army> loadArmies = new();
            List<Map.CountryController> loadControllers = new();
            HashSet<Relation> loadRelations = new();
			//needs to go provinces->countries->relations->armies->events otherwise funny stuff happens


			foreach(var a in toLoad.Armies) {
                toLoad.destroyArmyView(a);
            }
			
			foreach (var p in data.provinces) {
                loadProvinces.Add(p.load());
            }
			toLoad.Provinces = null;
			toLoad.Provinces = loadProvinces;
            data.countries = data.countries.OrderBy(c => c.id).ToList();
            toLoad.Countries = new();
			for(int i = 0; i < data.countries.Count; i++) {
                toLoad.addCountry(data.countries[i].load(toLoad, managers), data.controllers[i]);
            }
			foreach (var r in data.relations) {
				loadRelations.Add(r.load(toLoad));
			}
			toLoad.Relations = null;
			toLoad.Relations = loadRelations;
            toLoad.destroyAllArmyViews();
            toLoad.Armies = new();
			foreach (var a in data.armies) {
                toLoad.addArmy(a.load());
            }
            //events on their own otherwise funni stuff
            foreach(var c in data.countries) {
                foreach(var eV in c.events) {
                    toLoad.Countries[c.id].Events.Add(SaveEvent.load(eV, toLoad, managers));
                }
            }
            //
            foreach(var c in toLoad.Countries.Where(c=>c.Id != 0)) {
                //Debug.Log(c.Events.Count + "->>");
                foreach(var ev in c.Events) {
                    //Debug.Log(ev.msg + "<->");
                }
            }
            //
            for (int i = 0; i < toLoad.Controllers.Count; i++) {
                if (toLoad.Controllers[i] == Map.CountryController.Local) {
                    toLoad.currentPlayer = i;
                    break;
                }
            }
			//foreach (var r in toLoad.Relations) {
			//	Debug.Log(r.type.ToString() + "->" + r.Sides[0].Id + " " + r.Sides[1].Id);
			//}
			data = null;
            mapView.Reload();
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
        public HashSet<(int, int)> revealedTiles;
        public HashSet<(int, int)> seenTiles;
        public Dictionary<int, int> opinions;
        public int tax;
        public HashSet<SaveEvent> events;
        public SaveCountry(Country country) {
            this.id = country.Id;
            this.name = country.Name;
            this.prio = country.Priority;
            this.capital = country.Capital;
            this.resources = country.Resources;
            this.technology = country.Technologies;
            this.color = new(country.Color);
            this.revealedTiles = country.RevealedTiles;
            this.seenTiles = country.SeenTiles;
            this.opinions = country.Opinions;
            this.coat = country.Coat;
            events = new();
            foreach(var e in country.Events) {
                events.Add(new SaveEvent(e));
            }
            if (country.Tax is LowTaxes) tax = 0;
            else if (country.Tax is HighTaxes) tax = 2;
            else if (country.Tax is WarTaxes) tax = 3;
            else if (country.Tax is InvesmentTaxes) tax = 4;
            else tax = 1;
        }
        public SaveCountry() {
            resources = new();
            technology = new();
            seenTiles = new();
            opinions = new();
        }
        public Country load(Map map, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            //Debug.Log("loading country " + id);
            Country loaded = new(id, name, capital, color.toColor(), coat, map);
            if(resources != null) foreach (var rT in resources) {
                loaded.setResource(rT.Key, rT.Value);
            }
            foreach (var tT in technology) {
                loaded.Technologies[tT.Key] = tT.Value;
            }
            foreach (var sT in seenTiles) {
                loaded.SeenTiles.Add(sT);
            }
            foreach(var rT in revealedTiles) {
                loaded.RevealedTiles.Add(rT);
            }
            foreach(var oP in opinions) {
                if (oP.Key != 0) loaded.SetOpinion(oP.Key, oP.Value);
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
            foreach(var p in map.Provinces.Where(p=>p.OwnerId == id)) {
                loaded.Provinces.Add(p);
            }
            loaded.Priority = prio;
            return loaded;
        }
    }
    [Serializable]
    internal class SaveProvince {
        public string name;
        public bool isLand;
        public (int, int) coordinates;
        public Resource resource;
        public float resourceAmount;
        public int population;
        public int recruitable;
        public int happinesss;
        public bool iscoast;
        public int owner;
        public Province.TerrainType terrain;
        public List<SaveStatus> status;
        public Dictionary<BuildingType, int> buildings;
        public SaveProvince(Province prov) {
            isLand = prov.IsLand;
            name = prov.Name;
            owner = prov.OwnerId;
            coordinates = (prov.X, prov.Y);
            resource = prov.ResourceType;
            resourceAmount = prov.ResourceAmount;
            population = prov.Population;
            recruitable = prov.RecruitablePopulation;
            happinesss = prov.Happiness;
            iscoast = prov.IsCoast;
            terrain = prov.Terrain;
            status = new();
            if(prov.Statuses!=null) foreach(var s in prov.Statuses) {
                status.Add(new(s));
            }
            //Debug.Log(prov.coordinates.ToString() + " -> " + prov.Buildings.ToString());
            if (prov.Buildings != null)
            {
                buildings = new();
                foreach (var b in prov.Buildings)
                {
                    buildings.Add(b.Key, b.Value);
                }
            }
            else buildings = null;
        }

        public SaveProvince() {
            status = new();
            buildings = new();
        }

        public Province load() {
            Province loaded = new Province(name, coordinates.Item1, coordinates.Item2, isLand, terrain, resource, resourceAmount, population, recruitable, happinesss, iscoast, owner);
            foreach(var s in status) {
                loaded.Statuses.Add(s.load());
            }
            if (loaded.IsLand) {
                var so = loaded.Statuses.Find(s => s is Occupation) as Occupation;
                if (so != null) {
                    loaded.OccupationInfo = new(true, so.Duration, so.Occupier_id);
                }
                else loaded.OccupationInfo = new(false, 0, 0);
            }
            if(buildings!= null) {
                loaded.Buildings = new();
                foreach(var b in buildings)
                {
                    loaded.Buildings.Add(b.Key, b.Value);
                }
            }
            else {
                loaded.Buildings = Province.defaultBuildings(loaded);
			}
            //if(owner!=0)Debug.Log("loaded " + coordinates.ToString() + " to " + owner);
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

        public SaveColor() { }

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

        public SaveArmy() {

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
                sideA = new HashSet<int>(war.participants1.Select(p => p.Id));
                sideD = new HashSet<int>(war.participants2.Select(p => p.Id));
            }
            else if(relation is Relation.Subsidies) {
                var subs = relation as Relation.Subsidies;
                duration = subs.Duration;
                amount = subs.Amount;
            }
        }

        public SaveRelation() {
            sideA = new();
            sideD = new();
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
            //Debug.Log("loaded " + loaded.type.ToString() + " between " + loaded.Sides[0] + " and " + loaded.Sides[1]);
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
            duration = status.Duration;
            id = status.Id;
            type = status.Type;
            if (status is Occupation) {
                occupier = (status as Occupation).Occupier_id;
            }
            else occupier = null;
        }

        public SaveStatus() {

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
                    loaded = new FloodStatus(duration);
                    break;
                case 10:
                    loaded = new FireStatus(duration);
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
            switch (type) {
                case true:
                    country = (ev as Event_.GlobalEvent).Country.Id;
                    globalId(ev as Event_.GlobalEvent);
                    break;
                case false:
                    province = ((int, int)?)(ev as Event_.LocalEvent).Province.coordinates;
                    localId(ev as Event_.LocalEvent);
                    break;
                default:
                    from = (int?)(ev as Event_.DiploEvent).From.Id;
                    to = (int?)(ev as Event_.DiploEvent).To.Id;
                    diploId(ev as Event_.DiploEvent);
                    break;
            }
        }

        public SaveEvent() { }

        private void globalId(Event_.GlobalEvent ev) {
            if (ev is Event_.GlobalEvent.Discontent) this.id = 0;
            else if (ev is Event_.GlobalEvent.Happiness) this.id = 1;
            else if (ev is Event_.GlobalEvent.Plague) this.id = 2;
            else if (ev is Event_.GlobalEvent.EconomicRecession) this.id = 3;
            else if (ev is Event_.GlobalEvent.TechnologicalBreakthrough) this.id = 4;
            else if (ev is Event_.GlobalEvent.FloodEvent) this.id = 5;
            else if (ev is Event_.GlobalEvent.FireEvent) this.id = 6;
            else if (ev is Event_.GlobalEvent.Earthquake) this.id = 7;
            else if (ev is Event_.GlobalEvent.Misfortune) this.id = 8;
            else this.id = 0;
        }
        private void localId(Event_.LocalEvent ev) {
            if (ev is Event_.LocalEvent.ProductionBoom) this.id = 0;
            else if (ev is Event_.LocalEvent.GoldRush) this.id = 1;
            else if (ev is Event_.LocalEvent.BonusRecruits) this.id = 2;
            else if (ev is Event_.LocalEvent.WorkersStrike1) this.id = 3;
            else if (ev is Event_.LocalEvent.WorkersStrike2) this.id = 4;
            else if (ev is Event_.LocalEvent.WorkersStrike3) this.id = 5;
            else if (ev is Event_.LocalEvent.PlagueFound) this.id = 6;
            else if (ev is Event_.LocalEvent.DisasterEvent) this.id = 7;
            else if (ev is Event_.LocalEvent.StrangeRuins1) this.id = 8;
            else if (ev is Event_.LocalEvent.StrangeRuins2) this.id = 9;
            else this.id = 0;
        }
        private void diploId(Event_.DiploEvent ev) {
            if (ev is Event_.DiploEvent.WarDeclared) this.id = 0;
            else if (ev is Event_.DiploEvent.PeaceOffer) {
                this.id = 1;
            }
            else if (ev is Event_.DiploEvent.CallToWar) {
                this.id = 2;
                var evv = ev as Event_.DiploEvent.CallToWar;
                this.country = evv.War.Sides[0].Id == from ? evv.War.Sides[1].Id : evv.War.Sides[0].Id;
            }
            else if (ev is Event_.DiploEvent.TruceEnd) {
                this.id = 3;
            }
            else if (ev is Event_.DiploEvent.AllianceOffer) {
                this.id = 4;
            }
            else if (ev is Event_.DiploEvent.AllianceAccepted) {
                this.id = 5;
            }
            else if (ev is Event_.DiploEvent.AllianceDenied) {
                this.id = 6;
            }
            else if (ev is Event_.DiploEvent.AllianceBroken) {
                this.id = 7;
            }
            else if (ev is Event_.DiploEvent.SubsOffer) {
                this.id = 8;
                var evv = ev as Event_.DiploEvent.SubsOffer;
                this.duration = evv.Duration;
                this.amount = evv.Amount;
            }
            else if (ev is Event_.DiploEvent.SubsRequest) {
                this.id = 9;
                var evv = ev as Event_.DiploEvent .SubsRequest;
                this.duration = evv.Duration;
                this.amount = evv.Amount;
            }
            else if (ev is Event_.DiploEvent.SubsEndMaster) {
                this.id = 10;
            }
            else if (ev is Event_.DiploEvent.SubsEndSlave) {
                this.id = 11;
            }
            else if (ev is Event_.DiploEvent.AccessOffer) {
                this.id = 12;
            }
            else if (ev is Event_.DiploEvent.AccessRequest) {
                this.id = 13;
            }
            else if (ev is Event_.DiploEvent.AccessEndMaster) {
                this.id = 14;
            }
            else if (ev is Event_.DiploEvent.AccessEndSlave) {
                this.id = 15;
            }
            else if (ev is Event_.DiploEvent.VassalOffer) {
                this.id = 16;
            }
            else if (ev is Event_.DiploEvent.VassalRebel) {
                this.id = 17;
            }
            else this.id = 0;
        }
        public static Event_ load(SaveEvent ev, Map map, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.type) {
                //global
                case true:
                    return loadGlobal(ev, map, managers);
                //local
                case false:
                    return loadLocal(ev, map, managers);
                //diplo
                default:
                    return loadDiplo(ev, map, managers);
            }
        }
        private static Event_.GlobalEvent loadGlobal(SaveEvent ev, Map map, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.id) {
                case 0:
                    return new Event_.GlobalEvent.Discontent(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 1:
                    return new Event_.GlobalEvent.Happiness(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 2:
                    return new Event_.GlobalEvent.Plague(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 3:
                    return new Event_.GlobalEvent.EconomicRecession(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 4:
                    return new Event_.GlobalEvent.TechnologicalBreakthrough(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 5:
                    return new Event_.GlobalEvent.FloodEvent(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 6:
                    return new Event_.GlobalEvent.FireEvent(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 7:
                    return new Event_.GlobalEvent.Earthquake(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                case 8:
                    return new Event_.GlobalEvent.Misfortune(map.Countries[(int)ev.country], managers.Item1, managers.Item2);
                default:
                    goto case 1;
			}
        }
        private static Event_.LocalEvent loadLocal(SaveEvent ev, Map map, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.id) {
                case 0:
                    return new Event_.LocalEvent.ProductionBoom(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 1:
                    return new Event_.LocalEvent.GoldRush(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 2:
                    return new Event_.LocalEvent.BonusRecruits(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 3:
                    return new Event_.LocalEvent.WorkersStrike1(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 4:
                    return new Event_.LocalEvent.WorkersStrike2(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 5:
                    return new Event_.LocalEvent.WorkersStrike3(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2, map);
                case 6:
                    return new Event_.LocalEvent.PlagueFound(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 7:
                    return new Event_.LocalEvent.DisasterEvent(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2);
                case 8:
                    return new Event_.LocalEvent.StrangeRuins1(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2, map);
                case 9:
                    return new Event_.LocalEvent.StrangeRuins1(map.getProvince(((int, int))ev.province), managers.Item1, managers.Item2, map);
                default:
                    goto case 0;
			}
        }
        private static Event_.DiploEvent loadDiplo(SaveEvent ev, Map map, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.id) {
                case 0:
                    return new Event_.DiploEvent.WarDeclared(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 1:
                    Country[] warsides = { map.Countries[(int)ev.from], map.Countries[(int)ev.to] };
                    var war = map.Relations.First(r => r.type == Relation.RelationType.War && warsides.All(val => r.Sides.Contains(val)));
                    return new Event_.DiploEvent.PeaceOffer(war as Relation.War, map.Countries[(int)ev.from], managers.Item3, managers.Item1, managers.Item2);
                case 2:
                    Country[] warsidess = {map.Countries[((int)ev.from)], map.Countries[(int)ev.country] };
                    var warr = map.Relations.First(r => r.type == Relation.RelationType.War && warsidess.All(val => r.Sides.Contains(val)));
                    return new Event_.DiploEvent.CallToWar(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, warr as Relation.War, managers.Item2);
                case 3:
                    return new Event_.DiploEvent.TruceEnd(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 4:
                    return new Event_.DiploEvent.AllianceOffer(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 5:
                    return new Event_.DiploEvent.AllianceAccepted(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 6:
                    return new Event_.DiploEvent.AllianceDenied(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 7:
                    return new Event_.DiploEvent.AllianceBroken(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 8:
                    return new Event_.DiploEvent.SubsOffer(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, (int)ev.amount, (int)ev.duration, managers.Item2);
                case 9:
                    return new Event_.DiploEvent.SubsRequest(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, (int)ev.amount, (int)ev.duration, managers.Item2);
                case 10:
                    return new Event_.DiploEvent.SubsEndMaster(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 11:
                    return new Event_.DiploEvent.SubsEndSlave(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 12:
                    return new Event_.DiploEvent.AccessOffer(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 13:
                    return new Event_.DiploEvent.AccessRequest(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 14:
                    Country[] accessSides = { map.Countries[(int)ev.from], map.Countries[(int)ev.to] };
                    var access = map.Relations.First(r => r.type == Relation.RelationType.MilitaryAccess && accessSides.All(val => r.Sides.Contains(val)));
                    return new Event_.DiploEvent.AccessEndMaster(access as Relation.MilitaryAccess, map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 15:
					Country[] accessSidess = { map.Countries[(int)ev.from], map.Countries[(int)ev.to] };
					var accesss = map.Relations.First(r => r.type == Relation.RelationType.MilitaryAccess && accessSidess.All(val => r.Sides.Contains(val)));
					return new Event_.DiploEvent.AccessEndMaster(accesss as Relation.MilitaryAccess, map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 16:
                    return new Event_.DiploEvent.VassalOffer(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                case 17:
                    return new Event_.DiploEvent.VassalRebel(map.Countries[(int)ev.from], map.Countries[(int)ev.to], managers.Item3, managers.Item1, managers.Item2);
                default:
                    goto case 0;
			}
        }
    }
}
