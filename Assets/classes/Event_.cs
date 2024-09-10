using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using static dialog_box_manager.dialog_box_precons;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace Assets.classes {
    public class Event_ {
        public virtual void call() { }
        public class GlobalEvent:Event_ {
            protected Country country;
            protected dialog_box_manager dialog_box;
            public Dictionary<Resource, float> Cost;
            public GlobalEvent(Country country, dialog_box_manager dialog) {
                this.country = country;
                this.dialog_box = dialog;
                this.Cost = cost();
            }

            public virtual void accept() {
                country.modifyResources(cost(), false);
            }
            public virtual void reject() { accept(); }
            public virtual string msg { get { return ""; } }
            public override void call() {
                dialog_box.invokeConfirmBox("", msg, accept, reject, this.Cost);
            }
            public void zoom() {
                // Implement zoom to capital or whatever, using the country object
            }

            protected virtual Dictionary<Resource, float> cost() {
                return null;
            }

            internal class Discontent:GlobalEvent {
                public Discontent(Country country, dialog_box_manager dialog) : base(country, dialog) { }
                public override string msg { get { return "A discontent has spread in the country. You can bribe officials to lower its impact"; } }
                public override void accept() {
                    base.accept();
                    foreach(var p in country.Provinces) {
                        if(p.coordinates != country.Capital) p.Happiness -= 5;
                        p.Happiness -= 5;
                    }
                }

                public override void reject() {
                    foreach(var p in country.Provinces) {
                        if(p.coordinates != country.Capital) p.Happiness -= 10;
                        p.Happiness -= 20;
                    }
                }

                protected override Dictionary<Resource, float> cost() {
                    var cost = new Dictionary<Resource, float> {
                    { Resource.Gold, 0 },
                    { Resource.AP, 0 }
                };
                    foreach(var p in country.Provinces) {
                        cost[Resource.Gold] += (float)Math.Round(25 * p.Population / 500f, 1);
                        cost[Resource.AP] += 0.2f;
                    }
                    return cost;
                }
            }

            internal class Happiness:GlobalEvent {
                public Happiness(Country country, dialog_box_manager dialog) : base(country, dialog) { }
                public override string msg { get { return "Happiness has increased in the country"; } }
                public override void accept() {
                    base.accept();
                    foreach(var p in country.Provinces) {
                        p.Happiness += 10;
                    }
                }

                public override void reject() {
                    this.accept();
                }
            }
            internal class Plague:GlobalEvent {
                public Plague(Country country, dialog_box_manager dialog) : base(country, dialog) {
                }

                public override string msg { get { return "Plague has struck the nation. You may pay to your researchers to quicken the cure's invention."; } }

                public override void accept() {
                    base.accept();
                }

                public override void call() {
                    base.call();
                }

                protected override Dictionary<Resource, float> cost() {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 0 }
                    };
                    foreach(var p in country.Provinces) {
                        if(p.coordinates == country.Capital) cost[Resource.Gold] += 15;
                        cost[Resource.Gold] += 30;
                    }
                    return cost;
                }

                public override void reject() {
                    base.reject();
                }
            }
            internal class EconomicReccesion : GlobalEvent
            {
                public EconomicReccesion(Country country, dialog_box_manager dialog) : base(country, dialog)
                {
                }

                public override string msg { get { return "The global economy has entered a downturn. Your nation's production is suffering as a result. Will you attempt to fight the reccesion by paying a cost in gold and ap?"; } }

                public override void accept()
                {
                    base.accept();
                    foreach(var p in country.Provinces)
                    {
                        if (UnityEngine.Random.Range(0f,1f) < 0.05f)
                        {
                            p.addStatus(new ProdDown(3));
                        }
                    }
                }

                public override void call()
                {
                    base.call();
                }

                protected override Dictionary<Resource, float> cost()
                {
                    var cost = new Dictionary<Resource, float> {
                        {Resource.Gold, 0 },
                        {Resource.AP,0 }
                    };
                    foreach (var p in country.Provinces)
                    {
                        if (p.coordinates == country.Capital) cost[Resource.Gold] += 5;
                        cost[Resource.Gold] += 10;
                        cost[Resource.AP] += 0.1f;
                    }
                    return cost;
                }

                public override void reject()
                {
                    base.reject();
                    foreach (var p in country.Provinces)
                    {
                        p.addStatus(new ProdDown(5));
                    }
                }
            }
            internal class TechnologicalBreakthrough : GlobalEvent
            {
                public TechnologicalBreakthrough(Country country, dialog_box_manager dialog) : base(country, dialog)
                {
                }
                public override string msg { get { return "Your researchers have made a significant technological breakthrough. Do you wish to invest more resources into its development?"; } }

                public override void accept()
                {
                    // propozycja ulepszenie o 1 kazdej techologii.
                    base.accept();
                }

                public override void call()
                {
                    base.call();
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
                    // ulepszenie losowej technologii albo SP?
                    base.reject();
                }
            }
            internal class Flood1:GlobalEvent
            {
                public Flood1(Country country, dialog_box_manager dialog) : base(country, dialog)
                {
                }
                public override string msg { get { return "The water levels have risen dramatically, flooding several provinces."; } }

                public override void accept()
                {
                    foreach(var p in country.Provinces)
                    {
                        if(p.Is_coast) { p.addStatus(new Flood(3)); }
                    }
                    base.accept();
                }

                public override void call()
                {
                    base.call();
                }
                public override void reject()
                {
                    base.reject();
                }
            }
            internal class Fire1 : GlobalEvent
            {
                public Fire1(Country country, dialog_box_manager dialog) : base(country, dialog)
                {
                }
                public override string msg { get { return "Severial provinces are on fire."; } }

                public override void accept()
                {
                    foreach (var p in country.Provinces)
                    {
                        if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                        {
                            int duration = UnityEngine.Random.Range(1, 5);
                            p.addStatus(new Fire(duration));
                        }
                    }
                    base.accept();
                }
                public override void call()
                {
                    base.call();
                }
                public override void reject()
                {
                    base.reject();
                }
            }
            internal class Earthquake : GlobalEvent
            {
                public Earthquake(Country country, dialog_box_manager dialog) : base(country, dialog)
                {
                }
                public override string msg { get { return "Severial provinces suffered from earthquake."; } }

                public override void accept()
                {
                    foreach (var p in country.Provinces)
                    {
                        var mine = p.Buildings.Find(b => b.BuildingType == BuildingType.Mine);
                        mine.Downgrade();
                        p.addStatus(new Disaster(3));
                    }
                    base.accept();
                }
                public override void call()
                {
                    base.call();
                }
                public override void reject()
                {
                    base.reject();
                }
            }

            internal class Misfortune : GlobalEvent
            {
                public Misfortune(Country country, dialog_box_manager dialog) : base(country, dialog)
                {
                }
                public override string msg { get { return "It seems you have angered the gods."; } }

                public override void accept()
                {
                    foreach (var p in country.Provinces)
                    {
                        if(UnityEngine.Random.Range(0f,1f) < 0.5f)
                        {
                            int duration = UnityEngine.Random.Range(1,5);
                            p.addStatus(new Disaster(duration));
                        }
                    }
                    base.accept();
                }

                public override void call()
                {
                    base.call();
                }
                public override void reject()
                {
                    base.reject();
                }
            }
        }
        public class LocalEvent:Event_ {
            protected Province province;
            protected Map map;
            protected dialog_box_manager dialog_box;
            public Dictionary<Resource, float> Cost;
            public LocalEvent(Province province, dialog_box_manager dialog_box) {
                this.province = province;
                this.dialog_box = dialog_box;
                this.Cost = cost();
            }
            public virtual void accept() { /*tbd*/}
            public virtual void reject() { accept(); }
            public virtual string msg { get { return ""; } }
            public void zoom() {
                // Implement zoom to province or whatever
            }
            public override void call() {
                var cost = this.cost();
                dialog_box.invokeConfirmBox("", msg, accept, reject, Cost);
            }

            protected virtual Dictionary<Resource, float> cost() {
                return null;
            }

            internal class ProductionBoom1:LocalEvent {
                public ProductionBoom1(Province province, dialog_box_manager dialog) : base(province, dialog) { }
                public override string msg { get { return "Work enthusiasm has increased in " + province.Name + ". Should you use it now or invest for future."; } }
                public override void accept() {
                    base.accept();
                    province.addStatus(new ProdBoom(5));
                }

                public override void reject() {
                    province.Resources_amount += 0.2f;
                }
            }
            internal class GoldRush:LocalEvent {
                public override string msg { get { return province.Name + " is experiencing a gold rush!"; } }

                public GoldRush(Province province, dialog_box_manager dialog_box) : base(province, dialog_box) {
                        
                }

                public override void call() {
                    base.call();
                }

                public override void accept() {
                    province.addStatus(new ProdBoom(6));
                }

                public override void reject() {
                    base.reject();
                }
            }
            internal class BonusRecruits:LocalEvent {
                public BonusRecruits(Province province, dialog_box_manager dialog_box) : base(province, dialog_box) {
                }

                public override string msg { get { return "More recruits started appearing in " + province.Name; } }

                public override void accept() {
                    province.RecruitablePopulation += (int)(province.Population * 0.05);
                    province.addStatus(new RecBoom(4));
                }

                public override void call() {
                    base.call();
                }

                public override void reject() {
                    base.reject();
                }
            }
            internal class WorkersStrike1:LocalEvent
            {
                public WorkersStrike1(Province province, dialog_box_manager dialog_box) : base(province, dialog_box) { 
                }
                public override string msg { get { return "Workers are displeased with their workplace in " + province.Name + ". Do you want to help them?"; } }
                public override void call()
                {
                    base.call();
                }
                public override void accept()
                {
                    base.accept();
                    // dodac koszt
                }
                public override void reject()
                {
                    base.reject();
                    if (UnityEngine.Random.Range(0, 1f) < 0.5f)
                    {
                        province.addStatus(new ProdDown(3));
                    }
                }
            }
            internal class WorkersStrike2 : LocalEvent
            {
                public WorkersStrike2(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }
                public override string msg { get { return "Workers in " + province.Name + " want fewer work hours for a few days. Will you agree?"; } }
                public override void call()
                {
                    base.call();
                }
                public override void accept()
                {
                    base.accept();
                    province.addStatus(new ProdDown(5));
                    province.Happiness += 10;
                }
                public override void reject()
                {
                    base.reject();
                    province.Happiness -= 30;
                }
            }
            internal class WorkersStrike3 : LocalEvent
            {
                public WorkersStrike3(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }
                public override string msg { get { return "Displeased workers formed armed movement. They demand lower taxes or they will destroy their workplace in " + province.Name+ ". Will you listen to their threats?"; } }
                public override void call()
                {
                    base.call();
                }
                public override void accept()
                {
                    base.accept();
                    province.addStatus(new TaxBreak(3));
                    province.Happiness += 10;
                }
                public override void reject()
                {
                    base.reject();
                    province.addStatus(new ProdDown(5));

                    Army strikeArmy = new Army(0, UnityEngine.Random.Range(10, 50), province.coordinates, province.coordinates);
                    map.addArmy(strikeArmy);
                }
            }
            internal class PlagueFound : LocalEvent
            {
                public PlagueFound(Province province, dialog_box_manager dialog_box) : base(province, dialog_box) { }
                public override string msg { get { return "Your scientists suspect that the population in " + province.Name + " is suffering from an unknown plague. Will you banish those showing symptoms?"; } }
                public override void call()
                {
                    base.call();
                }
                public override void accept()
                {
                    float percent = UnityEngine.Random.Range(5f, 10f) / 100;
                    province.Population -= (int)(province.Population * percent);
                    province.Happiness -= 10;
                    base.accept();
                }
                public override void reject()
                {
                    base.reject();
                    if(UnityEngine.Random.Range(0f,1f) < 0.2f)
                    {
                        province.addStatus(new Illness(8));
                    }
                }
            }
            internal class StrangeRuins : LocalEvent
            {
                public StrangeRuins(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "One of the locals found strange ruins in " + province.Name + ". Do you wish to explore them?"; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins1 : StrangeRuins
            {
                public StrangeRuins1(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local found an abandoned storage room."; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                    // Przyznanie zasobów
                    float gold = UnityEngine.Random.Range(0f, 20f); // od 0 do 20 złota
                    float wood = UnityEngine.Random.Range(0f, 30f); // od 0 do 30 drewna
                    float iron = UnityEngine.Random.Range(0f, 30f); // od 0 do 30 żelaza
                    Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
                    country.modifyResource(Resource.Gold, gold);
                    country.modifyResource(Resource.Wood, gold);
                    country.modifyResource(Resource.Iron, gold);
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins2 : StrangeRuins
            {
                public StrangeRuins2(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "Ruins seems to be already expolred. You didn't find anythinf usefull."; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins3 : StrangeRuins
            {
                public StrangeRuins3(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local found some gold."; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                    // Przyznanie zasobów
                    float gold = UnityEngine.Random.Range(0f, 5f); // od 0 do 5 złota

                    Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
                    country.modifyResource(Resource.Gold, gold);
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins4 : StrangeRuins
            {
                public StrangeRuins4(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local found some iron weapons.They are not in the best state but can they can be at least smelted."; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                    // Przyznanie zasobów
                    float iron = UnityEngine.Random.Range(0f, 20f); // od 0 do 20 żelaza
                    Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
                    country.modifyResource(Resource.Iron, iron);
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins5 : StrangeRuins
            {
                public StrangeRuins5(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local encountered a pack of wolves."; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();

                    Army army = new Army(0, UnityEngine.Random.Range(2, 5), province.coordinates, province.coordinates);
                    map.addArmy(army);
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins6 : StrangeRuins
            {
                public StrangeRuins6(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local awaken guardians of the ruins."; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                    Army army = new Army(0, UnityEngine.Random.Range(10, 20), province.coordinates, province.coordinates);
                    map.addArmy(army);
                }

                public override void reject()
                {
                    base.reject();
                }
            }
            internal class StrangeRuins7 : StrangeRuins
            {
                public StrangeRuins7(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local found a mysteroius artifact. You want to sell it?"; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                    float gold = UnityEngine.Random.Range(50f, 200f); // od 50 do 200 złota
                    Country country = map.Countries.FirstOrDefault(c => c.Id == province.Owner_id);
                    country.modifyResource(Resource.Gold, gold);
                }

                public override void reject()
                {
                    base.reject();
                    // chce dodać tutaj dodatkowy modifier do army power
                }
            }
            internal class StrangeRuins8 : StrangeRuins
            {
                public StrangeRuins8(Province province, dialog_box_manager dialog_box) : base(province, dialog_box)
                {
                }

                public override string msg
                {
                    get { return "The local found a storage full of wine. Do you want to make use of it?"; }
                }

                public override void call()
                {
                    base.call();
                }

                public override void accept()
                {
                    base.accept();
                    province.addStatus(new Festivities(4));
                    if (UnityEngine.Random.Range(0f,1f) < 0.5f)
                    {
                        province.addStatus(new Illness(2));
                    }
                }

                public override void reject()
                {
                    base.reject();
                }
            }
        }
        public class DiploEvent:Event_ {

        }
    }
    
}
