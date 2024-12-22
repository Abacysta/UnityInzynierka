using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts {
    internal class battle_manager : MonoBehaviour {
        [SerializeField] private Map map;

        public void CheckBattle(Army attackerArmy) {
            Province province = map.GetProvince(attackerArmy.Position);
            List<Army> otherArmies = map.Armies.FindAll(a => 
                a.Position == attackerArmy.Position && a != attackerArmy);

            if (otherArmies != null) {
                List<Army> enemyArmies = GetEnemyArmiesInProvince(attackerArmy);

                if (enemyArmies != null) {
                    var it = 0;

                    while (enemyArmies.Sum(a => a.Count) > 0) {
                        if (IsAttackerVictorious(attackerArmy, enemyArmies[it])) {
                            enemyArmies[it++].Count = 0;
                        }
                        else {
                            break;
                        }
                    }

                    enemyArmies.RemoveAll(a => a.Count == 0);
                    if (attackerArmy.Count == 0) map.RemoveArmy(attackerArmy);
                }
            }

            map.ManageOccupation();
        }

        private List<Army> GetEnemyArmiesInProvince(Army army) {
            var enemyArmies = Map.WarUtilities.GetEnemyArmies(map, map.Countries[army.OwnerId])
                .Where(a => a.Position == army.Position).ToList();
            enemyArmies.AddRange(map.Armies.Where(a => a.OwnerId == 0 
                && a.Position == army.Position).ToList());

            return enemyArmies;
        }

        private bool IsAttackerVictorious(Army attacker, Army defender) {
            Country attCountry = map.Countries[attacker.OwnerId];
            Country defCountry = map.Countries[defender.OwnerId];

            float attPower = attacker.Count * attCountry.TechStats.ArmyPower;
            float defPower = defender.Count * defCountry.TechStats.ArmyPower + 1;
            float fortModifier = 1;

            if (map.GetProvince(defender.Position).OwnerId == defender.OwnerId) { //fort defense bonus
                fortModifier += 0.1f * map.GetProvince(defender.Position).Buildings[BuildingType.Fort];
            }

            defPower *= fortModifier;
            float result = attPower - defPower;

            Debug.Log(map.Countries[attacker.OwnerId].Name + " has attacked " 
                + map.Countries[defender.OwnerId].Name +
                " with an army of " + attacker.Count + " vs " + defender.Count 
                + " and power modifier of " + attCountry.TechStats.ArmyPower + " vs " 
                + defCountry.TechStats.ArmyPower
                + "\nprojected result is: " + result);

            if (result > 0) {
                attacker.Count = (int)(result / attCountry.TechStats.ArmyPower);
                map.RemoveArmy(defender);
                return true;
            }
            else {
                map.RemoveArmy(attacker);
                defender.Count = (int)(-result/ fortModifier / defCountry.TechStats.ArmyPower);
                return false;
            }
        }
    }
}
