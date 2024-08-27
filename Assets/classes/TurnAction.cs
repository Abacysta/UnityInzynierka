using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.classes {
    public class actionContainer : MonoBehaviour{
        [SerializeField] private Map map;
        public class TurnAction {
            public enum ActionType {
                army_move,
                army_recruitment,
                war_declaration,
                alliance_offer
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

                public override string desc { get => count + " units disbanded in " + army.position; }

                public override void execute(Map map) {
                    base.execute(map);
                    map.disArmy(army.position, count);
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
        }
        private List<TurnAction> actions;

        public actionContainer(Map map) {
            this.map = map;
            actions = new List<TurnAction>();
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
