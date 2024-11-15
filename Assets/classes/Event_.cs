using Assets.classes.subclasses;
using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.classes {
    [Serializable]
    public class Event_ {
        public Dictionary<Resource, float> Cost { get; set; }
        public virtual void call() {}
        public virtual void accept() {}
        public virtual void reject() {}
        public virtual void zoom() {}
        public virtual string Message { get { return ""; } }

        public class GlobalEvent : Event_ {
            public Country Country { get; set; }
            protected dialog_box_manager dialog_box;
            protected camera_controller camera;

            public GlobalEvent(Country country, dialog_box_manager dialog, camera_controller camera) {
                Country = country;
                this.camera = camera;
                dialog_box = dialog;
                Cost = cost();
            }

            public override void accept() {
                Country.modifyResources(cost(), false);
            }

            public override void reject() { accept(); }

            public override void call() {
                dialog_box.invokeEventBox(this);
            }

            public override void zoom() {
                camera.ZoomCameraOnCountry(Country);
            }

            protected virtual Dictionary<Resource, float> cost() {
                return null;
            }

            //id=0
            internal class Discontent : GlobalEvent {
                public Discontent(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { 
                        return "A discontent has spread in the country. You can bribe officials to lower its impact"; 
                    } 
                }

                public override void accept() {
                    base.accept();
                    foreach (var p in Country.Provinces) {
                        if(p.coordinates != Country.Capital) p.Happiness -= 5;
                        p.Happiness -= 5;
                    }
                }

                public override void reject() {
                    foreach (var p in Country.Provinces) {
                        if(p.coordinates != Country.Capital) p.Happiness -= 10;
                        p.Happiness -= 20;
                    }
                }

                protected override Dictionary<Resource, float> cost() {
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

			//id=1
			internal class Happiness : GlobalEvent {
                public Happiness(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { 
                        return "Happiness has increased in the country"; 
                    } 
                }

                public override void accept() {
                    base.accept();
                    foreach (var p in Country.Provinces) {
                        p.Happiness += 10;
                    }
                }
            }

			//id=2
			internal class Plague : GlobalEvent {
                public Plague(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {
                }

                public override string Message { get { 
                        return "Plague has struck the nation. You may pay to your researchers to quicken the cure's invention."; 
                    } 
                }

                public override void accept() {
                    base.accept();
                    foreach (var p in Country.Provinces)
                    {
                        p.addStatus(new Illness(2));
                    }
                }

                protected override Dictionary<Resource, float> cost() {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 0 }
                    };
                    foreach (var p in Country.Provinces) {
                        if (p.coordinates == Country.Capital) cost[Resource.Gold] += 15;
                        cost[Resource.Gold] += 30;
                    }
                    return cost;
                }

                public override void reject() {
                    base.reject();
                    foreach (var p in Country.Provinces)
                    {
                        p.addStatus(new Illness(5));
                    }
                }
            }

			//id=3
			internal class EconomicRecession : GlobalEvent
            {
                public EconomicRecession(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { 
                        return "The global economy has entered a downturn. Your nation's production is suffering as a result. " +
                            "Will you attempt to fight the reccesion by paying a cost in gold and AP?"; 
                    } 
                }

                public override void accept()
                {
                    base.accept();
                    foreach(var p in Country.Provinces)
                    {
                        if (UnityEngine.Random.Range(0f,1f) < 0.05f)
                        {
                            p.addStatus(new ProdDown(3));
                        }
                    }
                }

                protected override Dictionary<Resource, float> cost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 0 },
                        {Resource.AP,0 }
                    };
                    foreach (var p in Country.Provinces)
                    {
                        if (p.coordinates == Country.Capital) cost[Resource.Gold] += 5;
                        cost[Resource.Gold] += 10;
                        cost[Resource.AP] += 0.1f;
                    }
                    return cost;
                }

                public override void reject()
                {
                    base.reject();
                    foreach (var p in Country.Provinces)
                    {
                        p.addStatus(new ProdDown(5));
                    }
                }
            }

			//id=4
			internal class TechnologicalBreakthrough : GlobalEvent
            {
                public TechnologicalBreakthrough(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { 
                        return "Your researchers have made a significant technological breakthrough. " +
                            "Do you wish to invest more resources into its development?"; 
                    } 
                }

                public override void accept()
                {
                    base.accept();
                    Country.Technologies[Technology.Economic] += 1;
                    Country.Technologies[Technology.Military] += 1;
                    Country.Technologies[Technology.Administrative] += 1;
                }

                protected override Dictionary<Resource, float> cost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 150 },
                        {Resource.AP,1 }
                    };
                    return cost;
                }

                public override void reject()
                {
                    base.reject();
                    int randomTechnology = UnityEngine.Random.Range(0, 3);
                    Country.Technologies[(Technology)randomTechnology] += 1;
                }
            }

			//id=5
			internal class FloodEvent : GlobalEvent
            {
                public FloodEvent(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}
                public override string Message { get { 
                        return "The water levels have risen dramatically, flooding several provinces."; 
                    } 
                }

                public override void accept()
                {
                    foreach(var p in Country.Provinces)
                    {
                        if (p.Is_coast) p.addStatus(new FloodStatus(3));
                        else
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
                            {
                                p.addStatus(new FloodStatus(3));
                            }
                        }
                    }
                    base.accept();
                }
            }

			//id=6
			internal class FireEvent : GlobalEvent
            {
                public FireEvent(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { return "Severial provinces are on fire."; } }

                public override void accept()
                {
                    foreach (var province in Country.Provinces)
                    {
                        if (province.ResourcesT == Resource.Wood)
                        {
                            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                            {
                                int duration = UnityEngine.Random.Range(1, 5);
                                province.addStatus(new FireStatus(duration));
                            }
                        }
                    }
                    base.accept();
                }
            }

			//id=7
			internal class Earthquake : GlobalEvent
            {
                public Earthquake(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { return "Severial provinces suffered from earthquake."; } }

                public override void accept()
                {
                    foreach (var p in Country.Provinces)
                    {
                        var mine = p.Buildings.Find(b => b.BuildingType == BuildingType.Mine);
                        mine.Downgrade();
                        p.addStatus(new Disaster(3));
                    }
                    base.accept();
                }
            }

			//id=8
			internal class Misfortune : GlobalEvent
            {
                public Misfortune(Country country, dialog_box_manager dialog, camera_controller camera) : 
                    base(country, dialog, camera) {}

                public override string Message { get { return "It seems you have angered the gods."; } }

                public override void accept()
                {
                    foreach (var p in Country.Provinces)
                    {
                        if(UnityEngine.Random.Range(0f,1f) < 0.5f)
                        {
                            int duration = UnityEngine.Random.Range(1,5);
                            p.addStatus(new Disaster(duration));
                        }
                    }
                    base.accept();
                }
            }
        }

        public class LocalEvent : Event_ {
            public Province Province { get; set; }
            protected dialog_box_manager dialog_box;
            protected camera_controller camera;

            public LocalEvent(Province province, dialog_box_manager dialog_box, camera_controller camera)
            {
                Province = province;
                this.dialog_box = dialog_box;
                Cost = cost();
                this.camera = camera;
            }

            public override void accept() {}

            public override void reject() { accept(); }

            public override string Message { get { return ""; } }

            public override void zoom() {
                camera.ZoomCameraOnProvince(Province);
            }

            public override void call() {
                var cost = this.cost();
                dialog_box.invokeEventBox(this);
            }

            protected virtual Dictionary<Resource, float> cost() {
                return null;
            }

            //id=0
            internal class ProductionBoom : LocalEvent {
                public ProductionBoom(Province province, dialog_box_manager dialog, camera_controller camera) : 
                    base(province, dialog , camera) {}

                public override string Message { get { 
                        return "Work enthusiasm has increased in " + Province.Name + 
                            ". Should you use it now or invest for future."; 
                    } 
                }
                
                public override void accept() {
                    base.accept();
                    Province.addStatus(new ProdBoom(5));
                }

                public override void reject() {
                    Province.Resources_amount += 0.2f;
                }
            }

			//id=1
			internal class GoldRush : LocalEvent {
                public override string Message { get { return Province.Name + " is experiencing a gold rush!"; } }

                public GoldRush(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override void accept() {
                    Province.addStatus(new ProdBoom(6));
                }
            }

			//id=2
			internal class BonusRecruits : LocalEvent {
                public BonusRecruits(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override string Message { get { return "More recruits started appearing in " + Province.Name; } }

                public override void accept() {
                    Province.RecruitablePopulation += (int)(Province.Population * 0.05);
                    Province.addStatus(new RecBoom(4));
                }
            }

			//id=3
			internal class WorkersStrike1 : LocalEvent // turmoil mass migration
            {
                public WorkersStrike1(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override string Message { get { return "Workers are displeased with their workplace in " + Province.Name 
                            + ". Do you want to help them?"; } }

                protected override Dictionary<Resource, float> cost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 50 },
                        {Resource.AP, 0.1f }
                    };
                    return cost;
                }

                public override void accept()
                {
                    base.accept();
                }

                public override void reject()
                {
                    base.reject();
                    if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                    {
                        Province.addStatus(new ProdDown(3));
                    }
                }
            }

			//id=4
			internal class WorkersStrike2 : LocalEvent
            {
                public WorkersStrike2(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override string Message { get { 
                        return "Workers in " + Province.Name + " want fewer work hours for a few days. Will you agree?"; 
                    } 
                }
               
                public override void accept()
                {
                    base.accept();
                    Province.addStatus(new ProdDown(5));
                    Province.Happiness += 10;
                }

                public override void reject()
                {
                    base.reject();
                    Province.Happiness -= 30;
                }
            }

            //id=5
            internal class WorkersStrike3 : LocalEvent
            {
                private Map map;
                public WorkersStrike3(Province province, dialog_box_manager dialog_box, 
                    camera_controller camera, Map map) : base(province, dialog_box, camera)
                {
                    this.map = map;
                }

                public override string Message { get { 
                        return "Displeased workers formed armed movement. " +
                            "They demand lower taxes or they will destroy their workplace in " + Province.Name 
                            + ". Will you listen to their threats?"; 
                    } 
                }

                public override void accept()
                {
                    base.accept();
                    Province.addStatus(new TaxBreak(3));
                    Province.Happiness += 10;
                }

                public override void reject()
                {
                    base.reject();
                    Province.addStatus(new ProdDown(5));
                    Army strikeArmy = new(0, UnityEngine.Random.Range(10, 50), Province.coordinates, Province.coordinates);
                    map.addArmy(strikeArmy);
                }
            }

            //id=6
            internal class PlagueFound : LocalEvent
            {
                public PlagueFound(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override string Message { get { 
                        return "Your scientists suspect that the population in " 
                            + Province.Name + " is suffering from an unknown plague. " +
                            "Will you banish those showing symptoms?"; 
                    }
                }

                public override void accept()
                {
                    float percent = UnityEngine.Random.Range(5f, 10f) / 100;
                    Province.Population -= (int)(Province.Population * percent);
                    Province.Happiness -= 10;
                    base.accept();
                }

                public override void reject()
                {
                    base.reject();
                    if(UnityEngine.Random.Range(0f,1f) < 0.2f)
                    {
                        Province.addStatus(new Illness(8));
                    }
                }
            }

            //id=7
            internal class DisasterEvent : LocalEvent
            {
                public DisasterEvent(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override string Message
                {
                    get { return "A severe disaster has struck the province of " + Province.Name + "."; }
                }

                public override void accept()
                {
                    base.accept();

                    Province.addStatus(new Disaster(5));
                    Province.Happiness -= 10;
                }
            }

            //id=8
            internal class StrangeRuins1 : LocalEvent
            {
                private Map map;
                public StrangeRuins1(Province province, dialog_box_manager dialog_box, 
                    camera_controller camera, Map map) : base(province, dialog_box, camera)
                {
                    this.map = map;
                }

                public override string Message
                {
                    get { return "The local found some gold."; }
                }

                public override void accept()
                {
                    base.accept();
                    // Resource allocation
                    float gold = UnityEngine.Random.Range(0f, 5f); // From 0 to 5 gold

                    Country country = map.Countries.FirstOrDefault(c => c.Id == Province.Owner_id);
                    country.modifyResource(Resource.Gold, gold);
                }
            }

            //id=9
            internal class StrangeRuins2 : LocalEvent 
            {
                public StrangeRuins2(Province province, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(province, dialog_box, camera) {}

                public override string Message
                {
                    get { return "The local found a storage full of wine. Do you want to make use of it?"; }
                }

                public override void accept()
                {
                    base.accept();
                    Province.addStatus(new Festivities(4));
                    if (UnityEngine.Random.Range(0f,1f) < 0.5f)
                    {
                        Province.addStatus(new Illness(2));
                    }
                }

                public override void reject()
                {
                    base.reject();
                }
            }
        }

        public class DiploEvent : Event_ {
            public Country From {  get; set; }
            public Country To { get; set; }
            private diplomatic_relations_manager diplomacy;
            private camera_controller camera;
            private dialog_box_manager dialog_box;

            public override string Message { get { return ""; } }
            
            public override void zoom() {
                camera.ZoomCameraOnCountry(From);
            }

            public override void call() {
                dialog_box.invokeEventBox(this);
            }

            DiploEvent(Country from, Country to, diplomatic_relations_manager diplomacy, 
                dialog_box_manager dialog_box, camera_controller camera) {
                From = from;
                To = to;
                this.diplomacy = diplomacy;
                this.camera = camera;
                this.dialog_box = dialog_box;
                Cost = null;
            }

			internal class WarDeclared : DiploEvent {
				public WarDeclared(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {
				}

				public override string Message { get { return From.Name + " has declared a war on you!"; } }
			}

			internal class PeaceOffer : DiploEvent {
                private Country offer;
                private Relation.War war;

                public PeaceOffer(Relation.War war, Country offer, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(offer, offer == war.Sides[0] ? war.Sides[1] : war.Sides[0], diplomacy, dialog_box, camera) { 
                    this.offer = offer;
                    this.war = war;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override string Message { get { 
                        return (offer == war.Sides[0] ? war.Sides[1].Name : war.Sides[0].Name) + " has offered peace."; 
                    } 
                }

                public override void accept() {
                    diplomacy.endRelation(war);
                }

                public override void reject() {}
            }

            internal class CallToWar : DiploEvent {
                public Relation.War War { get; private set; }

                public CallToWar(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, Relation.War war, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { 
                        return From.Name + " has called you to war against " 
                            + (War.Sides[0] == From ? War.Sides[0].Name : War.Sides[1].Name); 
                    } 
                }

                public override void accept() {
                    diplomacy.joinWar(War, To, From);
                }

                public override void reject() {
                    diplomacy.declineWar(To, From);
                }
            }

            internal class TruceEnd : DiploEvent {
                public TruceEnd(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return "A truce with " + From.Name + " has ended"; } }
            }

            internal class AllianceOffer : DiploEvent {
                public AllianceOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " has sent you an alliance offer"; } }

                public override void accept() {
                    base.accept();
                    From.Events.Add(new AllianceAccepted(To, From, diplomacy, dialog_box, camera));
                    diplomacy.startAlliance(To, From);
                }

                public override void reject() {
                    From.Events.Add(new AllianceDenied(To, From, diplomacy, dialog_box, camera));
                }
            }

            internal class AllianceAccepted : DiploEvent {
                public AllianceAccepted(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " has accepted the alliance offer"; } }
            }

            internal class AllianceDenied : DiploEvent {
                public AllianceDenied(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " has denied our alliance offer"; } }
            }

            internal class AllianceBroken : DiploEvent {
                public AllianceBroken(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " has broken our pact"; } }
            }

            internal class SubsOffer : DiploEvent {
                public int Amount { get; set; }
                public int Duration { get; set; }

                public SubsOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, int amount, int duration, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {
                    Duration = duration;
                    Amount = amount;
                }

                public override string Message { get { return From.Name + " has offered you subsidies."; } }

                public override void accept() {
                    base.accept();
                    if(Duration != 0) {
                        diplomacy.startSub(From, To, Amount, false, Duration);
                    }
                    else {
                        diplomacy.startSub(From, To, Amount);
                    }
                }

                public override void reject() {}
            }

            internal class SubsRequest : DiploEvent {
                public int Amount {  get; set; }
                public int Duration { get; set; }

                public SubsRequest(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, int amount, int duration, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {
                    Amount = amount;
                    Duration = duration;
                }

                public override string Message { get { 
                        return From.Name + " has requested our subsidies of " 
                            + Amount + " gold for " + Duration + " turns"; 
                    } 
                }

                public override void accept() {
                    base.accept();
                    if (Duration != 0) { 
                        diplomacy.startSub(To, From, Amount, false, Duration);
                    }
                    else {
                        diplomacy.startSub(To, From, Amount);
                    }
                }
                public override void reject() {}
            }

            internal class SubsEndMaster : DiploEvent {
                public SubsEndMaster(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return "Subsidies from " + From.Name + " have stopped."; } }
            }

            internal class SubsEndSlave : DiploEvent {
                public SubsEndSlave(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return "We ended subsidizing " + To.Name; } }
            }

            internal class AccessOffer : DiploEvent {
                public AccessOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {
                    From = from;
                    To = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override string Message { get { return From.Name + " offers access to their territory"; } }

                public override void accept() {
                    diplomacy.startAccess(From, To);
                }

                public override void reject() {}
            }

            internal class AccessRequest : DiploEvent {
                public AccessRequest(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " asks for military access to our teritorry"; } }

                public override void accept() {
                    diplomacy.startAccess(To, From);
                }

                public override void reject() {}
            }

            internal class AccessEndMaster : DiploEvent {
                public Relation.MilitaryAccess Access { get; set; }

                public AccessEndMaster(Relation.MilitaryAccess access, Country from, Country to, 
                    diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {
                    Access = access;
                }

                public override string Message { get { return From.Name + " has stopped giving us the military access"; } }

                public override void accept() {
                    diplomacy.endRelation(Access);
                }
            }

            internal class AccessEndSlave : DiploEvent {
                public Relation.MilitaryAccess Access { get; set; }

                public AccessEndSlave(Relation.MilitaryAccess access, Country from, Country to, 
                    diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {
                    Access = access;
                }

                public override string Message { get { return From.Name + " stopped using our military access"; } }

                public override void accept() {
                    diplomacy.endRelation(Access);
                }
            }

            internal class VassalOffer : DiploEvent {
                public VassalOffer(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " demands our submission"; } }

                public override void accept() {
                    diplomacy.startVassalage(From, To);
                }

                public override void reject() {
                    diplomacy.startWar(From, To);
                }
            }

            internal class VassalRebel:DiploEvent {
                public VassalRebel(Country from, Country to, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera) : 
                    base(from, to, diplomacy, dialog_box, camera) {}

                public override string Message { get { return From.Name + " has rebelled against us"; } }
            }
        }
    }
}
