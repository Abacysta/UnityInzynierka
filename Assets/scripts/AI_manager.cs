using Assets.classes;
using Assets.classes.subclasses;
using Assets.classes.Tax;
using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using UnityEditor;
using UnityEngine;
using static Assets.classes.Event_;

namespace Assets.Scripts {
    internal class AI_manager: MonoBehaviour {
        internal enum Humor {
            /// <summary>
            /// Base so that ai doesn't go rouge
            /// </summary>
            Null,
            /// <summary>
            /// country is overall the most powerfull one on the map and power projects
            /// </summary>
            Leading,
            /// <summary>
            /// country wants to expand offensively
            /// </summary>
            Offensive,
            /// <summary>
            /// country wants to defend against all external threats at all costs
            /// </summary>
            Defensive,
            /// <summary>
            /// country wants to rebel against its senior (VASSAL ONLY)
            /// </summary>
            Rebellious,
            /// <summary>
            /// country wants to appease its senior (VASSAL ONLY)
            /// </summary>
            Subservient,
            /// <summary>
            /// country has no special objectives
            /// </summary>
            Normal
        }
        [SerializeField] private Map map;
        [SerializeField] private random_events_manager random;
        [SerializeField] private diplomatic_relations_manager diplo;
        [SerializeField] private dialog_box_manager dialog_box;
        [SerializeField] private camera_controller camera;
        [SerializeField] private diplomatic_actions_manager diplo_actions;
        /// <summary>
        /// Every turn AI lead country will asess its situation in the world and change its behavior to suit it the best.
        /// </summary>
        private Humor humor;

        /// <summary>
        /// main method responsible for coordinating behavior of AI country's turn
        /// </summary>
        public void Behave() {
            if(map.CurrentPlayer.Id!=0){
                this.humor = Asess();
            //events block
            while (map.CurrentPlayer.Events.Count > 0) { 
                var e = map.CurrentPlayer.Events.Last();
                RespondToEvent(e);
                map.CurrentPlayer.Events.Remove(e);
            }
            ManageInternal();
            Diplomacy();
            MoveArmies();
                this.humor = Humor.Null;
            }
        }

        /// <summary>
        /// decides on value of humor of AI this turn
        /// </summary>
        private Humor Asess() {
            //first block; vassal comparison with senior
            var vassalage = map.GetRelationsOfType(map.CurrentPlayer, Relation.RelationType.Vassalage).FirstOrDefault(rel => rel.Sides[1] == map.CurrentPlayer);
            if (vassalage != null) {
                var senior = vassalage.Sides[0];
                var vassal = vassalage.Sides[1];
                if (GetArmySum(senior.Id) <= GetArmySum(vassal.Id) //army bigger than seniors
                    || senior.Provinces.Count <= (int)(0.5*vassal.Provinces.Count)//provinces count bigger than half of seniors
                    || vassal.Opinions[senior.Id] < -50) //negative opinion of senior
                    return Humor.Rebellious;
                return Humor.Subservient;
            }
            vassalage = null;
            //second block; war related extremes
            var wars = map.GetRelationsOfType(map.CurrentPlayer, Relation.RelationType.War).Cast<Relation.War>();
            int severity = 0;
            if (wars != null) {
                foreach (var war in wars) {
                    bool attacker = war.Participants1.Contains(map.CurrentPlayer);
                    var allies = attacker ? war.Participants1 : war.Participants2;
                    var enemies = attacker ? war.Participants2 : war.Participants1;
                    int allyCount = allies.Sum(a => GetArmySum(a.Id));
                    int enemyCount = enemies.Sum(e => GetArmySum(e.Id));
                    severity += allyCount - enemyCount;
                }
                if(severity > 300) {
                    return Humor.Offensive;
                }
                else if(severity < -300) {
                    return Humor.Defensive;
                }
            }
            wars = null;
            //third block; overall competetiveness (vassal count and gold production)
            var vassals = map.GetRelationsOfType(map.CurrentPlayer, Relation.RelationType.Vassalage).Where(r => r.Sides[0] == map.CurrentPlayer).Cast<Relation.Vassalage>();
            var topproductions = map.Countries
                .Where(c => c.Id != 0)
                .Select(c => c.GetResourcesGain(map))
                .SelectMany(d => d)
                .Where(k => k.Key == Resource.Gold)
                .GroupBy(k => k.Key)
                .Select(group => new KeyValuePair<Resource, float>(group.Key, group.Sum(k => k.Value)))
                .OrderByDescending(k => k.Value)
                .Take(2) // adjust this as needed
                .ToDictionary(k => k.Key, k => k.Value);
            var production = map.CurrentPlayer.GetResourcesGain(map)[Resource.Gold];
            if(vassals.Count() > 2//has 3 or more vassals
                || topproductions.ContainsValue(production)//is in the 2 top gold producing countries
                ) {
                return Humor.Leading;
            }
            vassals = null;
            return Humor.Normal;
        }

        
        
