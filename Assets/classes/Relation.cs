using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.classes {
    internal abstract class Relation {
        private Country[] countries;
        private RelationType type;
        private int initialChange, constChange;
        public virtual void turnEffect() {
            countries[0].Opinions[countries[1].Id] += constChange;
            countries[1].Opinions[countries[0].Id] += constChange;
        }
        public enum RelationType { 
            War = -1,
            Alliance = 3,
            Truce = 0,
            Vassalage = 4,
            Subsidies = 1,
            MilitaryAccess = 2
        }
        protected Relation(Country c1, Country c2, RelationType type, int initalChange, int constChange) {
            this.countries = new Country[2] { c1, c2};
            this.type = type;
            this.initialChange = initalChange;
            this.constChange = constChange;
            countries[0].Opinions[c2.Id] += initalChange;
            countries[1].Opinions[c1.Id] += initalChange;
        }

        internal class War:Relation {
            public War(Country c1, Country c2) : base(c1, c2, RelationType.War, -100, -15) {
                countries[0].AtWar = true;
                countries[1].AtWar = true;
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }
        internal class Alliance:Relation {
            public Alliance(Country c1, Country c2, RelationType type, int initalChange, int constChange) : base(c1, c2, RelationType.Alliance, 100, 10) {
                foreach(var p in countries[0].Provinces) {
                    p.Happiness += 3;
                }
                foreach(var p in countries[1].Provinces) {
                    p.Happiness += 3;
                }
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }
        internal class Truce:Relation {
            public Truce(Country c1, Country c2, RelationType type, int initalChange, int constChange) : base(c1, c2, RelationType.Truce, 10, 0) {
                
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }
        internal class Vassalage:Relation {
            public Vassalage(Country c1, Country c2, RelationType type, int initalChange, int constChange) : base(c1, c2, RelationType.Vassalage, -30, 5) {
                countries[0].Opinions[countries[1].Id] -= constChange;
            }

            public override void turnEffect() {
                countries[1].Opinions[countries[0].Id] += constChange;
            }
        }
        internal class Subsidies:Relation {
            public Subsidies(Country c1, Country c2, RelationType type, int initalChange, int constChange) : base(c1, c2, RelationType.Subsidies, 20, 5) {
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }
        internal class MilitaryAccess:Relation {
            public MilitaryAccess(Country c1, Country c2, RelationType type, int initalChange, int constChange) : base(c1, c2, RelationType.MilitaryAccess, 0, 0) {
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }
    }
}
