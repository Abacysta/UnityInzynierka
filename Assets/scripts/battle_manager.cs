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
            var ea = Map.WarUtilities.getEnemyArmies(map, map.Countries[army.OwnerId]).Where(a=>a.Position == army.Position).ToList();
            ea.AddRange(map.Armies.Where(a => a.OwnerId == 0 && a.Position == army.Position).ToList());
            return ea;
        }

        private bool battle(Army attacker, Army defender) {
            Country attCountry = map.Countries[attacker.OwnerId], defCountry = map.Countries[defender.OwnerId];
            float attPower = attacker.Count * attCountry.techStats.armyPower;
            float defPower = defender.Count * defCountry.techStats.armyPower + 1;
            float fortModifier = 1;
            if (map.getProvince(defender.Position).OwnerId == defender.OwnerId)//fort defense bonus
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
