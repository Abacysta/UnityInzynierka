using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.classes {
    public class actionContainer : MonoBehaviour{
        [SerializeField] private static Map map;
        public class TurnAction {
            public enum ActionType {
                army_move,
                army_recruitment,
                war_declaration,
                alliance_offer
            }

            public ActionType type;
            public virtual string desc { get;}
            public TurnAction(ActionType type) {
                this.type = type;
            }
            /// <summary>
            /// execution is locked-in action done during turn calculation
            /// </summary>
            public virtual void execute() {

            }
            /// <summary>
            /// preview shows effects of action during turn, able to be reversed
            /// </summary>
            public virtual void preview() {

            }

            /// <summary>
            /// reversion is a deletion of action from the queue. done before turn calculation, therefore only during player's turn
            /// </summary>
            public virtual void revert() {

            }
            internal class army_move : TurnAction {
                private (int, int) from, to;
                private int count;
                private Army army;
                public army_move((int, int) from, (int, int) to, int count, Army army) : base(ActionType.army_move){
                    this.from = from;
                    this.to = to;
                    this.count = count;
                    this.army = army;
                }
                public override string desc { get => count + "units moved from " + from.ToString() + " to " + to.ToString(); }

                public override void execute() {
                    base.execute();
                    map.MoveArmy(army);// temp and to be explained by someone who knows what the fuck is going on with army movement
                }

                public override void preview() {
                    base.preview();
                    map.setMoveArmy(army, count, to);
                }

                public override void revert() {
                    base.revert();

                }
            }
            internal class army_recruitment :TurnAction {
                private (int, int) coordinates;
                private int count;

                public army_recruitment((int, int) coordinates, int count) : base(ActionType.army_recruitment){
                    this.coordinates = coordinates;
                    this.count = count;
                }

                public override string desc { get => count + " units recruited in " + coordinates.ToString();  }

                public override void execute() {
                    base.execute();
                    map.recArmy(coordinates, count);
                }
                public override void preview() {
                    base.preview();
                }
                public override void revert() {
                    base.revert();

                }
            }
            internal class army_disbandment : TurnAction {
                private Army army;
                private int count;

                public army_disbandment(Army army, int count) : base(ActionType.army_recruitment){
                    this.army = army;
                    this.count = count;
                }

                public override string desc { get => count + " units disbanded in " + army.position; }

                public override void execute() {
                    base.execute();
                    army.count -= this.count;
                }
                public override void preview() {
                    base.preview();
                }
                public override void revert() {
                    base.revert();
                }
            }
        }
        private List<TurnAction> actions;

        public actionContainer() {
            actions = new List<TurnAction>();
        }

        public TurnAction last { get => actions[actions.Count - 1]; set => actions.Add(value);  }

        public void execute() {
            actions[0].execute();
            actions.RemoveAt(0);
        }
        public void revert() { 
            last.revert();
            actions.Remove(last);
        }
    }
}
