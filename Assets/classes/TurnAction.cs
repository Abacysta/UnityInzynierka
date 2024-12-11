using Assets.classes.subclasses;
using Assets.map.scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.classes.Relation;
using static Assets.classes.subclasses.Constants.Relation;

namespace Assets.classes {
    public class TurnAction {
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

        public ActionType Type {  get; set; }
        public virtual string desc { get; }

        /// <summary>
        /// AP cost
        /// </summary>
        public float Cost { get; set; }

        /// <summary>
        /// non-AP cost
        /// </summary>
        public Dictionary<Resource, float> AltCosts { get; set; }
        public Dictionary<Resource, float> fullCost { get => FullRes(); }
        internal Dictionary<Resource, float> cRes;

        private Dictionary<Resource, float> FullRes() {
            var res = AltCosts != null ? new Dictionary<Resource, float>(AltCosts) : new Dictionary<Resource, float> { { Resource.AP, 0 } };
            res[Resource.AP] = Cost;
            return res;
        }

        public TurnAction(ActionType type, float cost, Dictionary<Resource, float> altCosts = null) {
            this.Type = type;
            this.Cost = cost;
            this.AltCosts = altCosts;
        }

        /// <summary>
        /// execution is locked-in action done during turn calculation
        /// </summary>
        public virtual void Execute(Map map) {

        }

        /// <summary>
        /// preview shows effects of action during turn, able to be reversed
        /// </summary>
        public virtual void Preview(Map map) {
            cRes[Resource.AP] -= Cost;
            if (AltCosts != null) foreach (var key in AltCosts.Keys) {
                    cRes[key] -= AltCosts[key];
                }
        }

        /// <summary>
        /// reversion is a deletion of action from the queue. done before turn calculation, therefore only during player's turn
        /// </summary>
        public virtual void Revert(Map map) {
            cRes[Resource.AP] += Cost;
            if (AltCosts != null) foreach (var key in AltCosts.Keys) {
                    cRes[key] += AltCosts[key];
                }
        }

        internal class ArmyMove : TurnAction {
            private (int, int) from, to;
            private int count;
            private Army army;
            private Army armyPreview;

            public ArmyMove((int, int) from, (int, int) to, int count, Army army) : base(ActionType.ArmyMove,
                CostsCalculator.TurnActionApCost(ActionType.ArmyMove)) {
                Debug.Log(from + " " + to + " " + count);
                this.from = from;
                this.to = to;
                this.count = count;
                this.army = army;
            }

            public override string desc { get => count + "units moved from " + from.ToString() + " to " + to.ToString(); }

            public override void Execute(Map map) {
                base.Execute(map);
                army = map.SetMoveArmy(army, count, to);
                map.MoveArmy(army);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                armyPreview = map.SetMoveArmy(army, count, to);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                map.UndoSetMoveArmy(armyPreview);
            }

            public Army Army { get { return army; } }
        }

        internal class ArmyRecruitment : TurnAction {
            private (int, int) coordinates;
            private int count;

            public ArmyRecruitment((int, int) coordinates, int count, Country.TechnologyInterpreter techStats) : base(ActionType.ArmyRecruitment,
                CostsCalculator.TurnActionApCost(ActionType.ArmyRecruitment)) {
                Debug.Log(coordinates + " " + count);
                this.coordinates = coordinates;
                this.count = count;
                AltCosts = CostsCalculator.TurnActionAltCost(ActionType.ArmyRecruitment);
            }

            public override string desc { get => count + " units recruited in " + coordinates.ToString(); }

            public override void Execute(Map map) {
                base.Execute(map);
                map.GetProvince(coordinates).Population += count;
                map.GetProvince(coordinates).RecruitablePopulation += count;
                map.RecArmy(coordinates, count);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                map.GetProvince(coordinates).Population -= count;
                map.GetProvince(coordinates).RecruitablePopulation -= count;
            }

            public override void Revert(Map map) {
                base.Revert(map);
                map.GetProvince(coordinates).Population += count;
                map.GetProvince(coordinates).RecruitablePopulation += count;
            }
        }

        internal class ArmyDisbandment : TurnAction {
            private Army army;
            private int count;

            public ArmyDisbandment(Army army, int count) : base(ActionType.ArmyDisbandment,
                CostsCalculator.TurnActionApCost(ActionType.ArmyDisbandment)) {
                Debug.Log(count);
                this.army = army;
                this.count = count;
            }

            public override string desc { get => count + " units disbanded in " + army.Position; }

            public override void Execute(Map map) {
                base.Execute(map);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                map.DisbandArmy(army, count);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                map.UndoDisbandArmy(army, count);
            }
        }

