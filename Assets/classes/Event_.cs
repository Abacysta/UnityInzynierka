using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static dialog_box_manager.dialog_box_precons;

namespace Assets.classes {
    public class Event_ {
        public virtual void call() { }
        public class GlobalEvent:Event_ {
            protected Country country;
            protected dialog_box_manager dialog_box;
            public GlobalEvent(Country country, dialog_box_manager dialog) {
                this.country = country;
                this.dialog_box = dialog;
            }

            public virtual void accept() {
                country.modifyResources(cost(), false);
            }
            public virtual void reject() { }
            public virtual string msg { get { return ""; } }
            public override void call() {
                dialog_box.invokeConfirmBox("", msg, accept, reject);
            }
            public void zoom() {
                // Implement zoom to capital or whatever, using the country object
            }

            public virtual Dictionary<Resource, float> cost() {
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

                public override Dictionary<Resource, float> cost() {
                    var cost = new Dictionary<Resource, float> {
                { Resource.Gold, 0 },
                { Resource.AP, 0 }
            };
                    foreach(var p in country.Provinces) {
                        cost[Resource.Gold] += (float)Math.Round((float)(25 * p.Population / 500), 1);
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

                public override Dictionary<Resource, float> cost() {
                    return base.cost();
                }

                public override void reject() {
                    this.accept(); // Calls accept on the same country
                }
            }
        }
        public class LocalEvent:Event_ {
            protected Province province;
            protected dialog_box_manager dialog_box;
            public LocalEvent(Province province, dialog_box_manager dialog_box) {
                this.province = province;
                this.dialog_box = dialog_box;
            }
            public virtual void accept() { }
            public virtual void reject() { }
            public virtual string msg { get { return ""; } }
            public void zoom() {
                // Implement zoom to province or whatever
            }
            public override void call() {
                dialog_box.invokeConfirmBox("", msg, accept, reject);
            }

            public virtual Dictionary<Resource, float> cost() {
                return null;
            }

            internal class ProductionBoom1:LocalEvent {
                public ProductionBoom1(Province province, dialog_box_manager dialog) : base(province, dialog) { }
                public override string msg { get { return "Work enthusiasm has increased in " + province.Name + ". Should you use it now or invest for future."; } }
                public override void accept() {
                    base.accept();
                    province.addStatus(new ProdBoom(5));
                }

                public override Dictionary<Resource, float> cost() {
                    return base.cost();
                }

                public override void reject() {
                    province.Resources_amount += 0.2f;
                }
            }
        }
    }
    
}
