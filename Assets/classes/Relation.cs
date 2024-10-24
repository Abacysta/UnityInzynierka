using System;
using System.Collections.Generic;

namespace Assets.classes {
    [Serializable]
    public abstract class Relation {

        public static readonly int WarOpinionPenaltyInit = 100;
        public static readonly int WarOpinionPenaltyConst = 15;

        public static readonly int AllianceOpinionBonusInit = 100;
        public static readonly int AllianceOpinionBonusConst = 10;

        public static readonly int TruceOpinionBonusInit = 10;
        public static readonly int TruceOpinionBonusConst = 0;

        public static readonly int AllianceHappinessBonusInit = 3;

        public static readonly int VassalageOpinionPenaltyInitC2 = 30;
        public static readonly int VassalageOpinionPenaltyConstC2 = 5;

        public static readonly int SubsidiesOpinionBonusInit = 20;
        public static readonly int SubsidiesOpinionBonusConst = 5;

        private Country[] countries;
        public RelationType type;
        private int initialChange, constChange;
        public Country[] Sides { get { return countries; } }

        public virtual void turnEffect() {
            countries[0].Opinions[countries[1].Id] += constChange;
            countries[1].Opinions[countries[0].Id] += constChange;
        }

        [Serializable]
        public enum RelationType { 
            Rebellion = -2,//only for happ rebels, purely technical, maybe don't treat like a real relation retard
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
            public HashSet<Country> participants1, participants2;
            public War(Country c1, Country c2) : base(c1, c2, RelationType.War, -WarOpinionPenaltyInit, -WarOpinionPenaltyConst) {
                countries[0].AtWar = true;
                countries[1].AtWar = true;
                participants1 = new HashSet<Country> { c1 };
                participants2 = new HashSet<Country>{c2};
            }
            
            public override void turnEffect() {
                base.turnEffect();
            }
        }

        internal class Alliance:Relation {
            public Alliance(Country c1, Country c2) : base(c1, c2, RelationType.Alliance, AllianceOpinionBonusInit, AllianceOpinionBonusConst) {
                foreach(var p in countries[0].Provinces) {
                    p.Happiness += AllianceHappinessBonusInit;
                }
                foreach(var p in countries[1].Provinces) {
                    p.Happiness += AllianceHappinessBonusInit;
                }
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }

        internal class Truce:Relation {
            private int d;
            public int Duration {  get { return d; } }
            public Truce(Country c1, Country c2, int duration) : base(c1, c2, RelationType.Truce, TruceOpinionBonusInit, TruceOpinionBonusConst) {
                this.d = duration;
            }

            public override void turnEffect() {
                base.turnEffect();
                if(d > 0) d--;
            }
        }

        internal class Vassalage:Relation {
            public Vassalage(Country c1, Country c2) : base(c1, c2, RelationType.Vassalage, -VassalageOpinionPenaltyInitC2, VassalageOpinionPenaltyConstC2) {
                countries[0].Opinions[countries[1].Id] -= initialChange;
            }

            public override void turnEffect() {
                countries[1].Opinions[countries[0].Id] -= constChange;
            }
        }

        internal class Subsidies:Relation {
            private int amount;
            private int duration;
            public int Amount { get { return amount; } }
            public int Duration { get { return duration; } }
            public float fAmount { get { return (float)amount; } }

            public Subsidies(Country c1, Country c2, int amount) : base(c1, c2, RelationType.Subsidies, SubsidiesOpinionBonusInit, SubsidiesOpinionBonusConst) {
                this.amount = amount;
                this.duration = -1;
            }

            public Subsidies(Country c1, Country c2, int amount, int duration) : base(c1, c2, RelationType.Subsidies, SubsidiesOpinionBonusInit, SubsidiesOpinionBonusConst) {
                this.amount = amount;
                this.duration = duration;
            }

            public override void turnEffect() {
                base.turnEffect();
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

        internal class MilitaryAccess:Relation {
            public MilitaryAccess(Country c1, Country c2) : base(c1, c2, RelationType.MilitaryAccess, 0, 0) {
            }

            public override void turnEffect() {
                base.turnEffect();
            }
        }
    }
}