        internal class RebelSuppresion : TurnAction, IInstantAction {
            private readonly Province province;
            private int oldH;
            private int oldR;
            public RebelSuppresion(Province province) :
                base(ActionType.RebelSuppresion, CostsCalculator.TurnActionApCost(ActionType.RebelSuppresion)) {
                this.province = province;
                this.oldH = province.Happiness;
                this.oldR = province.RecruitablePopulation;
            }
            public override void Preview(Map map) {
                base.Preview(map);
                province.Happiness = 40;
                province.Population -= oldR;
                province.RecruitablePopulation -= oldR;
            }
            public override void Revert(Map map) {
                base.Revert(map);
                province.Happiness = oldH;
                province.Population += oldR;
                province.RecruitablePopulation += oldR;
            }

        }
        internal class TechnologyUpgrade : TurnAction, IInstantAction {
            private readonly Technology techType;
            private readonly Country country;

            public TechnologyUpgrade(Country country, Technology techType) :
                base(ActionType.TechnologyUpgrade, CostsCalculator.TurnActionApCost(ActionType.TechnologyUpgrade)) {
                this.techType = techType;
                this.country = country;
                AltCosts = CostsCalculator.TurnActionAltCost(ActionType.TechnologyUpgrade, country.Technologies, techType);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                country.Technologies[techType]++;
                country.techStats.Calculate(country.Technologies);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                country.Technologies[techType]--;
                country.techStats.Calculate(country.Technologies);
            }
        }

        internal class BuildingUpgrade : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly BuildingType buildingType;

            public BuildingUpgrade(Province province, BuildingType buildingType) : base(ActionType.BuildingUpgrade,
                CostsCalculator.TurnActionApCost(ActionType.BuildingUpgrade)) {
                this.province = province;
                this.buildingType = buildingType;
                int upgradeLevel = province.Buildings[buildingType] + 1;
                AltCosts = CostsCalculator.TurnActionAltCost(ActionType.BuildingUpgrade, buildingType, upgradeLevel);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                province.UpgradeBuilding(buildingType);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                province.DowngradeBuilding(buildingType);
            }
        }

        internal class BuildingDowngrade : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly BuildingType buildingType;

            public BuildingDowngrade(Province province, BuildingType buildingType) : base(ActionType.BuildingDowngrade,
                CostsCalculator.TurnActionApCost(ActionType.BuildingDowngrade)) {
                this.province = province;
                this.buildingType = buildingType;
            }

