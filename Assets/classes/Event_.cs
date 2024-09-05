using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                dialog_box.invokeConfirmBox("", msg, accept, reject);
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
                    this.accept(); // Calls accept on the same country
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
        }
        public class LocalEvent:Event_ {
            protected Province province;
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
                dialog_box.invokeConfirmBox("", msg, accept, reject);
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
        }
        public class DiploEvent:Event_ {

        }
    }
    
}
