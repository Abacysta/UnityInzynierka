using Assets.classes.subclasses;
using Assets.map.scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.classes.Relation;

namespace Assets.classes {
    public class TurnAction {

        public static readonly int PraiseOurOpinionBonusInit = 20;
        public static readonly int PraiseTheirOpinionBonusInit = PraiseOurOpinionBonusInit / 2;
        public static readonly int InsultOurOpinionPenaltyInit = 25;
        public static readonly int InsultTheirOpinionPenaltyInit = InsultOurOpinionPenaltyInit / 2;

        public enum ActionType {
            ArmyMove,
            ArmyRecruitment,
            ArmyDisbandment,
            StartWar,
            IntegrateVassal,
            WarEnd,
            AllianceOffer,
            AllianceEnd,
            MilAccOffer,
            MilAccRequest,
            MilAccEndMaster,
            MilAccEndSlave,
            SubsOffer,
            SubsRequest,
            SubsEnd,
            VassalizationOffer,
            VassalRebel,
            Insult,
            Praise,
            CallToWar,
            TechnologyUpgrade,
            BuildingUpgrade,
            BuildingDowngrade,
            FestivitiesOrganization,
            TaxBreakIntroduction,
            RebelSuppresion
        }

        public ActionType type;
        public virtual string desc { get; }
        /// <summary>
        /// AP cost
        /// </summary>
        public float cost;
        /// <summary>
        /// non-AP cost
        /// </summary>
        public Dictionary<Resource, float> altCosts;
        public Dictionary<Resource, float> fullCost { get => fullRes(); }
        internal Dictionary<Resource, float> cRes;

        private Dictionary<Resource, float> fullRes() {
            var res = altCosts != null ? new Dictionary<Resource, float>(altCosts) : new Dictionary<Resource, float> { { Resource.AP, 0 } };
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
            if (altCosts != null) foreach (var key in altCosts.Keys) {
                    cRes[key] -= altCosts[key];
                }
        }

        /// <summary>
        /// reversion is a deletion of action from the queue. done before turn calculation, therefore only during player's turn
        /// </summary>
        public virtual void revert(Map map) {
            cRes[Resource.AP] += cost;
            if (altCosts != null) foreach (var key in altCosts.Keys) {
                    cRes[key] += altCosts[key];
                }
        }

        internal class army_move : TurnAction {
            private (int, int) from, to;
            private int count;
            private Army army;
            private Army armyPreview;

            public army_move((int, int) from, (int, int) to, int count, Army army) : base(ActionType.ArmyMove,
                CostsCalculator.TurnActionApCost(ActionType.ArmyMove)) {
                Debug.Log(from + " " + to + " " + count);
                this.from = from;
                this.to = to;
                this.count = count;
                this.army = army;
            }

            public override string desc { get => count + "units moved from " + from.ToString() + " to " + to.ToString(); }

            public override void execute(Map map) {
                base.execute(map);
                army = map.setMoveArmy(army, count, to);
                map.MoveArmy(army);
            }

            public override void preview(Map map) {
                base.preview(map);
                armyPreview = map.setMoveArmy(army, count, to);
            }

            public override void revert(Map map) {
                base.revert(map);
                map.undoSetMoveArmy(armyPreview);
            }

            public Army Army { get { return army; } }
        }

        internal class army_recruitment : TurnAction {
            private (int, int) coordinates;
            private int count;

            public army_recruitment((int, int) coordinates, int count, Country.TechnologyInterpreter techStats) : base(ActionType.ArmyRecruitment,
                CostsCalculator.TurnActionApCost(ActionType.ArmyRecruitment)) {
                Debug.Log(coordinates + " " + count);
                this.coordinates = coordinates;
                this.count = count;
                altCosts = CostsCalculator.TurnActionAltCost(ActionType.ArmyRecruitment);
            }

            public override string desc { get => count + " units recruited in " + coordinates.ToString(); }

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

