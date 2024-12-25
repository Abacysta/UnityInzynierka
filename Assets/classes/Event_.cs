using Assets.classes.subclasses;
using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.classes {
    [Serializable]
    public class Event_ {
        public virtual void Call() {}
        public virtual void Accept() {}
        public virtual void Reject() {}
        public virtual void Zoom() {}
        public virtual string Message { get { return ""; } }
        public Dictionary<Resource, float> Cost { get; set; }
        public bool IsRejectable { get; set; } = true;

        public class GlobalEvent : Event_ {
            public Country Country { get; set; }
            protected dialog_box_manager dialog_box;
            protected camera_controller camera;

            public GlobalEvent(Country country, dialog_box_manager dialog, camera_controller camera, 
                bool isRejectable) {
                Country = country;
                this.camera = camera;
                dialog_box = dialog;
                Cost = GetCost();
                IsRejectable = isRejectable;
            }

            public override void Accept() {
                Country.ModifyResources(GetCost(), false);
            }

            public override void Reject() { Accept(); }

            public override void Call() {
                dialog_box.InvokeEventBox(this);
            }

            public override void Zoom() {
                camera.ZoomCameraOnCountry(Country);
            }

            protected virtual Dictionary<Resource, float> GetCost() {
                return null;
            }


            internal class Discontent : GlobalEvent {
                public Discontent(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, true) {}

                public override string Message { get { 
                        return "A discontent has spread in the country. You can bribe officials to lower its impact"; 
                    } 
                }

                public override void Accept() {
                    base.Accept();
                    foreach (var p in Country.Provinces) {
                        if(p.Coordinates != Country.Capital) p.Happiness -= 5;
                        p.Happiness -= 5;
                    }
                }

                public override void Reject() {
                    foreach (var p in Country.Provinces) {
                        if(p.Coordinates != Country.Capital) p.Happiness -= 10;
                        p.Happiness -= 20;
                    }
                }

                protected override Dictionary<Resource, float> GetCost() {
                    var cost = new Dictionary<Resource, float> {
                        { Resource.Gold, 0 },
                        { Resource.AP, 0 }
                    };

                    foreach (var p in Country.Provinces) {
                        cost[Resource.Gold] += (float)Math.Round(25 * p.Population / 500f, 1);
                        cost[Resource.AP] += 0.2f;
                    }

                    return cost;
                }
            }


			internal class Happiness : GlobalEvent {
                public Happiness(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, false) {}

                public override string Message { get { 
                        return "Happiness has increased in the country"; 
                    } 
                }

                public override void Accept() {
                    base.Accept();
                    foreach (var p in Country.Provinces) {
                        p.Happiness += 10;
                    }
                }
            }


			internal class Plague : GlobalEvent {
                public Plague(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, true) {
                }

                public override string Message { get { 
                        return "Plague has struck the nation. You may pay to your researchers to quicken the cure's invention."; 
                    } 
                }

                public override void Accept() {
                    base.Accept();
                    foreach (var p in Country.Provinces)
                    {
                        p.AddStatus(new Illness(2));
                    }
                }

                protected override Dictionary<Resource, float> GetCost() {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 0 }
                    };
                    foreach (var p in Country.Provinces) {
                        if (p.Coordinates == Country.Capital) cost[Resource.Gold] += 15;
                        cost[Resource.Gold] += 30;
                    }
                    return cost;
                }

                public override void Reject() {
                    foreach (var p in Country.Provinces)
                    {
                        p.AddStatus(new Illness(5));
                    }
                }
            }


			internal class EconomicRecession : GlobalEvent
            {
                public EconomicRecession(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, true) {}

                public override string Message { get { 
                        return "The global economy has entered a downturn. Your nation's production is suffering as a result. " +
                            "Will you attempt to fight the reccesion by paying a cost in gold and AP?"; 
                    } 
                }

                public override void Accept()
                {
                    base.Accept();
                    foreach(var p in Country.Provinces)
                    {
                        if (UnityEngine.Random.Range(0f,1f) < 0.05f)
                        {
                            p.AddStatus(new ProdDown(3));
                        }
                    }
                }

                protected override Dictionary<Resource, float> GetCost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 0 },
                        {Resource.AP,0 }
                    };
                    foreach (var p in Country.Provinces)
                    {
                        if (p.Coordinates == Country.Capital) cost[Resource.Gold] += 5;
                        cost[Resource.Gold] += 10;
                        cost[Resource.AP] += 0.1f;
                    }
                    return cost;
                }

                public override void Reject()
                {
                    foreach (var p in Country.Provinces)
                    {
                        p.AddStatus(new ProdDown(5));
                    }
                }
            }


			internal class TechnologicalBreakthrough : GlobalEvent
            {
                public TechnologicalBreakthrough(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, true) {}

                public override string Message { get { 
                        return "Your researchers have made a significant technological breakthrough. " +
                            "Do you wish to invest more resources into its development?"; 
                    } 
                }

                public override void Accept()
                {
                    base.Accept();
                    Country.Technologies[Technology.Economic] += 1;
                    Country.Technologies[Technology.Military] += 1;
                    Country.Technologies[Technology.Administrative] += 1;
                }

                protected override Dictionary<Resource, float> GetCost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 150 },
                        {Resource.AP,1 }
                    };
                    return cost;
                }

                public override void Reject()
                {
                    int randomTechnology = UnityEngine.Random.Range(0, 3);
                    Country.Technologies[(Technology)randomTechnology] += 1;
                }
            }

			internal class FloodEvent : GlobalEvent
            {
                public FloodEvent(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, false) {}
                public override string Message { get { 
                        return "The water levels have risen dramatically, flooding several provinces."; 
                    } 
                }

                public override void Accept()
                {
                    foreach(var p in Country.Provinces)
                    {
                        if (p.IsCoast) p.AddStatus(new FloodStatus(3));
                        else
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
                            {
                                p.AddStatus(new FloodStatus(3));
                            }
                        }
                    }
                    base.Accept();
                }
            }


			internal class FireEvent : GlobalEvent
            {
                public FireEvent(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, false) {}

                public override string Message { get { return "Severial provinces are on fire."; } }

                public override void Accept()
                {
                    foreach (var province in Country.Provinces)
                    {
                        if (province.ResourceType == Resource.Wood)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                            {
                                int duration = UnityEngine.Random.Range(1, 5);
                                province.AddStatus(new FireStatus(duration));
                            }
                        }
                    }
                    base.Accept();
                }
            }

			internal class Earthquake : GlobalEvent
            {
                public Earthquake(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, false) {}

                public override string Message { get { return "Severial provinces suffered from earthquake."; } }

                public override void Accept()
                {
                    foreach (var p in Country.Provinces)
                    {
                        p.DowngradeBuilding(BuildingType.Mine);
                        p.AddStatus(new Disaster(3));
                    }
                    base.Accept();
                }
            }


			internal class Misfortune : GlobalEvent
            {
                public Misfortune(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera, false) {}

                public override string Message { get { return "It seems you have angered the gods."; } }

                public override void Accept()
                {
                    foreach (var p in Country.Provinces)
                    {
                        if(UnityEngine.Random.Range(0f,1f) < 0.5f)
                        {
                            int duration = UnityEngine.Random.Range(1,5);
                            p.AddStatus(new Disaster(duration));
                        }
                    }
                    base.Accept();
                }
            }
        }

        public class LocalEvent : Event_ {
            public Province Province { get; set; }
            protected dialog_box_manager dialog_box;
            protected camera_controller camera;

            public LocalEvent(Province province, dialog_box_manager dialog_box, 
                camera_controller camera, bool isRejectable) {
                Province = province;
                this.dialog_box = dialog_box;
                Cost = GetCost();
                this.camera = camera;
                IsRejectable = isRejectable;
            }

            public override void Accept() {}

            public override void Reject() { Accept(); }

            public override string Message { get { return ""; } }

            public override void Zoom() {
                camera.ZoomCameraOnProvince(Province);
            }

            public override void Call() {
                var cost = this.GetCost();
                dialog_box.InvokeEventBox(this);
            }

            protected virtual Dictionary<Resource, float> GetCost() {
                return null;
            }


            internal class ProductionBoom : LocalEvent {
                public ProductionBoom(Province province, dialog_box_manager dialog, camera_controller camera) : 
                    base(province, dialog , camera, true) {}

                public override string Message { get { 
                        return "Work enthusiasm has increased in " + Province.Name + 
                            ". Should you use it now or invest for future?"; 
                    } 
                }
                
                public override void Accept() {
                    base.Accept();
                    Province.AddStatus(new ProdBoom(5));
                }

                public override void Reject() {
                    Province.ResourceAmount += 0.2f;
                }
            }


			internal class GoldRush : LocalEvent {
                public override string Message { get { return Province.Name + " is experiencing a gold rush!"; } }

                public GoldRush(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, false) {}

                public override void Accept() {
                    Province.AddStatus(new ProdBoom(6));
                }
            }


			internal class BonusRecruits : LocalEvent {
                public BonusRecruits(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, false) {}

                public override string Message { get { return "More recruits started appearing in " + Province.Name; } }

                public override void Accept() {
                    Province.RecruitablePopulation += (int)(Province.Population * 0.05);
                    Province.AddStatus(new RecBoom(4));
                }
            }


			internal class WorkersStrike1 : LocalEvent // turmoil mass migration
            {
                public WorkersStrike1(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, true) {}

                public override string Message { get { return "Workers are displeased with their workplace in " + Province.Name 
                            + ". Do you want to help them?"; } }

                protected override Dictionary<Resource, float> GetCost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 50 },
                        {Resource.AP, 0.1f }
                    };
                    return cost;
                }

                public override void Accept()
                {
                    base.Accept();
                }

                public override void Reject()
                {
                    if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                    {
                        Province.AddStatus(new ProdDown(3));
                    }
                }
            }


			internal class WorkersStrike2 : LocalEvent
            {
                public WorkersStrike2(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, true) {}

                public override string Message { get { 
                        return "Workers in " + Province.Name + " want fewer work hours for a few days. Will you agree?"; 
                    } 
                }
               
                public override void Accept()
                {
                    base.Accept();
                    Province.AddStatus(new ProdDown(5));
                    Province.Happiness += 10;
                }

                public override void Reject()
                {
                    Province.Happiness -= 30;
                }
            }


            internal class WorkersStrike3 : LocalEvent
            {
                private Map map;
                public WorkersStrike3(Province province, dialog_box_manager dialog_box, 
                    camera_controller camera, Map map) : base(province, dialog_box, camera, true)
                {
                    this.map = map;
                }

                public override string Message { get { 
                        return "Displeased workers formed armed movement. " +
                            "They demand lower taxes or they will destroy their workplace in " + Province.Name 
                            + ". Will you listen to their threats?"; 
                    } 
                }

                public override void Accept()
                {
                    base.Accept();
                    Province.AddStatus(new TaxBreak(3));
                    Province.Happiness += 10;
                }

                public override void Reject()
                {
                    Province.AddStatus(new ProdDown(5));
                    Army strikeArmy = new(0, UnityEngine.Random.Range(10, 50), Province.Coordinates, Province.Coordinates);
                    map.AddArmy(strikeArmy);
                }
            }


            internal class PlagueFound : LocalEvent
            {
                public PlagueFound(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, true) {}

                public override string Message { get { 
                        return "Your scientists suspect that the population in " 
                            + Province.Name + " is suffering from an unknown plague. " +
                            "Will you banish those showing symptoms?"; 
                    }
                }

                public override void Accept()
                {
                    float percent = UnityEngine.Random.Range(5f, 10f) / 100;
                    Province.Population -= (int)(Province.Population * percent);
                    Province.Happiness -= 10;
                    base.Accept();
                }

                public override void Reject()
                {
                    if (UnityEngine.Random.Range(0f,1f) < 0.2f)
                    {
                        Province.AddStatus(new Illness(8));
                    }
                }
            }


            internal class DisasterEvent : LocalEvent
            {
                public DisasterEvent(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, false) {}

                public override string Message
                {
                    get { return "A severe disaster has struck the province of " + Province.Name + "."; }
                }

                public override void Accept()
                {
                    base.Accept();

                    Province.AddStatus(new Disaster(5));
                    Province.Happiness -= 10;
                }
            }


            internal class StrangeRuins1 : LocalEvent
            {
                private Map map;
                public StrangeRuins1(Province province, dialog_box_manager dialog_box, 
                    camera_controller camera, Map map) : base(province, dialog_box, camera, false)
                {
                    this.map = map;
                }

                public override string Message
                {
                    get { return "The local found some gold."; }
                }

                public override void Accept()
                {
                    base.Accept();
                    // Resource allocation
                    float gold = UnityEngine.Random.Range(0f, 5f); // From 0 to 5 gold

                    Country country = map.Countries.FirstOrDefault(c => c.Id == Province.OwnerId);
                    country.ModifyResource(Resource.Gold, gold);
                }
            }


            internal class StrangeRuins2 : LocalEvent 
            {
                public StrangeRuins2(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera, true) {}

                public override string Message
                {
                    get { return "The local found a storage full of wine. Do you want to make use of it?"; }
                }

                public override void Accept()
                {
                    base.Accept();
                    Province.AddStatus(new Festivities(4));
                    if (UnityEngine.Random.Range(0f,1f) < 0.5f)
                    {
                        Province.AddStatus(new Illness(2));
                    }
                }

                public override void Reject() {}
            }
        }

        public class DiploEvent : Event_ {
            public Country From {  get; set; }
            public Country To { get; set; }
            private diplomatic_relations_manager diplomacy;
            private camera_controller camera;
            private dialog_box_manager dialog_box;

            public override string Message { get { return ""; } }
            
            public override void Zoom() {
                camera.ZoomCameraOnCountry(From);
            }

            public override void Call() {
                dialog_box.InvokeEventBox(this);
            }

            DiploEvent(Country from, Country to, diplomatic_relations_manager diplomacy, 
                dialog_box_manager dialog_box, camera_controller camera, bool isRejectable) {
                From = from;
                To = to;
                this.diplomacy = diplomacy;
                this.camera = camera;
                this.dialog_box = dialog_box;
                Cost = null;
                IsRejectable = isRejectable;
            }

			internal class WarDeclared : DiploEvent {
				public WarDeclared(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {
				}

				public override string Message { get { return From.Name + " has declared a war on you!"; } }
			}

			internal class PeaceOffer : DiploEvent {
                private Country offer;
                private Relation.War war;

                public PeaceOffer(Relation.War war, Country offer, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(offer, offer == war.Sides[0] 
                        ? war.Sides[1] : war.Sides[0], diplomacy, dialog_box, camera, true) { 
                    this.offer = offer;
                    this.war = war;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override string Message { get { 
                        return (offer == war.Sides[0] ? war.Sides[1].Name : war.Sides[0].Name) + " has offered peace."; 
                    } 
                }

                public override void Accept() {
                    diplomacy.EndRelation(war);
                }

                public override void Reject() {
                    base.Reject();
                }
            }

            internal class CallToWar : DiploEvent {
                public Relation.War War { get; private set; }

                public CallToWar(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, Relation.War war, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {
                    War = war;
                }

                public override string Message { get { 
                        return From.Name + " has called you to war against " 
                            + (War.Sides[0] == From ? War.Sides[0].Name : War.Sides[1].Name); 
                    } 
                }

                public override void Accept() {
                    diplomacy.JoinWar(War, To, From);
                }

                public override void Reject() {
                    diplomacy.DeclineWar(To, From);
                }
            }

            internal class TruceEnd : DiploEvent {
                public TruceEnd(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return "A truce with " + From.Name + " has ended"; } }
            }

            internal class AllianceOffer : DiploEvent {
                public AllianceOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {}

                public override string Message { get { return From.Name + " has sent you an alliance offer"; } }

                public override void Accept() {
                    base.Accept();
                    From.Events.Add(new AllianceAccepted(To, From, diplomacy, dialog_box, camera));
                    diplomacy.StartAlliance(To, From);
                }

                public override void Reject() {
                    From.Events.Add(new AllianceDenied(To, From, diplomacy, dialog_box, camera));
                }
            }

            internal class AllianceAccepted : DiploEvent {
                public AllianceAccepted(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return From.Name + " has accepted the alliance offer"; } }
            }

            internal class AllianceDenied : DiploEvent {
                public AllianceDenied(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return From.Name + " has denied our alliance offer"; } }
            }

            internal class AllianceBroken : DiploEvent {
                public AllianceBroken(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return From.Name + " has broken our pact"; } }
            }

            internal class SubsOffer : DiploEvent {
                public int Amount { get; set; }
                public int Duration { get; set; }

                public SubsOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, int amount, int duration, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {
                    Duration = duration;
                    Amount = amount;
                }

                public override string Message { get { return From.Name + " has offered you subsidies."; } }

                public override void Accept() {
                    base.Accept();
                    if(Duration != 0) {
                        diplomacy.StartSub(From, To, Amount, false, Duration);
                    }
                    else {
                        diplomacy.StartSub(From, To, Amount);
                    }
                }

                public override void Reject() {
                    base.Reject();
                }
            }

            internal class SubsRequest : DiploEvent {
                public int Amount {  get; set; }
                public int Duration { get; set; }

                public SubsRequest(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, int amount, int duration, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {
                    Amount = amount;
                    Duration = duration;
                }

                public override string Message { get { 
                        return From.Name + " has requested our subsidies of " 
                            + Amount + " gold for " + Duration + " turns"; 
                    } 
                }

                public override void Accept() {
                    base.Accept();
                    if (Duration != 0) { 
                        diplomacy.StartSub(To, From, Amount, false, Duration);
                    }
                    else {
                        diplomacy.StartSub(To, From, Amount);
                    }
                }
                public override void Reject() {
                    base.Reject();
                }
            }

            internal class SubsEndMaster : DiploEvent {
                public SubsEndMaster(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return "Subsidies from " + From.Name + " have stopped."; } }
            }

            internal class SubsEndSlave : DiploEvent {
                public SubsEndSlave(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return "We ended subsidizing " + To.Name; } }
            }

            internal class AccessOffer : DiploEvent {
                public AccessOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {
                    From = from;
                    To = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override string Message { get { return From.Name + " offers access to their territory"; } }

                public override void Accept() {
                    diplomacy.StartAccess(From, To);
                }

                public override void Reject() {
                    base.Reject();
                }
            }

            internal class AccessRequest : DiploEvent {
                public AccessRequest(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {}

                public override string Message { get { return From.Name + " asks for military access to our teritorry"; } }

                public override void Accept() {
                    diplomacy.StartAccess(To, From);
                }

                public override void Reject() {
                    base.Reject();
                }
            }

            internal class AccessEndMaster : DiploEvent {
                public Relation.MilitaryAccess Access { get; set; }

                public AccessEndMaster(Relation.MilitaryAccess access, Country from, Country to, 
                    diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {
                    Access = access;
                }

                public override string Message { get { return From.Name + " has stopped giving us the military access"; } }

                public override void Accept() {
                    diplomacy.EndRelation(Access);
                }
            }

            internal class AccessEndSlave : DiploEvent {
                public Relation.MilitaryAccess Access { get; set; }

                public AccessEndSlave(Relation.MilitaryAccess access, Country from, Country to, 
                    diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, false) {
                    Access = access;
                }

                public override string Message { get { return From.Name + " stopped using our military access"; } }

                public override void Accept() {
                    diplomacy.EndRelation(Access);
                }
            }

            internal class VassalOffer : DiploEvent {
                public VassalOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera, true) {}

                public override string Message { get { return From.Name + " demands our submission"; } }

                public override void Accept() {
                    diplomacy.StartVassalage(From, To);
                }

                public override void Reject() {
                    diplomacy.StartWar(From, To);
                }
            }

            internal class VassalRebel:DiploEvent {
                public VassalRebel(Country from, Country to, diplomatic_relations_manager diplomacy,
                    dialog_box_manager dialog_box, camera_controller camera) :
                    base(from, to, diplomacy, dialog_box, camera, false) {}

                public override string Message { get { return From.Name + " has rebelled against us"; } }
            }
        }
    }
}
