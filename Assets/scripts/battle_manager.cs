using Assets.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts {
    internal class battle_manager : MonoBehaviour{
        [SerializeField] private Map map;

        public void checkBattle(Army army) {
            Province province = map.getProvince(army.Position);
            List<Army> other = map.Armies.FindAll(a => a.Position == army.Position && a != army);
            if(other != null) {
                List<Army> enemyarmies = enemyArmiesInProvince(army);
                if (enemyarmies != null) {
                    var it = 0;
                    while (army.Count > 0 && enemyarmies.Sum(a=>a.Count) > 0) { 
                        if(battle(army, enemyarmies[it]))
                            enemyarmies[it++].Count=0;
                    }
                    enemyarmies.RemoveAll(a => a.Count == 0);
                    if (army.Count == 0) map.removeArmy(army);
                }
            }
        }

        private List<Army> enemyArmiesInProvince(Army army) {
            List<int> enemyCountries = new List<int>();
            foreach(var relation in map.Relations) {
                if(relation.type == classes.Relation.RelationType.War) {
                    Relation.War war = relation as Relation.War;
                    if(war.participants1.Any(c=>c.Id == army.OwnerId)) {
                        enemyCountries.AddRange(war.participants2.Select(c => c.Id));
                    }
                    else if (war.participants2.Any(c=>c.Id == army.OwnerId)) {
                        enemyCountries.AddRange(war.participants1.Select(c => c.Id));
                    }
                }
            }
            var armies = map.Armies.FindAll(a => a.Position == army.Position && a != army);
            List<Army> enemyarmies = armies.FindAll(a => enemyCountries.Contains(a.OwnerId));
            enemyarmies.AddRange(armies.FindAll(a => a.OwnerId == 0));
            return enemyarmies;
        }

        private bool battle(Army attacker, Army defender) {
            Country attCountry = map.Countries[attacker.OwnerId], defCountry = map.Countries[defender.OwnerId];
            float attPower = attacker.Count * attCountry.techStats.armyPower;//attacker power
            float defPower = defender.Count * defCountry.techStats.armyPower + 1;//defender power
            float fortModifier = 1;
            if (map.getProvince(defender.Position).Owner_id == defender.OwnerId)//fort defense bonus
                fortModifier += 0.1f * map.getProvince(defender.Position).Buildings[BuildingType.Fort];
            defPower *= fortModifier;
            float result = attPower - defPower;
            Debug.Log(map.Countries[attacker.OwnerId].Name + " has attacked " + map.Countries[defender.OwnerId].Name +
                " with an army of " + attacker.Count + " vs " + defender.Count + " and power modifier of " + attCountry.techStats.armyPower + " vs " + defCountry.techStats.armyPower
                + "\nprojected result is: " + result);
            if (result > 0) {
                attacker.Count = (int)(result / attCountry.techStats.armyPower);
                map.removeArmy(defender);
                return true;
            }
            else {
                map.removeArmy(attacker);
                defender.Count = (int)(-result/ fortModifier / defCountry.techStats.armyPower);
                return false;
            }
        }
    }
}