            public override void Preview(Map map) {
                base.Preview(map);
                province.DowngradeBuilding(buildingType);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                province.UpgradeBuilding(buildingType);
            }
        }

        internal class FestivitiesOrganization : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly Status status;

            public FestivitiesOrganization(Province province) : base(ActionType.FestivitiesOrganization,
                CostsCalculator.TurnActionApCost(ActionType.FestivitiesOrganization)) {
                this.province = province;
                status = new Festivities(5);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                province.AddStatus(status);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                province.RemoveStatus(status);
            }
        }

        internal class TaxBreakIntroduction : TurnAction, IInstantAction {
            private readonly Province province;
            private readonly Status status;

            public TaxBreakIntroduction(Province province) : base(ActionType.TaxBreakIntroduction,
                CostsCalculator.TurnActionApCost(ActionType.TaxBreakIntroduction)) {
                this.province = province;
                status = new TaxBreak(5);
            }

            public override void Preview(Map map) {
                base.Preview(map);
                province.AddStatus(status);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                province.RemoveStatus(status);
            }
        }

        internal class WarDeclaration : TurnAction, IInstantAction {
            private Country c1, c2;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public WarDeclaration(Country c1, Country c2, diplomatic_relations_manager diplomacy,
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

            public override void Execute(Map map) {
                diplomacy.StartWar(c1, c2);
                c2.Events.Add(new Event_.DiploEvent.WarDeclared(c1, c2, diplomacy, dialog_box, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetDeclareWarRelatedButtonStates(true, c2.Id);
            }
        }

        internal class VassalIntegration : TurnAction, IInstantAction {
            private Relation.Vassalage vassalage;
            private diplomatic_relations_manager diplomacy;
            private diplomatic_actions_manager dipl_actions;

            public VassalIntegration(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy,
                diplomatic_actions_manager dipl_actions) : base(ActionType.IntegrateVassal,
                    CostsCalculator.TurnActionApCost(ActionType.IntegrateVassal, vassalage)) {
                this.vassalage = vassalage;
                this.diplomacy = diplomacy;
                this.dipl_actions = dipl_actions;
            }

            public override void Execute(Map map) {
                diplomacy.IntegrateVassal(vassalage);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetIntegrateVassalRelatedButtonStates(true, vassalage.Sides[1].Id);
            }
        }

        internal class PeaceOffer : TurnAction, IInstantAction {
            private Country offer;
            private Relation.War war;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private Country to;

            public PeaceOffer(Country offer, Relation.War war, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera) : base(ActionType.WarEnd,
                CostsCalculator.TurnActionApCost(ActionType.WarEnd)) {
                this.offer = offer;
                this.war = war;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.camera = camera;
                to = war.Sides[0] == offer ? war.Sides[1] : war.Sides[0];
            }

            public override void Execute(Map map) {
                to.Events.Add(new Event_.DiploEvent.PeaceOffer(war, offer, diplomacy, dialog_box, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetOfferPeaceRelatedButtonStates(true, to.Id);
            }
        }

        internal class AllianceOffer : TurnAction, IInstantAction {
            private Country c1, c2;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public AllianceOffer(Country c1, Country c2, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.AllianceOffer,
                    CostsCalculator.TurnActionApCost(ActionType.AllianceOffer)) {
                this.c1 = c1;
                this.c2 = c2;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
                this.camera = camera;
            }
            public override void Execute(Map map) {
                base.Execute(map);
                c2.Events.Add(new Event_.DiploEvent.AllianceOffer(c1, c2, diplomacy, dialog_box, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetOfferAllianceRelatedButtonStates(true, c2.Id);
            }
        }

        internal class AllianceBreak : TurnAction, IInstantAction {
            private Country from;
            private Relation.Alliance alliance;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private Country to;

            public AllianceBreak(Country from, Relation.Alliance alliance, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
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

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.AllianceBroken(from, to, diplomacy, dialog_box, camera));
                diplomacy.EndRelation(alliance);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetBreakAllianceRelatedButtonStates(true, to.Id);
            }
        }

        internal class MilAccessOffer : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public MilAccessOffer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.MilAccOffer,
                    CostsCalculator.TurnActionApCost(ActionType.MilAccOffer)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
                this.camera = camera;
            }
            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessOffer(from, to, diplomacy, dialog_box, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetOfferMilitaryAccessRelatedButtonStates(true, to.Id);
            }
        }

        internal class MilAccessRequest : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public MilAccessRequest(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.MilAccRequest,
                    CostsCalculator.TurnActionApCost(ActionType.MilAccRequest)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
                this.camera = camera;
            }
            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessRequest(from, to, diplomacy, dialog_box, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetRequestMilitaryAccessRelatedButtonStates(true, to.Id);
            }
        }

        internal class MilAccessEndMaster : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private MilitaryAccess militaryAccess;

            public MilAccessEndMaster(Country from, Country to, MilitaryAccess militaryAccess, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
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

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessEndMaster(militaryAccess, from, to, diplomacy, dialog_box, camera));
                diplomacy.EndRelation(militaryAccess);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetEndMilitaryAccessMasterRelatedButtonStates(true, to.Id);
            }
        }

        internal class MilAccessEndSlave : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private MilitaryAccess militaryAccess;

            public MilAccessEndSlave(Country from, Country to, MilitaryAccess militaryAccess, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
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

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.AccessEndSlave(militaryAccess, from, to, diplomacy, dialog_box, camera));
                diplomacy.EndRelation(militaryAccess);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetEndMilitaryAccessSlaveRelatedButtonStates(true, to.Id);
            }
        }

        internal class SubsOffer : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private int amount, duration;

            public SubsOffer(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
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

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.SubsOffer(from, to, diplomacy, dialog_box, amount, duration, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetSubsidizeRelatedButtonStates(true, to.Id);
            }
        }

        internal class SubsEnd : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private Subsidies subsidies;

            public SubsEnd(Country from, Country to, Subsidies subsidies, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
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

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.SubsEndMaster(from, to, diplomacy, dialog_box, camera));
                diplomacy.EndRelation(subsidies);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetEndSubsidiesRelatedButtonStates(true, to.Id);
            }
        }

        internal class SubsRequest : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;
            private int amount, duration;

            public SubsRequest(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
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

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.SubsRequest(from, to, diplomacy, dialog_box, amount, duration, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetRequestSubsidiesRelatedButtonStates(true, to.Id);
            }
        }

        internal class VassalizationDemand : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public VassalizationDemand(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.VassalizationOffer,
                    CostsCalculator.TurnActionApCost(ActionType.VassalizationOffer)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
            }

            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.VassalOffer(from, to, diplomacy, dialog_box, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetOfferVassalizationRelatedButtonStates(true, to.Id);
            }
        }

        internal class VassalRebellion : TurnAction, IInstantAction {
            private Relation.Vassalage vassalage;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public VassalRebellion(Relation.Vassalage vassalage, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.VassalRebel,
                    CostsCalculator.TurnActionApCost(ActionType.VassalRebel)) {
                this.vassalage = vassalage;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.camera = camera;
                this.dipl_actions = dipl_actions;
            }

            public override void Execute(Map map) {
                base.Execute(map);
                vassalage.Sides[0].Events.Add(new Event_.DiploEvent.VassalRebel(vassalage.Sides[1], vassalage.Sides[0], diplomacy, dialog_box, camera));
                diplomacy.EndRelation(vassalage);
                diplomacy.StartWar(vassalage.Sides[1], vassalage.Sides[0]);
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.SetVassalRebelRelatedButtonStates(true, vassalage.Sides[0].Id);
            }
        }

        internal class Insult : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public Insult(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.Insult,
                    CostsCalculator.TurnActionApCost(ActionType.Insult)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
            }

            public override void Preview(Map map) {
                base.Preview(map);
                from.SetOpinion(to.Id, from.Opinions[to.Id] - INSULT_OUR_OPINION_PENALTY_INIT);
                to.SetOpinion(from.Id, to.Opinions[from.Id] - INSULT_THEIR_OPINION_PENALTY_INIT);
            }
            public override void Revert(Map map) {
                base.Revert(map);
                from.SetOpinion(to.Id, from.Opinions[to.Id] + INSULT_OUR_OPINION_PENALTY_INIT);
                to.SetOpinion(from.Id, to.Opinions[from.Id] + INSULT_THEIR_OPINION_PENALTY_INIT);
                dipl_actions.SetInsultRelatedButtonStates(true, to.Id);
            }
        }

        internal class Praise : TurnAction, IInstantAction {
            private Country from, to;
            private diplomatic_relations_manager diplomacy;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_actions_manager dipl_actions;

            public Praise(Country from, Country to, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box,
                camera_controller camera, diplomatic_actions_manager dipl_actions) : base(ActionType.Praise,
                    CostsCalculator.TurnActionApCost(ActionType.Praise)) {
                this.from = from;
                this.to = to;
                this.diplomacy = diplomacy;
                this.dialog_box = dialog_box;
                this.dipl_actions = dipl_actions;
            }
            public override void Preview(Map map) {
                base.Preview(map);
                from.SetOpinion(to.Id, from.Opinions[to.Id] + PRAISE_OUR_OPINION_BONUS_INIT);
                to.SetOpinion(from.Id, to.Opinions[from.Id] + PRAISE_THEIR_OPINION_BONUS_INIT);
            }
            public override void Revert(Map map) {
                base.Revert(map);
                from.SetOpinion(to.Id, from.Opinions[to.Id] - PRAISE_OUR_OPINION_BONUS_INIT);
                to.SetOpinion(from.Id, to.Opinions[from.Id] - PRAISE_THEIR_OPINION_BONUS_INIT);
                dipl_actions.SetDiplomaticMissionRelatedButtonStates(true, to.Id);
            }
        }

        internal class CallToWar : TurnAction, IInstantAction {
            private Country from, to;
            private War war;
            private dialog_box_manager dialog_box;
            private camera_controller camera;
            private diplomatic_relations_manager diplomacy;
            private diplomatic_actions_manager dipl_actions;

            public CallToWar(Country from, Country to, War war, dialog_box_manager dialog_box,
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
            public override void Execute(Map map) {
                base.Execute(map);
                to.Events.Add(new Event_.DiploEvent.CallToWar(from, to, diplomacy, dialog_box, war, camera));
            }

            public override void Revert(Map map) {
                base.Revert(map);
                dipl_actions.RevertCallToWarRelatedButtonStates(to.Id);
            }
        }
    }
    internal interface IInstantAction { }
    public class ActionContainer {
        private Map map;
        private List<TurnAction> actions;

        public ActionContainer(Map map) {
            this.map = map;
            actions = new List<TurnAction>();
        }

        public List<TurnAction> ExtractInstants() {
            List<TurnAction> instants = new List<TurnAction>();
            if (actions != null) {
                instants = actions.FindAll(a => a is IInstantAction);
            }
            actions = actions.Except(instants).ToList();
            return instants;
        }

        public TurnAction last { get => actions[actions.Count - 1]; }

        public int Count { get { return actions.Count; } }

        public void AddAction(TurnAction action) {
            action.cRes = map.Countries[map.CurrentPlayerId].Resources;
            actions.Add(action);
            actions.Last().Preview(map);
        }

        public void Execute() {
            if(actions.Count == 0) return;
            actions[0].Execute(map);
            actions.RemoveAt(0);
        }

        public void Revert() {
            if(actions.Count>0){
                last.Revert(map);
                actions.Remove(last);
            }
        }
    }
}