using System;
using System.Collections.Generic;
using static Assets.classes.subclasses.Constants.RelationConstants;

namespace Assets.classes {
    [Serializable]
    public abstract class Relation {
        private Country[] countries;
        public RelationType Type {  get; set; }
        private int initialChange, constChange;
        public Country[] Sides { get { return countries; } }

        public virtual void TurnEffect() {
            countries[0].SetOpinion(countries[1].Id, countries[0].Opinions[countries[1].Id] + constChange);
            countries[1].SetOpinion(countries[0].Id, countries[1].Opinions[countries[0].Id] + constChange);
        }

        [Serializable]
        public enum RelationType { 
            Rebellion = -2, //only for happ rebels, purely technical, maybe don't treat like a real relation man :^)
            War = -1,
            Alliance = 3,
            Truce = 0,
            Vassalage = 4,
            Subsidies = 1,
            MilitaryAccess = 2
        }

        protected Relation(Country c1, Country c2, RelationType type, int initalChange, int constChange) {
            this.countries = new Country[2] { c1, c2};
            this.Type = type;
            this.initialChange = initalChange;
            this.constChange = constChange;

            countries[0].SetOpinion(c2.Id, countries[0].Opinions[c2.Id] + initalChange);
            countries[1].SetOpinion(c1.Id, countries[1].Opinions[c1.Id] + initalChange);
        }

        internal class War : Relation {
            public HashSet<Country> Participants1 {  get; set; }
            public HashSet<Country> Participants2 { get; set; }

            public War(Country c1, Country c2) : base(c1, c2, RelationType.War, 
                WAR_OPINION_PENALTY_INIT, WAR_OPINION_PENALTY_CONST) {
                countries[0].AtWar = true;
                countries[1].AtWar = true;
                Participants1 = new HashSet<Country> { c1 };
                Participants2 = new HashSet<Country>{c2};
            }
            
            public override void TurnEffect() {
                base.TurnEffect();
            }
        }

        internal class Alliance : Relation {
            public Alliance(Country c1, Country c2) : base(c1, c2, RelationType.Alliance, 
                ALLIANCE_OPINION_BONUS_INIT, ALLIANCE_OPINION_BONUS_CONST) {
                foreach(var p in countries[0].Provinces) {
                    p.Happiness += ALLIANCE_HAPP_BONUS_INIT;
                }
                foreach(var p in countries[1].Provinces) {
                    p.Happiness += ALLIANCE_HAPP_BONUS_INIT;
                }
            }

            public override void TurnEffect() {
                base.TurnEffect();
            }
        }

        internal class Truce : Relation {
            private int d;
            public int Duration {  get { return d; } }

            public Truce(Country c1, Country c2, int duration) : base(c1, c2, RelationType.Truce, 
                TRUCE_OPINION_BONUS_INIT, TRUCE_OPINION_BONUS_CONST) {
                this.d = duration;
            }

            public override void TurnEffect() {
                base.TurnEffect();
                if(d > 0) d--;
            }
        }

        internal class Vassalage : Relation {
            public Vassalage(Country c1, Country c2) : base(c1, c2, RelationType.Vassalage, 
                VASSALAGE_OPINION_PENALTY_INIT_C2, VASSALAGE_OPINION_PENALTY_CONST_C2) {
                // Senior needs to receive an adjustment of to be back to zero
                countries[0].SetOpinion(countries[1].Id, countries[0].Opinions[countries[1].Id] - initialChange);
            }

            public override void TurnEffect() {
                countries[1].SetOpinion(countries[0].Id, countries[1].Opinions[countries[0].Id] + constChange);
            }
        }

        internal class Subsidies : Relation {
            private int amount;
            private int duration;
            public int Amount { get { return amount; } }
            public int Duration { get { return duration; } }
            public float fAmount { get { return (float)amount; } }

            public Subsidies(Country c1, Country c2, int amount) : base(c1, c2, RelationType.Subsidies, 
                SUBSIDIES_OPINION_BONUS_INIT, SUBSIDIES_OPINION_BONUS_CONST) {
                this.amount = amount;
                this.duration = -1;
            }

            public Subsidies(Country c1, Country c2, int amount, int duration) : base(c1, c2, RelationType.Subsidies, 
                SUBSIDIES_OPINION_BONUS_INIT, SUBSIDIES_OPINION_BONUS_CONST) {
                this.amount = amount;
                this.duration = duration;
            }

            public override void TurnEffect() {
                base.TurnEffect();
                if(duration>0) {
                    duration--;
                    if(countries[0].Resources[Resource.Gold] >= amount) {
                        countries[0].Resources[Resource.Gold] -= amount;
                        countries[1].Resources[Resource.Gold] += amount;
                    }
                    else {
                        duration = 0;
                    }
                }
            }
        }

        internal class MilitaryAccess : Relation {
            public MilitaryAccess(Country c1, Country c2) : base(c1, c2, RelationType.MilitaryAccess, 0, 0) {}

            public override void TurnEffect() {
                base.TurnEffect();
            }
        }
    }
}