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
        public void behave() {
            if(map.CurrentPlayer.Id!=0){
                this.humor = asess();
            //events block
            while (map.CurrentPlayer.Events.Count > 0) { 
                var e = map.CurrentPlayer.Events.Last();
                respondToEvent(e);
                map.CurrentPlayer.Events.Remove(e);
            }
            manageInternal();
            diplomacy();
            moveArmies();
                this.humor = Humor.Null;
            }
        }

        /// <summary>
        /// decides on value of humor of AI this turn
        /// </summary>
        private Humor asess() {
            //first block; vassal comparison with senior
            var vassalage = map.getRelationsOfType(map.CurrentPlayer, Relation.RelationType.Vassalage).FirstOrDefault(rel => rel.Sides[1] == map.CurrentPlayer);
            if (vassalage != null) {
                var senior = vassalage.Sides[0];
                var vassal = vassalage.Sides[1];
                if (getArmySum(senior.Id) <= getArmySum(vassal.Id) //army bigger than seniors
                    || senior.Provinces.Count <= (int)(0.5*vassal.Provinces.Count)//provinces count bigger than half of seniors
                    || vassal.Opinions[senior.Id] < 0) //negative opinion of senior
                    return Humor.Rebellious;
                return Humor.Subservient;
            }
            vassalage = null;
            //second block; war related extremes
            var wars = map.getRelationsOfType(map.CurrentPlayer, Relation.RelationType.War).Cast<Relation.War>();
            int severity = 0;
            if (wars != null) {
                foreach (var war in wars) {
                    bool attacker = war.participants1.Contains(map.CurrentPlayer);
                    var allies = attacker ? war.participants1 : war.participants2;
                    var enemies = attacker ? war.participants2 : war.participants1;
                    int allyCount = allies.Sum(a => getArmySum(a.Id));
                    int enemyCount = enemies.Sum(e => getArmySum(e.Id));
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
            var vassals = map.getRelationsOfType(map.CurrentPlayer, Relation.RelationType.Vassalage).Where(r => r.Sides[0] == map.CurrentPlayer).Cast<Relation.Vassalage>();
            var topproductions = map.Countries
                .Where(c => c.Id != 0)
                .Select(c => map.getResourceGain(c))
                .SelectMany(d => d)
                .Where(k => k.Key == Resource.Gold)
                .GroupBy(k => k.Key)
                .Select(group => new KeyValuePair<Resource, float>(group.Key, group.Sum(k => k.Value)))
                .OrderByDescending(k => k.Value)
                .Take(2) // adjust this as needed
                .ToDictionary(k => k.Key, k => k.Value);
            var production = map.getResourceGain(map.CurrentPlayer)[Resource.Gold];
            if(vassals.Count() > 2//has 3 or more vassals
                || topproductions.ContainsValue(production)//is in the 2 top gold producing countries
                ) {
                return Humor.Leading;
            }
            vassals = null;
            return Humor.Normal;
        }

        
        
        private void respondToEvent(Event_ e) {
            if (!(e is Event_.DiploEvent)) {
                var r = random.chance;
                if (r > 50) e.accept();
                else e.reject();
            }
            else {
                AI_manager.diploEventResponder.Respond(e, map, humor);
            }
        }
        //no moving over water unless enemy land cuz i dont care at this point(splitting is too complicated as well)
        private void moveArmies() {
            var armies = map.getCountryArmies(map.CurrentPlayer).OrderByDescending(a=>a.Count).ToList();
            var unavailable = Map.LandUtilites.getUnpassableProvinces(map, map.CurrentPlayer);
            var enemyIds = Map.WarUtilities.getEnemyIds(map, map.CurrentPlayer);
            foreach(var a in armies) {
                if (map.CurrentPlayer.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.ArmyMove))) break;
                //get all land- no water
                var possible = map.getPossibleMoveCells(a).Where(c => map.getProvince(c).IsLand).ToList();
                //trbal first I guess
                var target = possible.FirstOrDefault(p => map.getProvince(p).OwnerId == 0);
                if(target != (0,0)) {
                    map.CurrentPlayer.Actions.addAction(new TurnAction.ArmyMove(a.Position, target, a.Count, a));
                    continue;
                }
                //then enemy provinces
                var Eprov = possible.FindAll(p => Map.WarUtilities.getEnemyIds(map, map.CurrentPlayer).Contains(map.getProvince(p).OwnerId));
                //no armies first
                var withnoarmies = Eprov.Where(p => !Map.WarUtilities.getEnemyArmies(map, map.CurrentPlayer).Select(a => a.Position).Contains(p));
                if (withnoarmies.Any()) {
                    map.CurrentPlayer.Actions.addAction(new TurnAction.ArmyMove(a.Position, target, a.Count, a));
                    continue;
                }
                //with smaller armies
                Eprov = Eprov.Except(withnoarmies).ToList();
                foreach (var pp in Eprov) {
                    if (Map.WarUtilities.getEnemyArmiesInProvince(map, map.CurrentPlayer, map.getProvince(pp)).Sum(a => a.Count) < a.Count) {
                        map.CurrentPlayer.Actions.addAction(new TurnAction.ArmyMove(a.Position, pp, a.Count, a));
                        break;
                    }
                }
                //get nearest enemy pos
                var nearestProv = HexUtils.getNearestProvince(map, a.Position, enemyIds);
                if (nearestProv != null) {
                    //if exists get best path
                    var bestPath = HexUtils.getBestPathProvinces(map, map.CurrentPlayer, unavailable.Select(p=>(p.X, p.Y)).ToHashSet(), a.Position, (nearestProv.X, nearestProv.Y));
                    if (bestPath != null) {
                        //if exists move
                        map.CurrentPlayer.Actions.addAction(new TurnAction.ArmyMove(a.Position, bestPath[1].coordinates, a.Count, a));
                    }
                }
            }
            
        }
        /// <summary>
        /// the way AI acts diplomaticly this turn
        /// </summary>
        private void diplomacy() {
            var toolbox = (diplo, dialog_box, camera, diplo_actions);
            diplomacyManager.manageAccess(map, map.CurrentPlayer, diplo, dialog_box, camera, diplo_actions);
            switch (humor) {
                case Humor.Leading:
                    diplomacyManager.leadingDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                case Humor.Defensive:
                    diplomacyManager.defensiveDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                case Humor.Subservient:
                    diplomacyManager.subservientDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                case Humor.Rebellious:
                    diplomacyManager.rebelliousDiplo(map, map.CurrentPlayer, toolbox, random);
                    break;
                case Humor.Offensive:
                    diplomacyManager.offensiveDiplo(map, map.CurrentPlayer, toolbox);
                    break;
                default:
                    diplomacyManager.defaultDiplo(map, map.CurrentPlayer, toolbox, random);
                    break;
            }
        }
        /// <summary>
        /// responsible for taking care of internal affairs
        /// different humors might have different squences they will take
        /// </summary>
        private void manageInternal() {
            internalAffairsManager.setProperTax(map, map.CurrentPlayer);
            if (humor == Humor.Defensive || humor == Humor.Offensive) internalAffairsManager.handleArmyRecruitment(map.CurrentPlayer, humor);
            internalAffairsManager.handleUnhappy(map.CurrentPlayer, humor);
            if (humor == Humor.Leading) internalAffairsManager.handleGrowable(map.CurrentPlayer, humor);
            internalAffairsManager.handleTechnology(map.CurrentPlayer, humor);
            internalAffairsManager.handleBuildings(map.CurrentPlayer, humor);
            if (humor != Humor.Defensive || humor == Humor.Offensive) internalAffairsManager.handleArmyRecruitment(map.CurrentPlayer, humor);
            if(humor != Humor.Leading) internalAffairsManager.handleGrowable(map.CurrentPlayer, humor);
        }


        private int getArmySum(int id) {
            return map.Armies.FindAll(a=>a.OwnerId == id).Sum(a=> a.Count);
        }
        private class diplomacyManager {
            //1.get needed access; 2. ask for it; 3....; 4.profit
            public static void manageAccess(Map map, Country c, diplomatic_relations_manager diplomacy, dialog_box_manager dialog_box, camera_controller camera, diplomatic_actions_manager diplo_actions) {
                var impassable = Map.LandUtilites.getUnpassableProvinces(map, c);
                foreach(var e in Map.WarUtilities.getEnemyIds(map, c).Select(id => map.Countries[id]).ToList()) {
                    var needed = HexUtils.getBestPathProvinces(map, c, c.Capital, e.Capital);
                    if(needed != null) foreach(var p in needed) {
                        if(impassable.Contains(p) && p.OwnerId != 0 && p.IsLand) {
                            if (map.Countries[p.OwnerId].Opinions[c.Id] >= 0) {
                                    c.Actions.addAction(new TurnAction.MilAccessRequest(c, map.Countries[p.OwnerId], diplomacy, dialog_box, camera, diplo_actions));
                            }
                        }
                    }
                }
            }
            public static void leadingDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) {
                
                var vassalages = Map.PowerUtilites.getVassalRelations(map, c);
                foreach(var v in vassalages) {
                    var integration = new TurnAction.VassalIntegration(v, toolbox.Item1, toolbox.Item4);
                    if(c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.IntegrateVassal, v))) {
                        c.Actions.addAction(integration);
                    }
                }
                var vassals = Map.PowerUtilites.getVassals(map, c);
                foreach(var v in vassals) {
                    var improvement = new TurnAction.Praise(c, v, toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4);
                    if (c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.Praise))) {
                        c.Actions.addAction(improvement);
                    }
                }
                var weaklings = Map.PowerUtilites.getWeakCountries(map, c);
                foreach(var w in weaklings) {
                    var threat = new TurnAction.VassalizationDemand(c, w, toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4);
                    if (c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.VassalizationOffer))) {
                        c.Actions.addAction(threat);
                    }
                }
            }
            public static void defensiveDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) {
                var wars = Map.WarUtilities.getAllWars(map, c);
                var allies = Map.WarUtilities.getAllies(map, c);
                foreach(var war in wars) {
                    foreach(var ally in allies) {
                        var call = new TurnAction.CallToWar(c, ally, war, toolbox.Item2, toolbox.Item1, toolbox.Item3, toolbox.Item4);
                        if (c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.CallToWar))) {
                            c.Actions.addAction(call);
                        }
                    }
                }
            }
            public static void offensiveDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) {
                //cant think of anything so just call all allies like defensive
                defensiveDiplo(map, c, toolbox);
            }
            public static void subservientDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox) { 
                //cant think of anything
            }
            public static void rebelliousDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox, random_events_manager random) { 
                if(Map.PowerUtilites.howArmyStronger(map, c, Map.PowerUtilites.getSenior(map, c)) >= 0.3) {
                    if(random.chance <= 10) {
                        c.Actions.addAction(new TurnAction.VassalRebellion(Map.PowerUtilites.getVassalage(map, c), toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                    }
                }
            }
            public static void defaultDiplo(Map map, Country c, (diplomatic_relations_manager, dialog_box_manager, camera_controller, diplomatic_actions_manager) toolbox, random_events_manager random) { 
                for(int i = 1; i < map.Countries.Count; i++) {
                    if(i == c.Id) continue;
                    //just make them do something, even if it is very retarded and chaotic
                    if(c.Opinions[i] <= -100 && random.chance <= 25) {
                        c.Actions.addAction(new TurnAction.WarDeclaration(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                        continue;
                    }
                    if(c.Opinions[i] >= 100 && random.chance <= 25) {
                        c.Actions.addAction(new TurnAction.AllianceOffer(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                        continue;
                    }
                    var rnd = random.chance;
                    if(rnd <= 25) {
                        c.Actions.addAction(new TurnAction.Praise(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                    }
                    else if(rnd >= 75) {
                        c.Actions.addAction(new TurnAction.Insult(c, map.Countries[i], toolbox.Item1, toolbox.Item2, toolbox.Item3, toolbox.Item4));
                    }
                }
            }
        }
        private class internalAffairsManager {
            public static void handleUnhappy(Country c, Humor humor) {
                var unhappy = Map.LandUtilites.getUnhappyProvinces(c);
                var veryBad = unhappy.FindAll(p => p.Happiness <= 30).OrderBy(p=>p.Happiness).ToList();
                var handlable = unhappy.FindAll(p=>p.Happiness >30).OrderBy(p=>p.Happiness).ToList();
                //tax break on provinces with low chance of rebellion
                while (handlable.Count > 0) {
                    if (c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.TaxBreakIntroduction))) {
                        c.Actions.addAction(new TurnAction.TaxBreakIntroduction(handlable[0]));
                        handlable.RemoveAt(0);
                    }
                    else break;
                }
                //if can suppress, will for dire provinces
                if(c.techStats.canRebelSupp) while(veryBad.Count > 0) {
                    if (c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.RebelSuppresion))) {
                        c.Actions.addAction(new TurnAction.RebelSuppresion(veryBad[0]));
                        veryBad.RemoveAt(0);
                    }
                    else break;
                }
            }
            public static void handleGrowable(Country c, Humor humor) {
                var growable = Map.LandUtilites.getGrowable(c);
                int limit = humor == Humor.Leading ? c.Provinces.Count/10 : c.Provinces.Count/20;//10 and 5 % respecitvely
                foreach(var p in growable) {
                    if (c.isPayable(CostsCalculator.TurnActionFullCost(TurnAction.ActionType.FestivitiesOrganization))) {
                        c.Actions.addAction(new TurnAction.FestivitiesOrganization(p));
                    }
                    else break;
                }
            }
            public static void handleArmyRecruitment(Country c, Humor humor) {
                var toRecruit = new List<Province>();
                if(humor == Humor.Defensive) {
                    toRecruit = c.Provinces.ToList().OrderByDescending(p=>p.RecruitablePopulation).ToList();
                }
                else {
                    toRecruit= Map.LandUtilites.getOptimalRecruitmentProvinces(c);
                }
                bool exitF = false;
                foreach(var p in toRecruit) {
                    if (exitF || c.Resources[Resource.AP] < 1)
                        break;
                    exitF = !Map.LandUtilites.recruitAllAvailable(c, p);
                }
            }
            public static void handleTax(Map map, Country c, Humor humor) {
                if (humor == Humor.Defensive && humor == Humor.Offensive) {
                    if (c.techStats.lvlTax >= 1) {
                        if (!(c.Tax is WarTaxes))
                            c.Tax = new WarTaxes();
                        else
                            setProperTax(map, c);
                    }
                }
                else if(humor == Humor.Leading) {
                    if (c.techStats.lvlTax >= 2) {
                        c.Tax = new InvesmentTaxes();
                        float tax = Map.PowerUtilites.getTaxGain(c), upkeep = Map.PowerUtilites.getArmyUpkeep(map, c);
                        if(tax < upkeep) {
                            setProperTax(map, c);
                        }

                    }
                }
                else {
                    setProperTax(map, c);
                }
            }
            public static void setProperTax(Map map, Country c) {
                c.Tax = new LowTaxes();
                float tax = Map.PowerUtilites.getTaxGain(c), upkeep = Map.PowerUtilites.getArmyUpkeep(map, c);
                if(tax < upkeep) {
                    c.Tax = new MediumTaxes();
                    tax = Map.PowerUtilites.getTaxGain(c); upkeep = Map.PowerUtilites.getArmyUpkeep(map, c);
                    if(tax < upkeep) {
                        c.Tax = new HighTaxes();
                    }
                }
            }
            //AI should rush adm4(taxbreak) so it doesn't collapse immideately(rebel suppresion is too far in administratice tree so good luck AI you're gonna need it)
            public static void handleTechnology(Country c, Humor humor) {
                if (c.Technologies[Technology.Administrative] < 4) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Administrative);
                    if(c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else if(humor == Humor.Defensive || humor == Humor.Offensive || humor == Humor.Rebellious) {
                    techMEAPrio(c);
                }
                else if(humor == Humor.Leading || humor == Humor.Subservient) {
                    techAEMPrio(c);
                }
                else {
                    techMAEPrio(c);
                }
            }
            /// <summary>
            /// Priority Military -> Economic -> Administrative
            /// Defensive, Offensive, Rebellious
            /// </summary>
            /// <param name="c"></param>
            private static void techMEAPrio(Country c) {
                var tech = c.Technologies;
                if (tech[Technology.Military] - tech[Technology.Administrative] < 2 && tech[Technology.Military] - tech[Technology.Economic] < 2) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Military);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else if (tech[Technology.Economic] - tech[Technology.Administrative] > 1) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Administrative);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
            }
            /// <summary>
            /// Priority Administrative -> Economic -> Military
            /// Leading, Subservient
            /// </summary>
            /// <param name="c"></param>
            private static void techAEMPrio(Country c) {
                var tech = c.Technologies;
                if (tech[Technology.Administrative] - tech[Technology.Economic] < 2 && tech[Technology.Administrative] - tech[Technology.Military] < 2) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Administrative);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else if (tech[Technology.Economic] - tech[Technology.Military] > 1) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Military);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
            }
            /// <summary>
            /// Priority Military -> Administrative -> Economic
            /// _default
            /// </summary>
            /// <param name="c"></param>
            private static void techMAEPrio(Country c) {
                var tech = c.Technologies;
                if (tech[Technology.Military] - tech[Technology.Administrative] < 2 && tech[Technology.Military] - tech[Technology.Economic] < 2) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Military);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else if (tech[Technology.Administrative] - tech[Technology.Economic] > 1) {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
                else {
                    var action = new TurnAction.TechnologyUpgrade(c, Technology.Economic);
                    if (c.isPayable(Resource.AP, action.cost) && c.isPayable(action.altCosts)) {
                        c.Actions.addAction(action);
                    }
                }
            }
            //to simplify things
            //up to 2 infrastructures per turn
            //up to 1 anything else
            //if no school yet prioritize school over anything else
            //smartest thing is probably mine -> infrastructure -> school -> fort
            public static void handleBuildings(Country c, Humor humor) {
                TurnAction.BuildingUpgrade upgrade;
                if (c.getCapital().Buildings[BuildingType.Infrastructure] == 0) {
                    upgrade = new(c.getCapital(), BuildingType.Infrastructure);
                    if (c.isPayable(upgrade.altCosts))
                        c.Actions.addAction(upgrade);
                }
                //mine block
                List<Province> toImprove = getMineable(c);
                if(toImprove.Count>1){
                    upgrade = new(toImprove[0], BuildingType.Mine);
                    if(c.isPayable(upgrade.altCosts))
                        c.Actions.addAction(upgrade);
                }
                //infr block
                toImprove = getInfrastructurable(c);
                if(toImprove.Count>2){
                upgrade = new(toImprove[0], BuildingType.Infrastructure);
                if (c.isPayable(upgrade.altCosts))
                    c.Actions.addAction(upgrade);
                upgrade = new(toImprove[1], BuildingType.Infrastructure);
                    if(c.isPayable(upgrade.altCosts))
                        c.Actions.addAction(upgrade);
                }
                //school block
                toImprove = getSchoolable(c);
                if(toImprove.Count>1){
                upgrade = new(toImprove[0], BuildingType.School);
                    if(c.isPayable(upgrade.altCosts))
                        c.Actions.addAction(upgrade);
                }
                //fort block
                toImprove = getFortable(c);
                if(toImprove.Count>1){
                    upgrade = new(toImprove[0], BuildingType.Fort);
                    if(c.isPayable(upgrade.altCosts))
                        c.Actions.addAction(upgrade);
                }
            }
            //infr limits for ai adm<2-1; adm<5-2; adm<7-3
            //mainly to make them build other stuff as well
            //getting lists cause I might increase limits(who knows)
            private static List<Province> getInfrastructurable(Country c) {
                int mode;
                var adm = c.Technologies[Technology.Administrative];
                if (adm < 2) mode = 1;
                else if (adm < 5) mode = 2;
                else mode = 3;
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.Infrastructure, out int level) && level < mode).OrderByDescending(p=>p.Population).ToList();
            }
            private static List<Province> getFortable(Country c) {
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.Fort, out int level) && level < ( c.techStats.lvlFort+1)).OrderByDescending(p=>p.Population).ToList();
            }
            private static List<Province> getMineable(Country c) {
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.Mine, out int level) && level < (c.techStats.lvlMine + 1)).OrderByDescending(p => p.Population).ToList();
            }
            //ograniczenie do 2 z lenistwa xd
            private static List<Province> getSchoolable(Country c) {
                return c.Provinces.Where(p => p.Buildings.TryGetValue(BuildingType.School, out int level) && level < (c.techStats.moreSchool ? 2 : 1)).OrderByDescending(p=> p.Population).ToList();
            }
        }
        private class diploEventResponder {
            public static void Respond(Event_ e, Map map, Humor humor) {
                Type t = e.GetType();
                var method = typeof(diploEventResponder).GetMethod("respond", new[] { t, typeof(Map), typeof(Humor) });
                if (method != null) {
                    method.Invoke(null, new object[] { e, map, humor });
                } else {
                    Debug.LogError("Diplo events cause problems once again. This time ai can't respond to one.");
                }
            }

            public static void respond(DiploEvent.WarDeclared e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.PeaceOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Defensive:
                        e.accept(); break;
                    case Humor.Offensive: 
                        e.reject(); break;
                    case Humor.Leading:
                        //check if armies of the main opponent are overwhelmingly big
                        if (map.getCountryArmies(e.From).Count >= 1.2 * (double)map.getCountryArmies(e.To).Count)
                            e.accept();
                        else 
                            e.reject();
                        break;
                    default:
                        if (map.getCountryArmies(e.From).Count >= 0.8 * (double)map.getCountryArmies(e.To).Count)
                            e.accept();
                        else
                            e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.CallToWar e, Map map, Humor humor) {
                var war = e.War;
                switch (humor) {
                    case Humor.Leading:
                        e.accept(); break;
                    case Humor.Defensive:
                        e.reject(); break;
                    case Humor.Offensive:
                        if (Map.WarUtilities.isAttacker(map, e.From, war)) {
                            if (Map.WarUtilities.isAttackersStronger(map, war))
                                e.accept();
                            else e.reject();
                        }
                        else {
                            if (Map.WarUtilities.isAttackersStronger(map, war))
                                e.reject();
                            else e.accept();
                        }
                        break;
                    case Humor.Subservient: e.accept(); break;
                    case Humor.Rebellious: e.accept(); break;
                    default:
                        var powers = Map.WarUtilities.getSidePowers(map, war);
                        if ((Map.WarUtilities.isAttacker(map, e.From, war) && powers.Item1 > 0.6 * powers.Item2) || !Map.WarUtilities.isAttacker(map, e.From, war))
                            e.accept();
                        else e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.TruceEnd e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.AllianceOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        e.reject();
                        break;
                    case Humor.Defensive:
                        e.accept();
                        break;
                    case Humor.Subservient:
                        e.reject();
                        break;
                    case Humor.Rebellious:
                        e.reject(); break;
                    case Humor.Offensive:
                        
                    default:
                        if (e.To.Opinions[e.From.Id] > 150 || (e.To.Opinions[e.From.Id] > 75 && map.getCountryArmies(e.From).Count > map.getCountryArmies(e.To).Count)) {
                            e.accept();
                        }
                        else e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.AllianceAccepted e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.AllianceDenied e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.AllianceBroken e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.SubsOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        e.reject(); break;
                    case Humor.Subservient:
                        e.reject(); break;
                    case Humor.Rebellious:
                        e.accept(); break;
                    case Humor.Defensive:
                        e.accept(); break;
                    case Humor.Offensive:
                        if (map.Countries[e.To.Id].Opinions[e.From.Id] > 100) {
                            e.accept();
                        }
                        else e.reject();
                        break;
                    default:
                        if (Map.PowerUtilites.getOpinion(e.From, e.To) > 0)
                            e.accept();
                        else e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.SubsRequest e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        if(Map.PowerUtilites.getOpinion(e.From, e.To) > 100 && e.Amount > 0.05f*Map.PowerUtilites.getGoldGain(map, e.To)) {
                            e.accept();
                        }
                        e.reject();
                        break;
                    case Humor.Offensive:
                        e.reject();
                        break;
                    case Humor.Defensive:
                        e.reject();
                        break;
                    default:
                        if (Map.PowerUtilites.getGoldGain(map, e.To) > 0.2f * e.Amount && Map.PowerUtilites.getOpinion(e.From, e.To) > 150) {
                            e.accept();
                        }
                        else e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.SubsEndMaster e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.SubsEndSlave e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.AccessOffer e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.AccessRequest e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        if (Map.PowerUtilites.howArmyStronger(map, e.To, e.From) <= 1.1f) {
                            e.reject();
                        }
                        else e.accept();
                        break;
                    case Humor.Offensive:
                        e.reject();
                        break;
                    case Humor.Subservient:
                        if(map.hasRelationOfType(map.getSeniorIfExists(e.To), e.From, Relation.RelationType.MilitaryAccess)) {
                            e.accept();
                        } else e.reject();
                        break;
                    case Humor.Rebellious:
                        if (map.hasRelationOfType(map.getSeniorIfExists(e.To), e.From, Relation.RelationType.MilitaryAccess)) {
                            e.accept();
                        }
                        else e.reject();
                        break;
                    case Humor.Defensive:
                        e.accept();
                        break;
                    default:
                        e.accept(); break;
                }
            }

            public static void respond(DiploEvent.AccessEndMaster e, Map map, Humor humor) {
                e.accept();
            }

            public static void respond(DiploEvent.AccessEndSlave e, Map map, Humor humor) {
                e.accept(); 
            }

            public static void respond(DiploEvent.VassalOffer e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        e.reject();
                        break;
                    case Humor.Offensive:
                        e.reject(); break;
                    case Humor.Defensive:
                        e.accept(); break;
                    default:
                        if(Map.PowerUtilites.getOpinion(e.From, e.To)> 175) {
                            e.accept();
                        }
                        else if(Map.PowerUtilites.howArmyStronger(map, e.From, e.To) > 2) {
                            e.accept();
                        }
                        else {
                            e.reject();
                        }
                        break;
                }
            }

            public static void respond(DiploEvent.VassalRebel e, Map map, Humor humor) {
                e.accept();
            }

        }
    }
}