            public army_disbandment(Army army, int count) : base(ActionType.ArmyDisbandment,
                CostsCalculator.TurnActionApCost(ActionType.ArmyDisbandment)) {
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
        internal class rebel_suppresion : TurnAction, IInstantAction {
            private readonly Province province;
            private int oldH;
            private int oldR;
            public rebel_suppresion(Province province) :
                base(ActionType.RebelSuppresion, CostsCalculator.TurnActionApCost(ActionType.RebelSuppresion)) {
                this.province = province;
                this.oldH = province.Happiness;
                this.oldR = province.RecruitablePopulation;
            }
            public override void preview(Map map) {
                base.preview(map);
                province.Happiness = 40;
                province.Population -= oldR;
                province.RecruitablePopulation -= oldR;
            }
            public override void revert(Map map) {
                base.revert(map);
                province.Happiness = oldH;
                province.Population += oldR;
                province.RecruitablePopulation += oldR;
            }

        }
        internal class technology_upgrade : TurnAction, IInstantAction {
            private readonly Technology techType;
            private readonly Country country;

            public technology_upgrade(Country country, Technology techType) :
                base(ActionType.TechnologyUpgrade, CostsCalculator.TurnActionApCost(ActionType.TechnologyUpgrade)) {
                this.techType = techType;
                this.country = country;
                altCosts = CostsCalculator.TurnActionAltCost(ActionType.TechnologyUpgrade, country.Technologies, techType);
            }

            public override void preview(Map map) {
                base.preview(map);
                country.Technologies[techType]++;
                country.techStats.Calculate(country.Technologies);
            }

            public override void revert(Map map) {
                base.revert(map);
                country.Technologies[techType]--;
                country.techStats.Calculate(country.Technologies);
            }
        }

        internal class building_upgrade : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly BuildingType buildingType;

            public building_upgrade(Province province, BuildingType buildingType) : base(ActionType.BuildingUpgrade,
                CostsCalculator.TurnActionApCost(ActionType.BuildingUpgrade)) {
                this.province = province;
                this.buildingType = buildingType;
                int upgradeLevel = province.Buildings.Find(b => b.BuildingType == buildingType).BuildingLevel + 1;
                altCosts = CostsCalculator.TurnActionAltCost(ActionType.BuildingUpgrade, buildingType, upgradeLevel);
            }

            public override void preview(Map map) {
                base.preview(map);
                province.UpgradeBuilding(buildingType);
            }

            public override void revert(Map map) {
                base.revert(map);
                province.DowngradeBuilding(buildingType);
            }
        }

        internal class building_downgrade : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly BuildingType buildingType;

            public building_downgrade(Province province, BuildingType buildingType) : base(ActionType.BuildingDowngrade,
                CostsCalculator.TurnActionApCost(ActionType.BuildingDowngrade)) {
                this.province = province;
                this.buildingType = buildingType;
            }

            public override void preview(Map map) {
                base.preview(map);
                province.DowngradeBuilding(buildingType);
            }

            public override void revert(Map map) {
                base.revert(map);
                province.UpgradeBuilding(buildingType);
            }
        }

        internal class festivities_organization : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly Status status;

            public festivities_organization(Province province) : base(ActionType.FestivitiesOrganization,
                CostsCalculator.TurnActionApCost(ActionType.FestivitiesOrganization)) {
                this.province = province;
                status = new Festivities(5);
            }

            public override void preview(Map map) {
                base.preview(map);
                province.addStatus(status);
            }

            public override void revert(Map map) {
                base.revert(map);
                province.RemoveStatus(status);
            }
        }

        internal class tax_break_introduction : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly Status status;

            public tax_break_introduction(Province province) : base(ActionType.TaxBreakIntroduction,
                CostsCalculator.TurnActionApCost(ActionType.TaxBreakIntroduction)) {
                this.province = province;
                status = new TaxBreak(5);
            }

            public override void preview(Map map) {
                base.preview(map);
                province.addStatus(status);
            }

