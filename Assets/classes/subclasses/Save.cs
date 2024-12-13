using Assets.classes.Tax;
using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.classes.subclasses {
    [Serializable]
    internal class Save {
        public int TurnCnt { get; set; }
        public string MapName { get; set; }
        public List<SaveProvince> Provinces { get; set; }
        public List<SaveCountry> Countries { get; set; }
        public List<SaveArmy> Armies { get; set; }
        public List<Map.CountryController> Controllers { get; set; }
        public HashSet<SaveRelation> Relations { get; set; }
        public int Turnlimit { get; set; }
        public int ResourceRate { get; set; }

        public Save(Map map) {
            TurnCnt = map.TurnCnt;
            MapName = map.name;
            Provinces = new List<SaveProvince>();
            Countries = new List<SaveCountry>();
            Armies = new List<SaveArmy>();
            Controllers = new List<Map.CountryController>(map.Controllers);
            Relations = new HashSet<SaveRelation>();
            Turnlimit = map.Turnlimit;
            ResourceRate = map.ResourceRate;

            foreach(var c in map.Countries) {
                Countries.Add(new SaveCountry(c));
            }

            foreach(var p in map.Provinces) {
                Provinces.Add(new SaveProvince(p));
            }

            foreach(var a in map.Armies) {
                Armies.Add(new SaveArmy(a));
            }

            foreach(var r in map.Relations) {
                Relations.Add(new SaveRelation(r));
            }
        }

        public Save() {
            Provinces = new();
            Countries = new();
            Armies = new();
            Controllers = new();
            Relations = new();
        }

        public static void LoadDataFromSave(Save data, Map toLoad, filter_modes mapView, 
            (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            toLoad.name = data.MapName;
            toLoad.TurnCnt = data.TurnCnt;
            toLoad.Turnlimit = data.Turnlimit;
            toLoad.ResourceRate = data.ResourceRate;
            List<Province> loadProvinces = new();
            List<Country> loadCountries = new();
            List<Army> loadArmies = new();
            HashSet<Relation> loadRelations = new();
			//needs to go provinces->countries->relations->armies->events otherwise funny stuff happens

			foreach(var a in toLoad.Armies) {
                toLoad.DestroyArmyView(a);
            }
			
			foreach (var p in data.Provinces) {
                loadProvinces.Add(p.Load());
            }

			toLoad.Provinces = null;
			toLoad.Provinces = loadProvinces;
            data.Countries = data.Countries.OrderBy(c => c.Id).ToList();
            toLoad.Countries = new();
            toLoad.Controllers = new();

			for(int i = 0; i < data.Countries.Count; i++) {
                toLoad.AddCountry(data.Countries[i].Load(toLoad, managers), data.Controllers[i]);
            }

			foreach (var r in data.Relations) {
				loadRelations.Add(r.Load(toLoad));
			}

			toLoad.Relations = null;
			toLoad.Relations = loadRelations;
            toLoad.DestroyAllArmyViews();
            toLoad.Armies = new();

			foreach (var a in data.Armies) {
                toLoad.AddArmy(a.Load());
            }

            //events on their own otherwise funni stuff
            foreach(var c in data.Countries) {
                foreach(var eV in c.Events) {
                    toLoad.Countries[c.Id].Events.Add(SaveEvent.Load(eV, toLoad, managers));
                }
            }

            foreach(var c in toLoad.Countries.Where(c=>c.Id != 0)) {
                foreach(var ev in c.Events) {
                }
            }

            for (int i = 0; i < toLoad.Controllers.Count; i++) {
                if (toLoad.Controllers[i] == Map.CountryController.Local) {
                    toLoad.CurrentPlayerId = i;
                    break;
                }
            }

            toLoad.CalcPopExtremes();

            data = null;
            mapView.Reload();
        }
    }

    [Serializable]
    internal class SaveCountry {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Prio { get; set; }
        public (int, int) Capital { get; set; }
        public Dictionary<Resource, float> Resources { get; set; }
        public Dictionary<Technology, int> Technology { get; set; }
        public SaveColor Color { get; set; }
        public int Coat { get; set; }
        public HashSet<(int, int)> RevealedTiles { get; set; }
        public HashSet<(int, int)> SeenTiles { get; set; }
        public Dictionary<int, int> Opinions { get; set; }
        public int Tax { get; set; }
        public HashSet<SaveEvent> Events { get; set; }

        public SaveCountry(Country country) {
            Id = country.Id;
            Name = country.Name;
            Prio = country.Priority;
            Capital = country.Capital;
            Resources = country.Resources;
            Technology = country.Technologies;
            Color = new(country.Color);
            RevealedTiles = country.RevealedTiles;
            SeenTiles = country.SeenTiles;
            Opinions = country.Opinions;
            Coat = country.Coat;
            Events = new();

            foreach(var e in country.Events) {
                Events.Add(new SaveEvent(e));
            }

            if (country.Tax is LowTaxes) Tax = 0;
            else if (country.Tax is HighTaxes) Tax = 2;
            else if (country.Tax is WarTaxes) Tax = 3;
            else if (country.Tax is InvesmentTaxes) Tax = 4;
            else Tax = 1;
        }
        public SaveCountry() {
            Resources = new();
            Technology = new();
            SeenTiles = new();
            Opinions = new();
        }
        public Country Load(Map map, (dialog_box_manager, camera_controller, diplomatic_relations_manager) managers) {
            Country loaded = new(Id, Name, Capital, Color.ToColor(), Coat, map);

            if(Resources != null) foreach (var rT in Resources) {
                loaded.SetResource(rT.Key, rT.Value);
            }

            foreach (var tT in Technology) {
                loaded.Technologies[tT.Key] = tT.Value;
            }

            foreach (var sT in SeenTiles) {
                loaded.SeenTiles.Add(sT);
            }

            foreach(var rT in RevealedTiles) {
                loaded.RevealedTiles.Add(rT);
            }

            foreach(var oP in Opinions) {
                if (oP.Key != 0) loaded.SetOpinion(oP.Key, oP.Value);
            }

            switch (Tax) {
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

            foreach(var p in map.Provinces.Where(p=>p.OwnerId == Id)) {
                loaded.Provinces.Add(p);
            }

            loaded.Priority = Prio;
            return loaded;
        }
    }

    [Serializable]
    internal class SaveProvince {
        public string Name { get; set; }
        public bool IsLand { get; set; }
        public (int, int) Coordinates { get; set; }
        public Resource ResourceType { get; set; }
        public float ResourceAmount { get; set; }
        public int Population { get; set; }
        public int Recruitable { get; set; }
        public int Happinesss { get; set; }
        public bool IsCoast { get; set; }
        public int OwnerId { get; set; }
        public Province.TerrainType Terrain { get; set; }
        public List<SaveStatus> Statuses { get; set; }
        public Dictionary<BuildingType, int> Buildings { get; set; }

        public SaveProvince(Province prov) {
            IsLand = prov.IsLand;
            Name = prov.Name;
            OwnerId = prov.OwnerId;
            Coordinates = (prov.X, prov.Y);
            ResourceType = prov.ResourceType;
            ResourceAmount = prov.ResourceAmount;
            Population = prov.Population;
            Recruitable = prov.RecruitablePopulation;
            Happinesss = prov.Happiness;
            IsCoast = prov.IsCoast;
            Terrain = prov.Terrain;
            Statuses = new();

            if (prov.Statuses != null) {
                foreach (var s in prov.Statuses) {
                    Statuses.Add(new(s));
                }
            }

            if (prov.Buildings != null)
            {
                Buildings = new();
                foreach (var b in prov.Buildings)
                {
                    Buildings.Add(b.Key, b.Value);
                }
            }
            else Buildings = null;
        }

        public SaveProvince() {
            Statuses = new();
            Buildings = new();
        }

        public Province Load() {
            Province loaded = new Province(Name, Coordinates.Item1, Coordinates.Item2, 
                IsLand, Terrain, ResourceType, ResourceAmount, Population, Recruitable, 
                Happinesss, IsCoast, OwnerId);

            foreach(var s in Statuses) {
                loaded.Statuses.Add(s.Load());
            }

            if (loaded.IsLand) {
                var so = loaded.Statuses.Find(s => s is Occupation) as Occupation;
                if (so != null) {
                    loaded.OccupationInfo = new(true, so.Duration, so.Occupier_id);
                }
                else loaded.OccupationInfo = new(false, 0, 0);
            }

            if (Buildings!= null) {
                loaded.Buildings = new();
                foreach(var b in Buildings)
                {
                    loaded.Buildings.Add(b.Key, b.Value);
                }
            }
            else {
                loaded.Buildings = Province.DefaultBuildings(loaded);
			}

            return loaded;
        }
    }
    [Serializable]
    internal class SaveColor {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public SaveColor(Color color) {
            R = color.r;
            G = color.g;
            B = color.b;
            A = color.a;
        }

        public SaveColor() {}

        public Color ToColor() {
            return new Color(R, G, B, A);
        }
    }

    [Serializable]
    internal class SaveArmy {
        public int OwnerId { get; set; }
        public int Count { get; set; }
        public (int, int) Position { get; set; }
        public (int, int) Destination { get; set; }

        public SaveArmy(Army army) {
            this.OwnerId = army.OwnerId;
            this.Count = army.Count;
            Position = army.Position;
            Destination = army.Destination;
        }

        public SaveArmy() {

        }

        public Army Load() {
            Army loaded = new Army(OwnerId, Count, Position, Destination);
            return loaded;
        }
    }

    [Serializable]
    internal class SaveRelation {
        public (int, int) Countries { get; set; }
        public Relation.RelationType Type { get; set; }
        public int? Duration { get; set; }
        public int? Amount { get; set; }
        public HashSet<int> SideA { get; set; }
        public HashSet<int> SideD { get; set; }

        public SaveRelation(Relation relation) {
            Countries = (relation.Sides[0].Id, relation.Sides[1].Id);
            Type = relation.Type;
            SideA = null;
            SideD = null;
            Amount = null;
            Duration = null;

            if (relation is Relation.War) {
                var war = relation as Relation.War;
                SideA = new HashSet<int>(war.Participants1.Select(p => p.Id));
                SideD = new HashSet<int>(war.Participants2.Select(p => p.Id));
            }
            else if(relation is Relation.Subsidies) {
                var subs = relation as Relation.Subsidies;
                Duration = subs.Duration;
                Amount = subs.Amount;
            }
        }

        public SaveRelation() {
            SideA = new();
            SideD = new();
        }

        public Relation Load(Map map) {
            Relation loaded = null;
            switch (Type) {
                case Relation.RelationType.War:
                    loaded = new Relation.War(map.Countries[Countries.Item1], 
                        map.Countries[Countries.Item2]);
                    foreach(var part in SideA) {
                        (loaded as Relation.War).Participants1.Add(map.Countries[part]);
                    }
                    foreach(var part in SideD) {
						(loaded as Relation.War).Participants2.Add(map.Countries[part]);
					}
                    break;
                case Relation.RelationType.Truce:
                    loaded = new Relation.Truce(map.Countries[Countries.Item1], 
                        map.Countries[Countries.Item2], (int)Duration);
                    break;
                case Relation.RelationType.Subsidies:
                    loaded = new Relation.Subsidies(map.Countries[Countries.Item1], 
                        map.Countries[Countries.Item2], (int)Amount, (int)Duration);
                    break;
                case Relation.RelationType.MilitaryAccess:
                    loaded = new Relation.MilitaryAccess(map.Countries[Countries.Item1], 
                        map.Countries[Countries.Item2]);
                    break;
                case Relation.RelationType.Alliance:
                    loaded = new Relation.Alliance(map.Countries[Countries.Item1], 
                        map.Countries[Countries.Item2]);
                    break;
                case Relation.RelationType.Vassalage:
                    loaded = new Relation.Vassalage(map.Countries[Countries.Item1], 
                        map.Countries[Countries.Item2]);
                    break;
            }

            return loaded;
        }
    }

    [Serializable]
    internal class SaveStatus {
        public int Duration {  get; set; }
        public int Id { get; set; }
        public int? Occupier { get; set; }

        public SaveStatus(Status status) {
            Duration = status.Duration;
            Id = status.Id;
            if (status is Occupation) {
                Occupier = (status as Occupation).Occupier_id;
            }
            else Occupier = null;
        }

        public SaveStatus() {

        }

        public Status Load() {
            Status loaded;
            switch (Id) {
                case 1:
                    loaded = new TaxBreak(Duration);
                    break;
                case 2:
                    loaded = new Festivities(Duration);
                    break;
                case 3:
                    loaded = new ProdBoom(Duration);
                    break;
                case 4:
                    loaded = new ProdDown(Duration);
                    break;
                case 5:
                    loaded = new Illness(Duration);
                    break;
                case 6:
                    loaded = new Disaster(Duration);
                    break;
                case 7:
                    loaded = new Occupation(Duration, (int)Occupier);
                    break;
                case 8:
                    loaded = new RecBoom(Duration);
                    break ;
                case 9:
                    loaded = new FloodStatus(Duration);
                    break;
                case 10:
                    loaded = new FireStatus(Duration);
                    break;
                default:
                    loaded = new Tribal(Duration);
                    break;
            }
            return loaded;
        }
    }

    [Serializable]
    internal class SaveEvent {
        public bool? Type { get; set; }
        public int Id { get; set; }
        public int? Country { get; set; }
        public (int, int)? Province { get; set; }
        public int? From { get; set; }
        public int? To { get; set; }
        public int? Amount { get; set; }
        public int? Duration { get; set; }

        public SaveEvent(Event_ ev){
            Country = null;
            Province = null;
            From = null;
            To = null;
            Amount = null;
            Duration = null;

            if (ev is Event_.GlobalEvent) Type = true;
            else if (ev is Event_.LocalEvent) Type = false;
            else Type = null;

            switch (Type) {
                case true:
                    Country = (ev as Event_.GlobalEvent).Country.Id;
                    GlobalId(ev as Event_.GlobalEvent);
                    break;
                case false:
                    Province = ((int, int)?)(ev as Event_.LocalEvent).Province.coordinates;
                    LocalId(ev as Event_.LocalEvent);
                    break;
                default:
                    From = (int?)(ev as Event_.DiploEvent).From.Id;
                    To = (int?)(ev as Event_.DiploEvent).To.Id;
                    DiploId(ev as Event_.DiploEvent);
                    break;
            }
        }

        public SaveEvent() { }

        private void GlobalId(Event_.GlobalEvent ev) {
            if (ev is Event_.GlobalEvent.Discontent) this.Id = 0;
            else if (ev is Event_.GlobalEvent.Happiness) this.Id = 1;
            else if (ev is Event_.GlobalEvent.Plague) this.Id = 2;
            else if (ev is Event_.GlobalEvent.EconomicRecession) this.Id = 3;
            else if (ev is Event_.GlobalEvent.TechnologicalBreakthrough) this.Id = 4;
            else if (ev is Event_.GlobalEvent.FloodEvent) this.Id = 5;
            else if (ev is Event_.GlobalEvent.FireEvent) this.Id = 6;
            else if (ev is Event_.GlobalEvent.Earthquake) this.Id = 7;
            else if (ev is Event_.GlobalEvent.Misfortune) this.Id = 8;
            else this.Id = 0;
        }

        private void LocalId(Event_.LocalEvent ev) {
            if (ev is Event_.LocalEvent.ProductionBoom) this.Id = 0;
            else if (ev is Event_.LocalEvent.GoldRush) this.Id = 1;
            else if (ev is Event_.LocalEvent.BonusRecruits) this.Id = 2;
            else if (ev is Event_.LocalEvent.WorkersStrike1) this.Id = 3;
            else if (ev is Event_.LocalEvent.WorkersStrike2) this.Id = 4;
            else if (ev is Event_.LocalEvent.WorkersStrike3) this.Id = 5;
            else if (ev is Event_.LocalEvent.PlagueFound) this.Id = 6;
            else if (ev is Event_.LocalEvent.DisasterEvent) this.Id = 7;
            else if (ev is Event_.LocalEvent.StrangeRuins1) this.Id = 8;
            else if (ev is Event_.LocalEvent.StrangeRuins2) this.Id = 9;
            else this.Id = 0;
        }

        private void DiploId(Event_.DiploEvent ev) {
            if (ev is Event_.DiploEvent.WarDeclared) this.Id = 0;
            else if (ev is Event_.DiploEvent.PeaceOffer) {
                this.Id = 1;
            }
            else if (ev is Event_.DiploEvent.CallToWar) {
                this.Id = 2;
                var evv = ev as Event_.DiploEvent.CallToWar;
                this.Country = evv.War.Sides[0].Id == From ? evv.War.Sides[1].Id : evv.War.Sides[0].Id;
            }
            else if (ev is Event_.DiploEvent.TruceEnd) {
                this.Id = 3;
            }
            else if (ev is Event_.DiploEvent.AllianceOffer) {
                this.Id = 4;
            }
            else if (ev is Event_.DiploEvent.AllianceAccepted) {
                this.Id = 5;
            }
            else if (ev is Event_.DiploEvent.AllianceDenied) {
                this.Id = 6;
            }
            else if (ev is Event_.DiploEvent.AllianceBroken) {
                this.Id = 7;
            }
            else if (ev is Event_.DiploEvent.SubsOffer) {
                this.Id = 8;
                var evv = ev as Event_.DiploEvent.SubsOffer;
                this.Duration = evv.Duration;
                this.Amount = evv.Amount;
            }
            else if (ev is Event_.DiploEvent.SubsRequest) {
                this.Id = 9;
                var evv = ev as Event_.DiploEvent .SubsRequest;
                this.Duration = evv.Duration;
                this.Amount = evv.Amount;
            }
            else if (ev is Event_.DiploEvent.SubsEndMaster) {
                this.Id = 10;
            }
            else if (ev is Event_.DiploEvent.SubsEndSlave) {
                this.Id = 11;
            }
            else if (ev is Event_.DiploEvent.AccessOffer) {
                this.Id = 12;
            }
            else if (ev is Event_.DiploEvent.AccessRequest) {
                this.Id = 13;
            }
            else if (ev is Event_.DiploEvent.AccessEndMaster) {
                this.Id = 14;
            }
            else if (ev is Event_.DiploEvent.AccessEndSlave) {
                this.Id = 15;
            }
            else if (ev is Event_.DiploEvent.VassalOffer) {
                this.Id = 16;
            }
            else if (ev is Event_.DiploEvent.VassalRebel) {
                this.Id = 17;
            }
            else this.Id = 0;
        }
        public static Event_ Load(SaveEvent ev, Map map, (dialog_box_manager, 
            camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.Type) {
                //global
                case true:
                    return LoadGlobal(ev, map, managers);
                //local
                case false:
                    return LoadLocal(ev, map, managers);
                //diplo
                default:
                    return LoadDiplo(ev, map, managers);
            }
        }
        private static Event_.GlobalEvent LoadGlobal(SaveEvent ev, Map map, (dialog_box_manager, 
            camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.Id) {
                case 0:
                    return new Event_.GlobalEvent.Discontent(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 1:
                    return new Event_.GlobalEvent.Happiness(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 2:
                    return new Event_.GlobalEvent.Plague(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 3:
                    return new Event_.GlobalEvent.EconomicRecession(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 4:
                    return new Event_.GlobalEvent.TechnologicalBreakthrough(
                        map.Countries[(int)ev.Country], managers.Item1, managers.Item2);
                case 5:
                    return new Event_.GlobalEvent.FloodEvent(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 6:
                    return new Event_.GlobalEvent.FireEvent(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 7:
                    return new Event_.GlobalEvent.Earthquake(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                case 8:
                    return new Event_.GlobalEvent.Misfortune(map.Countries[(int)ev.Country], 
                        managers.Item1, managers.Item2);
                default:
                    goto case 1;
			}
        }
        private static Event_.LocalEvent LoadLocal(SaveEvent ev, Map map, (dialog_box_manager, 
            camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.Id) {
                case 0:
                    return new Event_.LocalEvent.ProductionBoom(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 1:
                    return new Event_.LocalEvent.GoldRush(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 2:
                    return new Event_.LocalEvent.BonusRecruits(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 3:
                    return new Event_.LocalEvent.WorkersStrike1(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 4:
                    return new Event_.LocalEvent.WorkersStrike2(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 5:
                    return new Event_.LocalEvent.WorkersStrike3(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2, map);
                case 6:
                    return new Event_.LocalEvent.PlagueFound(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 7:
                    return new Event_.LocalEvent.DisasterEvent(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2);
                case 8:
                    return new Event_.LocalEvent.StrangeRuins1(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2, map);
                case 9:
                    return new Event_.LocalEvent.StrangeRuins1(map.GetProvince(((int, int))ev.Province), 
                        managers.Item1, managers.Item2, map);
                default:
                    goto case 0;
			}
        }
        private static Event_.DiploEvent LoadDiplo(SaveEvent ev, Map map, (dialog_box_manager, 
            camera_controller, diplomatic_relations_manager) managers) {
            switch (ev.Id) {
                case 0:
                    return new Event_.DiploEvent.WarDeclared(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 1:
                    Country[] warsides = { map.Countries[(int)ev.From], map.Countries[(int)ev.To] };
                    var war = map.Relations.First(r => r.Type == Relation.RelationType.War 
                        && warsides.All(val => r.Sides.Contains(val)));
                    return new Event_.DiploEvent.PeaceOffer(war as Relation.War, 
                        map.Countries[(int)ev.From], managers.Item3, managers.Item1, managers.Item2);
                case 2:
                    Country[] warsidess = {map.Countries[((int)ev.From)], 
                        map.Countries[(int)ev.Country] };
                    var warr = map.Relations.First(r => r.Type == Relation.RelationType.War 
                        && warsidess.All(val => r.Sides.Contains(val)));
                    return new Event_.DiploEvent.CallToWar(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, 
                        warr as Relation.War, managers.Item2);
                case 3:
                    return new Event_.DiploEvent.TruceEnd(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 4:
                    return new Event_.DiploEvent.AllianceOffer(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 5:
                    return new Event_.DiploEvent.AllianceAccepted(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 6:
                    return new Event_.DiploEvent.AllianceDenied(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 7:
                    return new Event_.DiploEvent.AllianceBroken(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 8:
                    return new Event_.DiploEvent.SubsOffer(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, (int)ev.Amount, 
                        (int)ev.Duration, managers.Item2);
                case 9:
                    return new Event_.DiploEvent.SubsRequest(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, (int)ev.Amount, 
                        (int)ev.Duration, managers.Item2);
                case 10:
                    return new Event_.DiploEvent.SubsEndMaster(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 11:
                    return new Event_.DiploEvent.SubsEndSlave(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 12:
                    return new Event_.DiploEvent.AccessOffer(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 13:
                    return new Event_.DiploEvent.AccessRequest(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 14:
                    Country[] accessSides = { map.Countries[(int)ev.From], map.Countries[(int)ev.To] };
                    var access = map.Relations.First(r => r.Type == Relation.RelationType.MilitaryAccess 
                    && accessSides.All(val => r.Sides.Contains(val)));
                    return new Event_.DiploEvent.AccessEndMaster(access as Relation.MilitaryAccess, 
                        map.Countries[(int)ev.From], map.Countries[(int)ev.To], managers.Item3, 
                        managers.Item1, managers.Item2);
                case 15:
					Country[] accessSidess = { map.Countries[(int)ev.From], map.Countries[(int)ev.To] };
					var accesss = map.Relations.First(r => r.Type == Relation.RelationType.MilitaryAccess 
                        && accessSidess.All(val => r.Sides.Contains(val)));
					return new Event_.DiploEvent.AccessEndMaster(accesss as Relation.MilitaryAccess, 
                        map.Countries[(int)ev.From], map.Countries[(int)ev.To], managers.Item3, 
                        managers.Item1, managers.Item2);
                case 16:
                    return new Event_.DiploEvent.VassalOffer(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                case 17:
                    return new Event_.DiploEvent.VassalRebel(map.Countries[(int)ev.From], 
                        map.Countries[(int)ev.To], managers.Item3, managers.Item1, managers.Item2);
                default:
                    goto case 0;
			}
        }
    }
}
