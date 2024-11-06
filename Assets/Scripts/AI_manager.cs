﻿using Assets.classes;
using Assets.classes.subclasses;
using Assets.classes.Tax;
using Assets.map.scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using UnityEditor.PackageManager.Requests;
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
        /// <summary>
        /// Every turn AI lead country will asess its situation in the world and change its behavior to suit it the best.
        /// </summary>
        private Humor humor;

        /// <summary>
        /// main method responsible for coordinating behavior of AI country's turn
        /// </summary>
        public void behave() {
            this.humor = asess();
            //events block
            while (map.CurrentPlayer.Events.Count > 0) { 
                var e = map.CurrentPlayer.Events.Last();
                respondToEvent(e);
                map.CurrentPlayer.Events.Remove(e);
            }
            //internal block
            manageInternal();
            this.humor = Humor.Null;
        }

        /// <summary>
        /// decides on value of humor of AI this turn
        /// </summary>
        private Humor asess() {
            //first block; vassal comparison with senior
            var vassalage = map.getRelationsOfType(map.CurrentPlayer, Relation.RelationType.Vassalage).First(rel => rel.Sides[1] == map.CurrentPlayer);
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
            var topproductions = map.Countries.Select(c => map.getResourceGain(c)).SelectMany(d => d)
                .Where(k => k.Key ==Resource.Gold)
                .OrderByDescending(k => k.Value)
                .Take(2)//can change to whatever just don't go overboard
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

        private void moveArmies() {

        }
        /// <summary>
        /// the way AI acts diplomaticly this turn
        /// </summary>
        private void diplomacy() {

        }
        private void manageInternal() {
            internalAffairsManager.setProperTax(map, map.CurrentPlayer);
            if (humor == Humor.Defensive || humor == Humor.Offensive) internalAffairsManager.handleArmyRecruitment(map.CurrentPlayer, humor);
            internalAffairsManager.handleUnhappy(map.CurrentPlayer, humor);
            internalAffairsManager.handleTechnology(map.CurrentPlayer, humor);
            internalAffairsManager.handleBuildings(map.CurrentPlayer, humor);
            if (humor != Humor.Defensive || humor == Humor.Offensive) internalAffairsManager.handleArmyRecruitment(map.CurrentPlayer, humor);
        }



        private int getArmySum(int id) {
            return map.Armies.FindAll(a=>a.OwnerId == id).Sum(a=> a.Count);
        }
        private class internalAffairsManager {
            public static void handleUnhappy(Country c, Humor humor) {
                var unhappy = Map.LandUtilites.getUnhappyProvinces(c);
                var unTaxBreakable = unhappy.FindAll(p => p.Happiness < 30);
                var handlable = unhappy.FindAll(p=>p.Happiness >30);
                //while (handlable.Count > 0) {
                //    handlable[0]
                //} czekamy :^))))))))
            }
            public static void handleGrowable(Country c, Humor humor) {
                var growable = Map.LandUtilites.getGrowable(c);
                //still need that actions :))))))))))))000
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
            //AI should rush eco2(boats) into adm4(taxbreak) so it doesn't collapse immideately
            //TODO xd
            public static void handleTechnology(Country c, Humor humor) {
                if (c.Resources[Resource.AP] >= 1) if (c.Technology_[Technology.Economic] < 2) {
                    var upgrade = CostsCalculator.TechCost(c.Technology_, Technology.Economic);
                    if (c.Resources[Resource.SciencePoint] >= upgrade[Resource.SciencePoint]) {
                        //bym chetnie cos zrobil ale nie moge
                    }
                }
            }
            public static void handleBuildings(Country c, Humor humor) {
                if(c.Provinces.First(p=>p.coordinates == c.Capital).Buildings[(int)BuildingType.Infrastructure].BuildingLevel == 0) {
                    //if(c.isPayable(CostsCalculator.TurnActionFullCost(actionContainer.TurnAction.ActionType.BuildingUpgrade, bType:BuildingType.Infrastructure, )))
                }
                else {

                }
            }
            
        }
        private class diploEventResponder {
            public static void Respond(Event_ e, Map map, Humor humor) {
                //tu sie dzieje magia
                Type t = e.GetType();
                //tu tez
                var method = typeof(diploEventResponder).GetMethod("respond", new[] { t, typeof(Map), typeof(Humor) });
                if (method != null) {
                    //ale w sumie to konkretnie tu
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
                        if (map.getCountryArmies(e.from).Count >= 1.2 * (double)map.getCountryArmies(e.to).Count)
                            e.accept();
                        else 
                            e.reject();
                        break;
                    default:
                        if (map.getCountryArmies(e.from).Count >= 0.8 * (double)map.getCountryArmies(e.to).Count)
                            e.accept();
                        else
                            e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.CallToWar e, Map map, Humor humor) {
                var war = e.war;
                switch (humor) {
                    case Humor.Leading:
                        e.accept(); break;
                    case Humor.Defensive:
                        e.reject(); break;
                    case Humor.Offensive:
                        if (Map.WarUtilities.isAttacker(map, e.from, war)) {
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
                        if ((Map.WarUtilities.isAttacker(map, e.from, war) && powers.Item1 > 0.6 * powers.Item2) || !Map.WarUtilities.isAttacker(map, e.from, war))
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
                        if (e.to.Opinions[e.from.Id] > 150 || (e.to.Opinions[e.from.Id] > 75 && map.getCountryArmies(e.from).Count > map.getCountryArmies(e.to).Count)) {
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
                        if (map.Countries[e.to.Id].Opinions[e.from.Id] > 100) {
                            e.accept();
                        }
                        else e.reject();
                        break;
                    default:
                        if (Map.PowerUtilites.getOpinion(e.from, e.to) > 0)
                            e.accept();
                        else e.reject();
                        break;
                }
            }

            public static void respond(DiploEvent.SubsRequest e, Map map, Humor humor) {
                switch (humor) {
                    case Humor.Leading:
                        if(Map.PowerUtilites.getOpinion(e.from, e.to) > 100 && e.amount > 0.05f*Map.PowerUtilites.getGoldGain(map, e.to)) {
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
                        if (Map.PowerUtilites.getGoldGain(map, e.to) > 0.2f * e.amount && Map.PowerUtilites.getOpinion(e.from, e.to) > 150) {
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
                        if (Map.PowerUtilites.howArmyStronger(map, e.to, e.from) <= 1.1f) {
                            e.reject();
                        }
                        else e.accept();
                        break;
                    case Humor.Offensive:
                        e.reject();
                        break;
                    case Humor.Subservient:
                        if(map.hasRelationOfType(map.getSeniorIfExists(e.to), e.from, Relation.RelationType.MilitaryAccess)) {
                            e.accept();
                        } else e.reject();
                        break;
                    case Humor.Rebellious:
                        if (map.hasRelationOfType(map.getSeniorIfExists(e.to), e.from, Relation.RelationType.MilitaryAccess)) {
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
                        if(Map.PowerUtilites.getOpinion(e.from, e.to)> 175) {
                            e.accept();
                        }
                        else if(Map.PowerUtilites.howArmyStronger(map, e.from, e.to) > 2) {
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
