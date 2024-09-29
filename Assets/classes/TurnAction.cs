using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.classes {
    public class actionContainer {
        [SerializeField] private Map map;

        internal interface IInstantAction { }
        public class TurnAction {
            public enum ActionType {
                army_move,
                army_recruitment,
                war_offer,
                war_end,
                alliance_offer,
                alliance_end,
                milacc_offer,
                milacc_end,
                vasal_offer,
                vasal_end,
                subs_offer,
                subs_end
            }

            public ActionType type;
            public virtual string desc { get;}
            public float cost;
            public Dictionary<Resource, float>? altCosts;
            public Dictionary<Resource, float> fullCost { get => fullRes(); }
            internal Dictionary<Resource, float> cRes;

            private Dictionary<Resource, float> fullRes() {
                var res = altCosts != null ? new Dictionary<Resource, float>(altCosts) : new Dictionary<Resource, float> { { Resource.AP, 0} };
                res[Resource.AP] = cost;
                return res;
            }
            public TurnAction(ActionType type, float cost, Dictionary<Resource, float> altCosts = null) {
                this.type = type;
                this.cost = cost;
                this.altCosts = altCosts;
            }
            /// <summary>
            /// execution is locked-in action done during turn calculation
            /// </summary>
            public virtual void execute(Map map) {

            }
            /// <summary>
            /// preview shows effects of action during turn, able to be reversed
            /// </summary>
            public virtual void preview(Map map) {
                cRes[Resource.AP] -= cost;
                if(altCosts!=null)foreach(var key in altCosts.Keys) {
                    cRes[key] -= altCosts[key];
                }
            }

            /// <summary>
            /// reversion is a deletion of action from the queue. done before turn calculation, therefore only during player's turn
            /// </summary>
            public virtual void revert(Map map) {
                cRes[Resource.AP] += cost;
                if(altCosts != null) foreach(var key in altCosts.Keys) {
                        cRes[key] += altCosts[key];
                }
            }
            internal class army_move : TurnAction {
                private (int, int) from, to;
                private int count;
                private Army army;
                public army_move((int, int) from, (int, int) to, int count, Army army) : base(ActionType.army_move, 1){
                    Debug.Log(from + " " + to + " " + count);
                    this.from = from;
                    this.to = to;
                    this.count = count;
                    this.army = army;
                }
                public override string desc { get => count + "units moved from " + from.ToString() + " to " + to.ToString(); }

                public override void execute(Map map) {
                    base.execute(map);
                    map.MoveArmy(army);// temp and to be explained by someone who knows what the fuck is going on with army movement
                }

                public override void preview(Map map) {
                    base.preview(map);
                    this.army = map.setMoveArmy(this.army, count, to);
                }

                public override void revert(Map map) {
                    base.revert(map);
                    map.undoSetMoveArmy(army);

                }
            }
            internal class army_recruitment :TurnAction {
                private (int, int) coordinates;
                private int count;

                public army_recruitment((int, int) coordinates, int count) : base(ActionType.army_recruitment, 0.5f){
                    Debug.Log(coordinates + " " + count);
                    this.coordinates = coordinates;
                    this.count = count;
                    this.altCosts = new Dictionary<Resource, float> { { Resource.Gold, 1 } };
                }

                public override string desc { get => count + " units recruited in " + coordinates.ToString();  }

                public override void execute(Map map) {
                    base.execute(map);
                    map.getProvince(coordinates).Population += count;
                    map.getProvince(coordinates).RecruitablePopulation += count;
                    map.recArmy(coordinates, count);
                }
                public override void preview(Map map) {
                    base.preview(map);
                    map.getProvince(coordinates).Population -= count;
                    map.getProvince(coordinates).RecruitablePopulation -= count;
                }
                public override void revert(Map map) {
                    base.revert(map);
                    map.getProvince(coordinates).Population += count;
                    map.getProvince(coordinates).RecruitablePopulation += count;
                }
            }
            internal class army_disbandment : TurnAction {
                private Army army;
                private int count;

                public army_disbandment(Army army, int count) : base(ActionType.army_recruitment, 0.1f){
                    Debug.Log(count);
                    this.army = army;
                    this.count = count;
                }

                public override string desc { get => count + " units disbanded in " + army.Position; }

                public override void execute(Map map) {
                    base.execute(map);
                    map.disArmy(army.Position, count);
                }
                public override void preview(Map map) {
                    base.preview(map);
                    //army.count -= this.count;
                }
                public override void revert(Map map) {
                    base.revert(map);
                    //army.count += this.count;
                }
            }
            internal class start_war:TurnAction, IInstantAction {
                private Country c1, c2;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                public start_war(Country c1, Country c2, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box) : base(ActionType.war_offer, 1, null) {
                    this.c1 = c1;
                    this.c2 = c2;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }
                public override void execute(Map map) {
                    diplomacy.startWar(c1, c2);
                    c2.Events.Add(new Event_.DiploEvent.WarDeclared(c1, c2, diplomacy, dialog_box));
                }
            }
            internal class integrate_vassal:TurnAction, IInstantAction {
                private Relation.Vassalage vassalage;
                private diplomatic_relations_manager diplomacy;
                public integrate_vassal(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy):base(ActionType.vasal_end, (int)vassalage.Sides[1].Provinces.Count/10) {
                    this.vassalage = vassalage;
                    this.diplomacy = diplomacy;
                }

                public override void execute(Map map) {
                    diplomacy.integrateVassal(vassalage);
                }
            }
            internal class end_war:TurnAction, IInstantAction {
                private Country offer;
                private Relation.War war;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;

                public end_war(Country offer, Relation.War war, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box):base(ActionType.war_end, 0, null) {
                    this.offer = offer;
                    this.war = war;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override void execute(Map map) {
                    var to = war.Sides[0] == offer ? war.Sides[1] : war.Sides[0];
                    to.Events.Add( new Event_.DiploEvent.PeaceOffer(war, offer, diplomacy, dialog_box));
                }
            }
            internal class alliance_offer:TurnAction, IInstantAction {
                private Country c1, c2;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;

                public alliance_offer(Country c1, Country c2, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box) :base(ActionType.alliance_offer, 1){
                    this.c1 = c1;
                    this.c2 = c2;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }
                public override void execute(Map map) {
                    base.execute(map);
                    c2.Events.Add(new Event_.DiploEvent.AllianceOffer(c1, c2, diplomacy, dialog_box));
                }
            }
            internal class alliance_end:TurnAction, IInstantAction {
                private Country from;
                private Relation.Alliance alliance;
                private diplomatic_relations_manager diplomacy; 
                private dialog_box_manager dialog_box;

                public alliance_end(Country from, Relation.Alliance alliance, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box):base(ActionType.alliance_end, 1) {
                    this.from = from;
                    this.alliance = alliance;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override void execute(Map map) {
                    base.execute(map);
                    Country to = alliance.Sides.FirstOrDefault(c=>c!=from);
                    to.Events.Add(new Event_.DiploEvent.AllianceBroken(from, to, diplomacy, dialog_box));
                    diplomacy.endRelation(alliance);
                }
            }
            internal class access_offer:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;

                public access_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box):base(ActionType.milacc_offer, 1) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }
                public override void execute(Map map) {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.AccessOffer(from, to, diplomacy, dialog_box));
                }
            }
            internal class subs_offer:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private int amount, duration;

                public subs_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, int amount, int duration):base(ActionType.subs_offer, 1) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.amount = amount;
                    this.duration = duration;
                }

                public override void execute(Map map) {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.SubsOffer(from, to, diplomacy, dialog_box));
                }
            }
            internal class vassal_offer:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;

                public vassal_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box):base(ActionType.vasal_offer, 1) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }
            }
            internal class vassal_rebel:TurnAction, IInstantAction {
                private Relation.Vassalage vassalage;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;

                public vassal_rebel(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box):base(ActionType.war_offer, 2) {
                    this.vassalage = vassalage;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                }

                public override void execute(Map map) {
                    base.execute(map);
                    vassalage.Sides[0].Events.Add(new Event_.DiploEvent.VassalRebel(vassalage.Sides[1], vassalage.Sides[0], diplomacy, dialog_box));
                    diplomacy.endRelation(vassalage);
                    diplomacy.startWar(vassalage.Sides[1], vassalage.Sides[0]);
                }
            }
        }
        private List<TurnAction> actions;

        public actionContainer(Map map) {
            this.map = map;
            actions = new List<TurnAction>();
        }

        public List<TurnAction> extractInstants() {
            List<TurnAction> instants = new List<TurnAction>();
            if (actions != null) {
                instants = actions.FindAll(a => a is IInstantAction);
            }
            actions = actions.Except(instants).ToList();
            return instants;
        }

        public TurnAction last { get => actions[actions.Count - 1]; }

        public int Count { get { return actions.Count; } }

        public void addAction(TurnAction action) {
            action.cRes = map.Countries[map.currentPlayer].Resources;
            actions.Add(action);
            actions.Last().preview(map);
        }

        public void execute() {
            if(actions.Count == 0) return;
            actions[0].execute(map);
            actions.RemoveAt(0);
        }
        public void revert() { 
            last.revert(map);
            actions.Remove(last);
        }
    }
}