            public override void revert(Map map) {
                base.revert(map);
                province.RemoveStatus(status);
            }
        }

        internal class start_war : TurnAction, IInstantAction {
            private Country c1, c2;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public start_war(Country c1, Country c2, diplomatic_relations_manager diplomacy,
                dialog_box_manager dialog_box, camera_controller camera, diplomatic_actions_manager dipl_actions) :
                base(ActionType.StartWar,
                CostsCalculator.TurnActionApCost(ActionType.StartWar)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetDeclareWarRelatedButtonStates(true, c2.Id);
            }
        }

        internal class integrate_vassal : TurnAction, IInstantAction {
            private Relation.Vassalage vassalage;
            private diplomatic_relations_manager diplomacy;
            private diplomatic_actions_manager dipl_actions;

            public integrate_vassal(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy,
                diplomatic_actions_manager dipl_actions) : base(ActionType.IntegrateVassal,
                    CostsCalculator.TurnActionApCost(ActionType.IntegrateVassal, vassalage)) {
                this.vassalage = vassalage;
                this.diplomacy = diplomacy;
                this.dipl_actions = dipl_actions;
            }

            public override void execute(Map map) {
                diplomacy.integrateVassal(vassalage);
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetIntegrateVassalRelatedButtonStates(true, vassalage.Sides[1].Id);
            }
        }

        internal class end_war : TurnAction, IInstantAction {
            private Country offer;
            private Relation.War war;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private Country to;

            public end_war(Country offer, Relation.War war, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera) : base(ActionType.WarEnd,
                CostsCalculator.TurnActionApCost(ActionType.WarEnd)) {
                this.offer = offer;
                this.war = war;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.camera = camera;
                to = war.Sides[0] == offer ? war.Sides[1] : war.Sides[0];
            }

            public override void execute(Map map) {
                to.Events.Add(new Event_.DiploEvent.PeaceOffer(war, offer, diplomacy, dialog_box, camera));
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetOfferPeaceRelatedButtonStates(true, to.Id);
            }
        }

        internal class alliance_offer : TurnAction, IInstantAction {
            private Country c1, c2;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public alliance_offer(Country c1, Country c2, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.AllianceOffer,
                    CostsCalculator.TurnActionApCost(ActionType.AllianceOffer)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetOfferAllianceRelatedButtonStates(true, c2.Id);
            }
        }

        internal class alliance_end : TurnAction, IInstantAction {
            private Country from;
            private Relation.Alliance alliance;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private Country to;

            public alliance_end(Country from, Relation.Alliance alliance, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.AllianceEnd,
                    CostsCalculator.TurnActionApCost(ActionType.AllianceEnd)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetBreakAllianceRelatedButtonStates(true, to.Id);
            }
        }

        internal class access_offer : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public access_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.MilAccOffer,
                    CostsCalculator.TurnActionApCost(ActionType.MilAccOffer)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetOfferMilitaryAccessRelatedButtonStates(true, to.Id);
            }
        }

        internal class access_request : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public access_request(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.MilAccRequest,
                    CostsCalculator.TurnActionApCost(ActionType.MilAccRequest)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
                this.camera = camera;
            }
            public override void execute(Map map) {
                base.execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessRequest(from, to, diplomacy, dialog_box, camera));
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetRequestMilitaryAccessRelatedButtonStates(true, to.Id);
            }
        }

        internal class access_end_master : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private MilitaryAccess militaryAccess;

            public access_end_master(Country from, Country to, MilitaryAccess militaryAccess, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.MilAccEndMaster,
                    CostsCalculator.TurnActionApCost(ActionType.MilAccEndMaster)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.camera = camera;
                this.dipl_actions = dipl_actions;
                this.militaryAccess = militaryAccess;
            }

            public override void execute(Map map) {
                base.execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessEndMaster(militaryAccess, from, to, diplomacy, dialog_box, camera));
                diplomacy.endRelation(militaryAccess);
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetEndMilitaryAccessMasterRelatedButtonStates(true, to.Id);
            }
        }

        internal class access_end_slave : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private MilitaryAccess militaryAccess;

            public access_end_slave(Country from, Country to, MilitaryAccess militaryAccess, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.MilAccEndSlave,
                    CostsCalculator.TurnActionApCost(ActionType.MilAccEndSlave)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.camera = camera;
                this.dipl_actions = dipl_actions;
                this.militaryAccess = militaryAccess;
            }

            public override void execute(Map map) {
                base.execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessEndSlave(militaryAccess, from, to, diplomacy, dialog_box, camera));
                diplomacy.endRelation(militaryAccess);
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetEndMilitaryAccessSlaveRelatedButtonStates(true, to.Id);
            }
        }

        internal class subs_offer : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private int amount, duration;

            public subs_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                int amount, int duration, camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.SubsOffer,
                    CostsCalculator.TurnActionApCost(ActionType.SubsOffer)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetSubsidizeRelatedButtonStates(true, to.Id);
            }
        }

        internal class subs_end : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private Subsidies subsidies;

            public subs_end(Country from, Country to, Subsidies subsidies, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.SubsEnd,
                    CostsCalculator.TurnActionApCost(ActionType.SubsEnd)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.camera = camera;
                this.dipl_actions = dipl_actions;
                this.subsidies = subsidies;
            }

            public override void execute(Map map) {
                base.execute(map);
                to.Events.Add(new Event_.DiploEvent.SubsEndMaster(from, to, diplomacy, dialog_box, camera));
                diplomacy.endRelation(subsidies);
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetEndSubsidiesRelatedButtonStates(true, to.Id);
            }
        }

        internal class subs_request : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private int amount, duration;

            public subs_request(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                int amount, int duration, camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.SubsRequest,
                    CostsCalculator.TurnActionApCost(ActionType.SubsRequest)) {
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
                to.Events.Add(new Event_.DiploEvent.SubsRequest(from, to, diplomacy, dialog_box, amount, duration, camera));
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetRequestSubsidiesRelatedButtonStates(true, to.Id);
            }
        }

        internal class vassal_offer : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public vassal_offer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.VassalizationOffer,
                    CostsCalculator.TurnActionApCost(ActionType.VassalizationOffer)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
            }

            public override void execute(Map map) {
                base.execute(map);
                to.Events.Add(new Event_.DiploEvent.VassalOffer(from, to, diplomacy, dialog_box, camera));
            }

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetOfferVassalizationRelatedButtonStates(true, to.Id);
            }
        }

        internal class vassal_rebel : TurnAction, IInstantAction {
            private Relation.Vassalage vassalage;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public vassal_rebel(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.VassalRebel,
                    CostsCalculator.TurnActionApCost(ActionType.VassalRebel)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.SetVassalRebelRelatedButtonStates(true, vassalage.Sides[0].Id);
            }
        }

        internal class insult : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public insult(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.Insult,
                    CostsCalculator.TurnActionApCost(ActionType.Insult)) {
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

        internal class praise : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public praise(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.Praise,
                    CostsCalculator.TurnActionApCost(ActionType.Praise)) {
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

        internal class call_to_war : TurnAction, IInstantAction {
            private Country from, to;
            private Relation.War war;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_relations_manager diplomacy;
            private diplomatic_actions_manager dipl_actions;

            public call_to_war(Country from, Country to, Relation.War war, dialog_box_manager dialog_box,
                diplomatic_relations_manager diplomacy, camera_controller camera,
                diplomatic_actions_manager dipl_actions) : base(ActionType.CallToWar,
                    CostsCalculator.TurnActionApCost(ActionType.CallToWar)) {
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

            public override void revert(Map map) {
                base.revert(map);
                dipl_actions.RevertCallToWarRelatedButtonStates(to.Id);
            }
        }
    }
    internal interface IInstantAction { }
    public class actionContainer {
        private Map map;

        


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