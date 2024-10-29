using Assets.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

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
        /// <summary>
        /// Every turn AI lead country will asess its situation in the world and change its behavior to suit it the best.
        /// </summary>
        private Humor humor;

        /// <summary>
        /// main method responsible for coordinating behavior of AI country's turn
        /// </summary>
        public void behave() {
            this.humor = asess();
            //body
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

        }

        private void moveArmies() {

        }
        /// <summary>
        /// the way AI acts diplomaticly this turn
        /// </summary>
        private void diplomacy() {

        }
        private void manageInternal() {

        }



        private int getArmySum(int id) {
            return map.Armies.FindAll(a=>a.OwnerId == id).Sum(a=> a.Count);
        }

    }
}
