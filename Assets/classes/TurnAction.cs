using Assets.map.scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.classes.Relation;

namespace Assets.classes {
    public class actionContainer {
        private Map map;

        internal interface IInstantAction { }

        public class TurnAction {
            public static readonly float HardActionCost = 1f;
            public static readonly float SoftActionCost = 0.1f;
            public static readonly int PraiseOurOpinionBonusInit = 20;
            public static readonly int PraiseTheirOpinionBonusInit = PraiseOurOpinionBonusInit / 2;
            public static readonly int InsultOurOpinionPenaltyInit = 25;
            public static readonly int InsultTheirOpinionPenaltyInit = InsultOurOpinionPenaltyInit / 2;

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
                subs_end,
                subs_request,
                message
            }

            public ActionType type;
            public virtual string desc { get;}
            public float cost;
            public Dictionary<Resource, float> altCosts;
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
                public static readonly float actionCost = HardActionCost;

                public army_move((int, int) from, (int, int) to, int count, Army army) : base(ActionType.army_move, actionCost) {
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

                public Army Army { get { return army; } }
            }

            internal class army_recruitment :TurnAction {
                private (int, int) coordinates;
                private int count;
                public static readonly float actionCost = HardActionCost;

                public army_recruitment((int, int) coordinates, int count) : base(ActionType.army_recruitment, actionCost){
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
                public static readonly float actionCost = SoftActionCost;

                public army_disbandment(Army army, int count) : base(ActionType.army_recruitment, actionCost)
                {
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
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public start_war(Country c1, Country c2, diplomatic_relations_manager diplomacy, 
                    dialog_box_manager dialog_box, camera_controller camera, diplomatic_actions_manager dipl_actions) : 
                    base(ActionType.war_offer, actionCost, null) {
                    this.c1 = c1;
                    this.c2 = c2;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                }

                public override void execute(Map map) {
                    diplomacy.startWar(c1, c2);
                    c2.Events.Add(new Event_.DiploEvent.WarDeclared(c1, c2, diplomacy, dialog_box, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetDeclareWarRelatedButtonStates(true, c2.Id);
                }
            }

            internal class integrate_vassal:TurnAction, IInstantAction {
                private Relation.Vassalage vassalage;
                private diplomatic_relations_manager diplomacy;
                private diplomatic_actions_manager dipl_actions;

                public integrate_vassal(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy, 
                    diplomatic_actions_manager dipl_actions) : base(ActionType.vasal_end, (int)vassalage.Sides[1].Provinces.Count/10) {
                    this.vassalage = vassalage;
                    this.diplomacy = diplomacy;
                    this.dipl_actions = dipl_actions;
                }

                public override void execute(Map map) {
                    diplomacy.integrateVassal(vassalage);
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetIntegrateVassalRelatedButtonStates(true, vassalage.Sides[1].Id);
                }
            }

            internal class end_war:TurnAction, IInstantAction {
                private Country offer;
                private Relation.War war;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private Country to;
                public static readonly float actionCost = SoftActionCost;

                public end_war(Country offer, Relation.War war, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera) : base(ActionType.war_end, actionCost, null) {
                    this.offer = offer;
                    this.war = war;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    to = war.Sides[0] == offer ? war.Sides[1] : war.Sides[0];
                }

                public override void execute(Map map) {
                    to.Events.Add( new Event_.DiploEvent.PeaceOffer(war, offer, diplomacy, dialog_box, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetOfferPeaceRelatedButtonStates(true, to.Id);
                }
            }

            internal class alliance_offer:TurnAction, IInstantAction {
                private Country c1, c2;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public alliance_offer(Country c1, Country c2, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.alliance_offer, actionCost) {
                    this.c1 = c1;
                    this.c2 = c2;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.dipl_actions = dipl_actions;
                    this.camera = camera;
                }
                public override void execute(Map map) {
                    base.execute(map);
                    c2.Events.Add(new Event_.DiploEvent.AllianceOffer(c1, c2, diplomacy, dialog_box, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetOfferAllianceRelatedButtonStates(true, c2.Id);
                }
            }

            internal class alliance_end:TurnAction, IInstantAction {
                private Country from;
                private Relation.Alliance alliance;
                private diplomatic_relations_manager diplomacy; 
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private Country to;
                public static readonly float actionCost = HardActionCost;

                public alliance_end(Country from, Relation.Alliance alliance, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.alliance_end, actionCost) {
                    this.from = from;
                    this.alliance = alliance;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.dipl_actions = dipl_actions;
                    this.camera = camera;
                    to = alliance.Sides.FirstOrDefault(c => c != from);
                }

                public override void execute(Map map) {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.AllianceBroken(from, to, diplomacy, dialog_box, camera));
                    diplomacy.endRelation(alliance);
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetBreakAllianceRelatedButtonStates(true, to.Id);
                }
            }

            internal class access_offer:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public access_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.milacc_offer, actionCost) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.dipl_actions = dipl_actions;
                    this.camera = camera;
                }
                public override void execute(Map map) {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.AccessOffer(from, to, diplomacy, dialog_box, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetOfferMilitaryAccessRelatedButtonStates(true, to.Id);
                }
            }

            internal class access_end_master : TurnAction, IInstantAction
            {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private MilitaryAccess militaryAccess;
                public static readonly float actionCost = SoftActionCost;

                public access_end_master(Country from, Country to, MilitaryAccess militaryAccess, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.milacc_end, actionCost)
                {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                    this.militaryAccess = militaryAccess;
                }

                public override void execute(Map map)
                {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.AccessEndMaster(militaryAccess, from, to, diplomacy, dialog_box, camera));
                    diplomacy.endRelation(militaryAccess);
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetEndMilitaryAccessMasterRelatedButtonStates(true, to.Id);
                }
            }

            internal class access_end_slave : TurnAction, IInstantAction
            {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private MilitaryAccess militaryAccess;
                public static readonly float actionCost = SoftActionCost;

                public access_end_slave(Country from, Country to, MilitaryAccess militaryAccess, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.milacc_end, actionCost)
                {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                    this.militaryAccess = militaryAccess;
                }

                public override void execute(Map map)
                {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.AccessEndSlave(militaryAccess, from, to, diplomacy, dialog_box, camera));
                    diplomacy.endRelation(militaryAccess);
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetEndMilitaryAccessSlaveRelatedButtonStates(true, to.Id);
                }
            }

            internal class subs_offer:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private int amount, duration;
                public static readonly float actionCost = HardActionCost;

                public subs_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    int amount, int duration, camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.subs_offer, actionCost) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                    this.amount = amount;
                    this.duration = duration;
                }