        private void RespondToEvent(Event_ e) {
            if (!(e is Event_.DiploEvent)) {
                var r = random.chance;
                if (r > 50) e.Accept();
                else e.Reject();
            }
            else {
                AI_manager.diploEventResponder.Respond(e, map, humor);
            }
        }
        //no moving over water unless enemy land cuz i dont care at this point(splitting is too complicated as well)
        private void MoveArmies() {
            var armies = map.GetCountryArmies(map.CurrentPlayer).OrderByDescending(a=>a.Count).ToList();
            var unavailable = Map.LandUtilites.GetUnpassableProvinces(map, map.CurrentPlayer);
            var enemyIds = Map.WarUtilities.GetEnemyIds(map, map.CurrentPlayer);
            foreach(var a in armies) {
                if (map.CurrentPlayer.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.ArmyMove))) break;
                //get all land- no water
                var possible = map.GetPossibleMoveCells(a).Where(c => map.GetProvince(c).IsLand).ToList();
                //trbal first I guess
                var target = possible.FirstOrDefault(p => map.GetProvince(p).OwnerId == 0);
                if(target != (0,0)) {
                    map.CurrentPlayer.Actions.AddAction(new TurnAction.ArmyMove(a.Position, target, a.Count, a));
                    continue;
                }
                //then enemy provinces
                var Eprov = possible.FindAll(p => Map.WarUtilities.GetEnemyIds(map, map.CurrentPlayer).Contains(map.GetProvince(p).OwnerId));
                //no armies first
                var withnoarmies = Eprov.Where(p => !Map.WarUtilities.GetEnemyArmies(map, map.CurrentPlayer).Select(a => a.Position).Contains(p));
                if (withnoarmies.Any()) {
                    map.CurrentPlayer.Actions.AddAction(new TurnAction.ArmyMove(a.Position, target, a.Count, a));
                    continue;
                }
                //with smaller armies
                Eprov = Eprov.Except(withnoarmies).ToList();
                foreach (var pp in Eprov) {
                    if (Map.WarUtilities.GetEnemyArmiesInProvince(map, map.CurrentPlayer, map.GetProvince(pp)).Sum(a => a.Count) < a.Count) {
                        map.CurrentPlayer.Actions.AddAction(new TurnAction.ArmyMove(a.Position, pp, a.Count, a));
                        break;
                    }
                }
                //get nearest enemy pos
                var nearestProv = HexUtils.GetNearestProvince(map, a.Position, enemyIds);
                if (nearestProv != null) {
                    //if exists get best path
                    var bestPath = HexUtils.GetBestPathProvinces(map, map.CurrentPlayer, unavailable.Select(p=>(p.X, p.Y)).ToHashSet(), a.Position, (nearestProv.X, nearestProv.Y));
                    if (bestPath != null) {
                        //if exists move
                        map.CurrentPlayer.Actions.AddAction(new TurnAction.ArmyMove(a.Position, bestPath[1].Coordinates, a.Count, a));
                    }
                }
            }
            
        }
        /// <summary>
        /// the way AI acts diplomaticly this turn
        /// </summary>
        private void Diplomacy() {
            var toolbox = (diplo, dialog_box, camera, diplo_actions);
            diplomacyManager.ManageAccess(map, map.CurrentPlayer, diplo, dialog_box, camera, diplo_actions);
            switch (humor) {
                case Humor.Leading:
                    diplomacyManager.LeadingDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                case Humor.Defensive:
                    diplomacyManager.DefensiveDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                case Humor.Subservient:
                    diplomacyManager.SubservientDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                case Humor.Rebellious:
                    diplomacyManager.RebelliousDiplo(map, map.CurrentPlayer, toolbox, random);
                    break;
                case Humor.Offensive:
                    diplomacyManager.OffensiveDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                default:
                    diplomacyManager.DefaultDiplo(map, map.CurrentPlayer, toolbox, random);
                    break;
            }
        }
        /// <summary>
        /// responsible for taking care of internal affairs
        /// different humors might have different squences they will take
        /// </summary>
        private void ManageInternal() {
            internalAffairsManager.HandleTax(map, map.CurrentPlayer, humor);
            if (humor == Humor.Defensive || humor == Humor.Offensive) internalAffairsManager.HandleArmyRecruitment(map.CurrentPlayer, humor);
            internalAffairsManager.HandleUnhappy(map.CurrentPlayer, humor);
            if (humor == Humor.Leading) internalAffairsManager.HandleGrowable(map.CurrentPlayer, humor);
            internalAffairsManager.HandleTechnology(map.CurrentPlayer, humor);
            internalAffairsManager.HandleBuildings(map.CurrentPlayer, humor);
            if (humor != Humor.Defensive && humor != Humor.Offensive) internalAffairsManager.HandleArmyRecruitment(map.CurrentPlayer, humor);
            if(humor != Humor.Leading) internalAffairsManager.HandleGrowable(map.CurrentPlayer, humor);
        }


        private int GetArmySum(int id) {
            return map.Armies.FindAll(a=>a.OwnerId == id).Sum(a=> a.Count);
        }
        private class diplomacyManager {
            //1.get needed access; 2. ask for it; 3....; 4.profit
            public static void ManageAccess(Map map, Country c, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, camera_controller camera, diplomatic_actions_manager diplo_actions) {
                var impassable = Map.LandUtilites.GetUnpassableProvinces(map, c);
                foreach(var e in Map.WarUtilities.GetEnemyIds(map, c).Select(id => map.Countries[id]).ToList()) {
                    var needed = HexUtils.GetBestPathProvinces(map, c, c.Capital, e.Capital);
                    if(needed != null) foreach(var p in needed) {
                        if(impassable.Contains(p) && p.OwnerId != 0 && p.IsLand) {
                            if (map.Countries[p.OwnerId].Opinions[c.Id] >= 0) {
                                    c.Actions.AddAction(new TurnAction.MilAccessRequest(c, map.Countries[p.OwnerId], diplomacy, dialog_box, camera, diplo_actions));
                            }
                        }
                    }
                }
            }
            public static void LeadingDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) {
                
                var vassalages = Map.PowerUtilites.GetVassalRelations(map, c);
                foreach(var v in vassalages) {
                    var integration = new TurnAction.VassalIntegration(v, toolbox.Item1, toolbox.Item4);
                    if(c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.IntegrateVassal, v))) {
                        c.Actions.AddAction(integration);
                    }
                }
                var vassals = Map.PowerUtilites.GetVassals(map, c);
                foreach(var v in vassals) {
                    var improvement = new TurnAction.Praise(c, v, toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4);
                    if (c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.Praise))) {
                        c.Actions.AddAction(improvement);
                    }
                }
                var weaklings = Map.PowerUtilites.GetWeakCountries(map, c);
                foreach(var w in weaklings) {
                    var threat = new TurnAction.VassalizationDemand(c, w, toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4);
                    if (c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.VassalizationOffer))) {
                        c.Actions.AddAction(threat);
                    }
                }
            }
            public static void DefensiveDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) {
                var wars = Map.WarUtilities.GetAllWars(map, c);
                var allies = Map.WarUtilities.GetAllies(map, c);
                foreach(var war in wars) {
                    foreach(var ally in allies) {
                        var call = new TurnAction.CallToWar(c, ally, war, toolbox.Item2, toolbox.Item1, toolbox.Item3, toolbox.Item4);
                        if (c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.CallToWar))) {
                            c.Actions.AddAction(call);
                        }
                    }
                }
            }
            public static void OffensiveDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) {
                //cant think of anything so just call all allies like defensive
                DefensiveDiplo(map, c, toolbox);
            }
            public static void SubservientDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) { 
                //cant think of anything
            }
            public static void RebelliousDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox, random_events_manager random) { 
                if(Map.PowerUtilites.HowArmyStronger(map, c, Map.PowerUtilites.GetSenior(map, c)) >= 0.3) {
                    if(random.chance <= 10) {
                        c.Actions.AddAction(new TurnAction.VassalRebellion(Map.PowerUtilites.GetVassalage(map, c), toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                    }
                }
            }
            public static void DefaultDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox, random_events_manager random) { 
                for(int i = 1; i < map.Countries.Count; i++) {
                    if(i == c.Id) continue;
                    //just make them do something, even if it is very retarded and chaotic
                    if(c.Opinions[i] <= -100 && random.chance <= 25) {
                        c.Actions.AddAction(new TurnAction.WarDeclaration(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                        continue;
                    }
                    if(c.Opinions[i] >= 100 && random.chance <= 25) {
                        c.Actions.AddAction(new TurnAction.AllianceOffer(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                        continue;
                    }
                    var rnd = random.chance;
                    if(rnd <= 25) {
                        c.Actions.AddAction(new TurnAction.Praise(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                    }
                    else if(rnd >= 75) {
                        c.Actions.AddAction(new TurnAction.Insult(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                    }
                }
            }
        }
        private class internalAffairsManager {
            public static void HandleUnhappy(Country c, Humor humor) {
                var unhappy = Map.LandUtilites.GetUnhappyProvinces(c);
                var veryBad = unhappy.FindAll(p => p.Happiness <= 30).OrderBy(p=>p.Happiness).ToList();
                var handlable = unhappy.FindAll(p=>p.Happiness >30).OrderBy(p=>p.Happiness).ToList();
                //tax break on provinces with low chance of rebellion
                while (handlable.Count > 0) {
                    if (c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.TaxBreakIntroduction))) {
                        c.Actions.AddAction(new TurnAction.TaxBreakIntroduction(handlable[0]));
                        handlable.RemoveAt(0);
                    }
                    else break;
                }
                //if can suppress, will for dire provinces
                if(c.TechStats.CanRebelSupp) while(veryBad.Count > 0) {
                    if (c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.RebelSuppresion))) {
                        c.Actions.AddAction(new TurnAction.RebelSuppresion(veryBad[0]));
                        veryBad.RemoveAt(0);
                    }
                    else break;
                }
            }
            public static void HandleGrowable(Country c, Humor humor) {
                var growable = Map.LandUtilites.GetGrowable(c);
                int limit = humor == Humor.Leading ? c.Provinces.Count/10 : c.Provinces.Count/20;//10 and 5 % respecitvely
                foreach(var p in growable) {
                    if (c.CanAfford(CostsCalculator.GetTurnActionFullCost(TurnAction.ActionType.FestivitiesOrganization))) {
                        c.Actions.AddAction(new TurnAction.FestivitiesOrganization(p));
                    }
                    else break;
                }
            }
            public static void HandleArmyRecruitment(Country c, Humor humor) {
                var toRecruit = new List<Province>();
                if(humor == Humor.Defensive) {
                    toRecruit = c.Provinces.ToList().OrderByDescending(p=>p.RecruitablePopulation).ToList();
                }
                else {
                    toRecruit= Map.LandUtilites.GetOptimalRecruitmentProvinces(c);
                }
                bool exitF = false;
                foreach(var p in toRecruit) {
                    if (exitF || c.Resources[Resource.AP] < 1)
                        break;
                    exitF = !Map.LandUtilites.RecruitAllAvailable(c, p);
                }
            }
            public static void HandleTax(Map map, Country c, Humor humor) {
                if (humor == Humor.Defensive && humor == Humor.Offensive) {
                    if (c.TechStats.LvlTax >= 1) {
                        if (!(c.Tax is WarTaxes))
                            c.Tax = new WarTaxes();
                        else
                            SetProperTax(map, c);
                    }
                }
                else if(humor == Humor.Leading) {
                    if (c.TechStats.LvlTax >= 2) {
                        c.Tax = new InvesmentTaxes();
                        float tax = Map.PowerUtilites.GetTaxGain(c), upkeep = Map.PowerUtilites.GetArmyUpkeep(map, c);
                        if(tax < upkeep) {
                            SetProperTax(map, c);
                        }

                    }
                }
                else {
                    SetProperTax(map, c);
                }
            }
            public static void SetProperTax(Map map, Country c) {
                c.Tax = new LowTaxes();
                float tax = Map.PowerUtilites.GetTaxGain(c), upkeep = Map.PowerUtilites.GetArmyUpkeep(map, c);
                if(tax < upkeep) {
                    c.Tax = new MediumTaxes();
                    tax = Map.PowerUtilites.GetTaxGain(c); upkeep = Map.PowerUtilites.GetArmyUpkeep(map, c);
                    if(tax < upkeep) {
                        c.Tax = new HighTaxes();
                    }
                }
            }
            //AI should rush adm4(taxbreak) so it doesn't collapse immideately(rebel suppresion is too far in administratice tree so good luck AI you're gonna need it)
            public static void HandleTechnology(Country c, Humor humor) {
                if (c.Technologies[Technology.Administrative] < 4) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Administrative);
                    if(c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else if(humor == Humor.Defensive || humor == Humor.Offensive || humor == Humor.Rebellious) {
                    TechMeaPrio(c);
                }
                else if(humor == Humor.Leading || humor == Humor.Subservient) {
                    TechAemPrio(c);
                }
                else {
                    TechMaePrio(c);
                }
            }
            /// <summary>
            /// Priority Military -> Economic -> Administrative
            /// Defensive, Offensive, Rebellious
            /// </summary>
            /// <param name="c"></param>
            private static void TechMeaPrio(Country c) {
                var tech = c.Technologies;
                if (tech[Technology.Military] - tech[Technology.Administrative] < 2 && tech[Technology.Military] - tech[Technology.Economic] < 2) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Military);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else if (tech[Technology.Economic] - tech[Technology.Administrative] > 1) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Administrative);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
            }
            /// <summary>
            /// Priority Administrative -> Economic -> Military
            /// Leading, Subservient
            /// </summary>
            /// <param name="c"></param>
            private static void TechAemPrio(Country c) {
                var tech = c.Technologies;
                if (tech[Technology.Administrative] - tech[Technology.Economic] < 2 && tech[Technology.Administrative] - tech[Technology.Military] < 2) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Administrative);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else if (tech[Technology.Economic] - tech[Technology.Military] > 1) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Military);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
            }
            /// <summary>
            /// Priority Military -> Administrative -> Economic
            /// _default
            /// </summary>
            /// <param name="c"></param>
            private static void TechMaePrio(Country c) {
                var tech = c.Technologies;
                if (tech[Technology.Military] - tech[Technology.Administrative] < 2 && tech[Technology.Military] - tech[Technology.Economic] < 2) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Military);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else if (tech[Technology.Administrative] - tech[Technology.Economic] > 1) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
                else {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.CanAfford(Resource.AP, action.ApCost) && c.CanAfford(action.AltCosts)) {
                        c.Actions.AddAction(action);
                    }
                }
            }
            //to simplify things
            //up to 2 infrastructures per turn
            //up to 1 anything else
            //if no school yet prioritize school over anything else
            //smartest thing is probably mine -> infrastructure -> school -> fort
            public static void HandleBuildings(Country c, Humor humor) {
                TurnAction.BuildingUpgrade upgrade;
                if (c.GetCapital().Buildings[BuildingType.Infrastructure] == 0) {
                    upgrade = new(c.GetCapital(), BuildingType.Infrastructure);
                    if (c.CanAfford(upgrade.AltCosts))
                        c.Actions.AddAction(upgrade);
                }
                //mine block
                List<Province> toImprove = GetMineable(c);
                if(toImprove.Count>1){
                    upgrade = new(toImprove[0], BuildingType.Mine);
                    if(c.CanAfford(upgrade.AltCosts))
                        c.Actions.AddAction(upgrade);
                }
                //infr block
                toImprove = GetInfrastructurable(c);
                if(toImprove.Count>2){
                upgrade = new(toImprove[1], BuildingType.Infrastructure);
                if (c.CanAfford(upgrade.AltCosts))
                    c.Actions.AddAction(upgrade);
                upgrade = new(toImprove[0], BuildingType.Infrastructure);
                    if(c.CanAfford(upgrade.AltCosts))
                        c.Actions.AddAction(upgrade);
                }
                //school block
                toImprove = GetSchoolable(c);
                if(toImprove.Count>1){
                upgrade = new(toImprove[0], BuildingType.School);
                    if(c.CanAfford(upgrade.AltCosts))
                        c.Actions.AddAction(upgrade);
                }
                //fort block
                toImprove = GetFortable(c);
                if(toImprove.Count>1){
                    upgrade = new(toImprove[0], BuildingType.Fort);
                    if(c.CanAfford(upgrade.AltCosts))
                        c.Actions.AddAction(upgrade);
                }
            }
            //infr limits for ai adm<2-1; adm<5-2; adm<7-3
            //mainly to make them build other stuff as well
            //getting lists cause I might increase limits(who knows)
            private static List<Province> GetInfrastructurable(Country c) {
                int mode;
                var adm = c.Technologies[Technology.Administrative];
                if (adm < 2) mode = 1;
                else if (adm < 5) mode = 2;
                else mode = 3;
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.Infrastructure, out int level) && level < mode).OrderByDescending(p=>p.Population).ToList();
            }
            private static List<Province> GetFortable(Country c) {
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.Fort, out int level) && level < ( c.TechStats.LvlFort+1)).OrderByDescending(p=>p.Population).ToList();
            }
            private static List<Province> GetMineable(Country c) {
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.Mine, out int level) && level < (c.TechStats.LvlMine + 1)).OrderByDescending(p => p.Population).ToList();
            }
            //ograniczenie do 2 z lenistwa xd
            private static List<Province> GetSchoolable(Country c) {
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.School, out int level) && level < (c.TechStats.MoreSchool ? 2 : 1)).OrderByDescending(p=> p.Population).ToList();
            }
        }
        private class diploEventResponder {
            public static void Respond(Event_ e, Map map, Humor humor) {
                Type t = e.GetType();
                var method = typeof(diploEventResponder).GetMethod("Respond", new[] { t, typeof(Map), typeof(Humor) });
                if (method != null) {
                    method.Invoke(null, new object[] { e, map, humor });
                } else {
                    Debug.LogError("Diplo events cause problems once again. This time ai can't respond to one.");
                }
            }

            public static void Respond(DiploEvent.WarDeclared e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.PeaceOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Defensive:
                        e.Accept(); break;
                    case Humor.Offensive: 
                        e.Reject(); break;
                    case Humor.Leading:
                        //check if armies of the main opponent are overwhelmingly big
                        if (map.GetCountryArmies(e.From).Count >= 1.2 * (double)map.GetCountryArmies(e.To).Count)
                            e.Accept();
                        else 
                            e.Reject();
                        break;
                    default:
                        if (map.GetCountryArmies(e.From).Count >= 0.8 * (double)map.GetCountryArmies(e.To).Count)
                            e.Accept();
                        else
                            e.Reject();
                        break;
                }
            }

            public static void Respond(DiploEvent.CallToWar e, Map map, Humor humor) {
                var war = e.War;
                switch (humor) {
                    case Humor.Leading:
                        e.Accept(); break;
                    case Humor.Defensive:
                        e.Reject(); break;
                    case Humor.Offensive:
                        if (Map.WarUtilities.IsAttacker(map, e.From, war)) {
                            if (Map.WarUtilities.IsAttackersStronger(map, war))
                                e.Accept();
                            else e.Reject();
                        }
                        else {
                            if (Map.WarUtilities.IsAttackersStronger(map, war))
                                e.Reject();
                            else e.Accept();
                        }
                        break;
                    case Humor.Subservient: e.Accept(); break;
                    case Humor.Rebellious: e.Accept(); break;
                    default:
                        var powers = Map.WarUtilities.GetSidePowers(map, war);
                        if ((Map.WarUtilities.IsAttacker(map, e.From, war) && powers.Item1 > 0.6 * powers.Item2) || !Map.WarUtilities.IsAttacker(map, e.From, war))
                            e.Accept();
                        else e.Reject();
                        break;
                }
            }

            public static void Respond(DiploEvent.TruceEnd e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.AllianceOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        e.Reject();
                        break;
                    case Humor.Defensive:
                        e.Accept();
                        break;
                    case Humor.Subservient:
                        e.Reject();
                        break;
                    case Humor.Rebellious:
                        e.Reject(); break;
                    case Humor.Offensive:
                        
                    default:
                        if (e.To.Opinions[e.From.Id] > 150 || (e.To.Opinions[e.From.Id] > 75 && map.GetCountryArmies(e.From).Count > map.GetCountryArmies(e.To).Count)) {
                            e.Accept();
                        }
                        else e.Reject();
                        break;
                }
            }

            public static void Respond(DiploEvent.AllianceAccepted e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.AllianceDenied e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.AllianceBroken e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.SubsOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        e.Reject(); break;
                    case Humor.Subservient:
                        e.Reject(); break;
                    case Humor.Rebellious:
                        e.Accept(); break;
                    case Humor.Defensive:
                        e.Accept(); break;
                    case Humor.Offensive:
                        if (map.Countries[e.To.Id].Opinions[e.From.Id] > 100) {
                            e.Accept();
                        }
                        else e.Reject();
                        break;
                    default:
                        if (Map.PowerUtilites.GetOpinion(e.From, e.To) > 0)
                            e.Accept();
                        else e.Reject();
                        break;
                }
            }

            public static void Respond(DiploEvent.SubsRequest e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        if(Map.PowerUtilites.GetOpinion(e.From, e.To) > 100 && e.Amount > 0.05f*Map.PowerUtilites.GetGoldGain(map, e.To)) {
                            e.Accept();
                        }
                        e.Reject();
                        break;
                    case Humor.Offensive:
                        e.Reject();
                        break;
                    case Humor.Defensive:
                        e.Reject();
                        break;
                    default:
                        if (Map.PowerUtilites.GetGoldGain(map, e.To) > 0.2f * e.Amount && Map.PowerUtilites.GetOpinion(e.From, e.To) > 150) {
                            e.Accept();
                        }
                        else e.Reject();
                        break;
                }
            }

            public static void Respond(DiploEvent.SubsEndMaster e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.SubsEndSlave e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.AccessOffer e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.AccessRequest e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        if (Map.PowerUtilites.HowArmyStronger(map, e.To, e.From) <= 1.1f) {
                            e.Reject();
                        }
                        else e.Accept();
                        break;
                    case Humor.Offensive:
                        e.Reject();
                        break;
                    case Humor.Subservient:
                        if(map.HasRelationOfType(map.GetSeniorIfExists(e.To), e.From, Relation.RelationType.MilitaryAccess)) {
                            e.Accept();
                        } else e.Reject();
                        break;
                    case Humor.Rebellious:
                        if (map.HasRelationOfType(map.GetSeniorIfExists(e.To), e.From, Relation.RelationType.MilitaryAccess)) {
                            e.Accept();
                        }
                        else e.Reject();
                        break;
                    case Humor.Defensive:
                        e.Accept();
                        break;
                    default:
                        e.Accept(); break;
                }
            }

            public static void Respond(DiploEvent.AccessEndMaster e, Map map, Humor humor) {
                e.Accept();
            }

            public static void Respond(DiploEvent.AccessEndSlave e, Map map, Humor humor) {
                e.Accept(); 
            }

            public static void Respond(DiploEvent.VassalOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        e.Reject();
                        break;
                    case Humor.Offensive:
                        e.Reject(); break;
                    case Humor.Defensive:
                        e.Accept(); break;
                    default:
                        if(Map.PowerUtilites.GetOpinion(e.From, e.To)> 175) {
                            e.Accept();
                        }
                        else if(Map.PowerUtilites.HowArmyStronger(map, e.From, e.To) > 2) {
                            e.Accept();
                        }
                        else {
                            e.Reject();
                        }
                        break;
                }
            }

            public static void Respond(DiploEvent.VassalRebel e, Map map, Humor humor) {
                e.Accept();
            }

        }
    }
}