                public override void execute(Map map) {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.SubsOffer(from, to, diplomacy, dialog_box, amount, duration, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetSubsidizeRelatedButtonStates(true, to.Id);
                }
            }

            internal class subs_end : TurnAction, IInstantAction
            {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private Subsidies subsidies;
                public static readonly float actionCost = SoftActionCost;

                public subs_end(Country from, Country to, Subsidies subsidies, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.subs_end, SoftActionCost) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                    this.subsidies = subsidies;
                }

                public override void execute(Map map)
                {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.SubsEndMaster(from, to, diplomacy, dialog_box, camera));
                    diplomacy.endRelation(subsidies);
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetEndSubsidiesRelatedButtonStates(true, to.Id);
                }
            }

            internal class subs_request : TurnAction, IInstantAction
            {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                private int amount, duration;
                public static readonly float actionCost = HardActionCost;

                public subs_request(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                    int amount, int duration, camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.subs_request, actionCost)
                {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                    this.amount = amount;
                    this.duration = duration;
                }

                public override void execute(Map map)
                {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.SubsRequest(from, to, diplomacy, dialog_box, amount, duration, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetRequestSubsidiesRelatedButtonStates(true, to.Id);
                }
            }

            internal class vassal_offer:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public vassal_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.vasal_offer, actionCost) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.dipl_actions = dipl_actions;
                }

                public override void execute(Map map)
                {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.VassalOffer(from, to, diplomacy, dialog_box, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetOfferVassalizationRelatedButtonStates(true, to.Id);
                }
            }

            internal class vassal_rebel:TurnAction, IInstantAction {
                private Relation.Vassalage vassalage;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public vassal_rebel(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.war_offer, actionCost) {
                    this.vassalage = vassalage;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.dipl_actions = dipl_actions;
                }

                public override void execute(Map map) {
                    base.execute(map);
                    vassalage.Sides[0].Events.Add(new Event_.DiploEvent.VassalRebel(vassalage.Sides[1], vassalage.Sides[0], diplomacy, dialog_box, camera));
                    diplomacy.endRelation(vassalage);
                    diplomacy.startWar(vassalage.Sides[1], vassalage.Sides[0]);
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.SetVassalRebelRelatedButtonStates(true, vassalage.Sides[0].Id);
                }
            }

            internal class insult:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public insult(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.message, actionCost) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.dipl_actions = dipl_actions;
                }

                public override void preview(Map map) {
                    base.preview(map);
                    from.Opinions[to.Id] -= InsultOurOpinionPenaltyInit;
                    to.Opinions[from.Id] -= InsultTheirOpinionPenaltyInit;
                }
                public override void revert(Map map) {
                    base.revert(map);
                    from.Opinions[to.Id] += InsultOurOpinionPenaltyInit;
                    to.Opinions[from.Id] += InsultTheirOpinionPenaltyInit;
                    dipl_actions.SetInsultRelatedButtonStates(true, to.Id);
                }
            }

            internal class praise:TurnAction, IInstantAction {
                private Country from, to;
                private diplomatic_relations_manager diplomacy;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public praise(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, 
                    camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.message, actionCost) {
                    this.from = from;
                    this.to = to;
                    this.diplomacy = diplomacy;
                    this.dialog_box = dialog_box;
                    this.dipl_actions = dipl_actions;
                }
                public override void preview(Map map) {
                    base.preview(map);
                    from.Opinions[to.Id] += PraiseOurOpinionBonusInit;
                    to.Opinions[from.Id] += PraiseTheirOpinionBonusInit;

                }
                public override void revert(Map map) {
                    base.revert(map);
                    from.Opinions[to.Id] -= PraiseOurOpinionBonusInit;
                    to.Opinions[from.Id] -= PraiseTheirOpinionBonusInit;
                    dipl_actions.SetDiplomaticMissionRelatedButtonStates(true, to.Id);
                }
            }

            internal class call_to_war:TurnAction, IInstantAction {
                private Country from, to;
                private Relation.War war;
                private dialog_box_manager dialog_box;
                private camera_controller camera;
                private diplomatic_relations_manager diplomacy;
                private diplomatic_actions_manager dipl_actions;
                public static readonly float actionCost = HardActionCost;

                public call_to_war(Country from, Country to, Relation.War war, dialog_box_manager dialog_box, 
                    diplomatic_relations_manager diplomacy, camera_controller camera, 
                    diplomatic_actions_manager dipl_actions) : base(ActionType.alliance_offer, actionCost) {
                    this.from = from;
                    this.to = to;
                    this.war = war;
                    this.dialog_box = dialog_box;
                    this.camera = camera;
                    this.diplomacy = diplomacy;
                    this.dipl_actions = dipl_actions;
                }
                public override void execute(Map map) {
                    base.execute(map);
                    to.Events.Add(new Event_.DiploEvent.CallToWar(from, to, diplomacy, dialog_box, war, camera));
                }

                public override void revert(Map map)
                {
                    base.revert(map);
                    dipl_actions.RevertCallToWarRelatedButtonStates(to.Id);
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
            if(actions.Count>0){
                last.revert(map);
                actions.Remove(last);
            }
        }
    }
}