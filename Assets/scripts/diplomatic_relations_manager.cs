using Assets.classes;
using Assets.classes.subclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.map.scripts {
    internal class diplomatic_relations_manager : MonoBehaviour {
        [SerializeField] private Map map;

        public const int WarHappinessPenaltyInitC1 = 15;
        public const int WarHappinessPenaltyInitC2 = 5;
        public const int AllianceHappinessBonusInit = 5;
        public const int VassalageHappinessPenaltyInitC2 = 8;
        public const int VassalageHappinessBonusInitC1 = 5;
        public const int DeclineWarOpinionPenaltyInit = 100;

        public Map Map { get => map; set => map = value; }

        public void TurnCalc() {
            HashSet<Relation> toRemove = new HashSet<Relation>();
            foreach(Relation r in map.Relations) {
                r.TurnEffect();
                if(r.Type == Relation.RelationType.Truce) {
                    var R = r as Relation.Truce;
                    if(R.Duration == 0) {
                        toRemove.Add(r);
                    }
                }
                else if(r.Type == Relation.RelationType.Subsidies) {
                    var R = r as Relation.Subsidies;
                    if(R.Duration == 0) {
                        toRemove.Add(r);
                    }
                }
            }
            foreach(var r in toRemove) { 
                map.Relations.Remove(r);
            }
        }

        public void EndRelation(Relation relation) {
            switch(relation.Type) {
                case Relation.RelationType.War:
                    EndWar((Relation.War)relation);
                    break;
                case Relation.RelationType.Alliance:
                    EndAlliance((Relation.Alliance)relation);
                    break;
                case Relation.RelationType.Truce:
                    EndTruce((Relation.Truce)relation);
                    break;
                case Relation.RelationType.Vassalage:
                    
                    break;
                case Relation.RelationType.Subsidies:
                    EndSub((Relation.Subsidies)relation);
                    break;
                case Relation.RelationType.MilitaryAccess:
                    EndAccess((Relation.MilitaryAccess)relation);
                    break;
                default:
                    break;
            }
        }

        public void StartWar(Country c1, Country c2) {
            //pos relations between c1 and c2 go away
            foreach(var r in map.Relations.Where(r => r.Type >= 0 
                && r.Sides.Contains(c1) && r.Sides.Contains(c2)).ToList()) { 
                map.Relations.Remove(r);
            }

            Relation.War warRelation = new(c1, c2);
            map.Relations.Add(warRelation);

            foreach(var p in c1.Provinces) p.Happiness -= WarHappinessPenaltyInitC1;
            foreach(var p in c2.Provinces) p.Happiness -= WarHappinessPenaltyInitC2;

            // Vassals of both sides join the war
            foreach (var r in map.Relations.Where(rel => rel.Type == Relation.RelationType.Vassalage))
            {
                if (r.Sides[0] == c1) JoinWar(warRelation, r.Sides[1], c1);
                if (r.Sides[0] == c2) JoinWar(warRelation, r.Sides[1], c2);
            }
        }

        private void EndWar(Relation.War relation)
        {
            void EndOccupationInNextTurn(Province province)
            {
                if (province.Statuses.Find(s => s is Occupation) is Occupation status)
                {
                    status.Duration = 1;
                }
            }

            var participants1 = new HashSet<int>(relation.Participants1.Select(c => c.Id));
            var participants2 = new HashSet<int>(relation.Participants2.Select(c => c.Id));

            map.Relations.Remove(relation);

            foreach (var province in map.Provinces.Where(p => p.IsLand))
            {
                bool isOwnerFromParticipants1 = participants1.Contains(province.OwnerId);
                bool isOccupyingFromParticipants2 = participants2.Contains(province.OccupationInfo.OccupyingCountryId);
                bool isOwnerFromParticipants2 = participants2.Contains(province.OwnerId);
                bool isOccupyingFromParticipants1 = participants1.Contains(province.OccupationInfo.OccupyingCountryId);

                if ((isOwnerFromParticipants1 && isOccupyingFromParticipants2) || 
                    (isOwnerFromParticipants2 && isOccupyingFromParticipants1)) {
                    EndOccupationInNextTurn(province);
                }
            }

            foreach (var country1 in participants1)
            {
                foreach (var country2 in participants2)
                {
                    Country c1 = map.Countries[country1];
                    Country c2 = map.Countries[country2];
                    map.Relations.Add(new Relation.Truce(c1, c2, 5));
                }
            }
        }

        private void EndTruce(Relation.Truce relation) {
            map.Relations.Remove(relation);
        }
        public void StartAlliance(Country c1, Country c2) {
            map.Relations.Add(new Relation.Alliance(c1, c2));
            //all ally tiles are revealed
            foreach(var tile in c1.Provinces) {
                c2.SeenTiles.Add(tile.coordinates);
                tile.Happiness += AllianceHappinessBonusInit;
            }
            foreach(var tile in c2.Provinces) {
                c1.SeenTiles.Add(tile.coordinates);
                tile.Happiness += AllianceHappinessBonusInit;
            }
        }
        private void EndAlliance(Relation.Alliance relation) {
            map.Relations.Remove(relation);
            map.Relations.Add(new Relation.Truce(relation.Sides[0], relation.Sides[1], 3));
        }
        public void StartAccess(Country c1, Country c2) {
            map.Relations.Add(new Relation.MilitaryAccess(c1, c2));
        }
        private void EndAccess(Relation.MilitaryAccess relation) {
            map.Relations.Remove(relation);
        }
        public void StartSub(Country c1, Country c2, int amount, bool indf = true, int duration = 0) {
            if(!indf) {
                map.Relations.Add(new Relation.Subsidies(c1, c2, amount, duration));
            }
            else {
                map.Relations.Add(new Relation.Subsidies(c1, c2, amount));
            }
        }
        private void EndSub(Relation.Subsidies relation) {
            map.Relations.Remove(relation);
        }
        public void StartVassalage(Country c1, Country c2) {
            map.Relations.Add(new Relation.Vassalage(c1, c2));

            //seen tiles
            foreach(var tile in c1.Provinces) {
                c2.SeenTiles.Add(tile.coordinates);
                tile.Happiness -= VassalageHappinessPenaltyInitC2;
            }
            foreach(var tile in c2.Provinces) {
                c1.SeenTiles.Add(tile.coordinates);
                tile.Happiness += VassalageHappinessBonusInitC1;
            }
        }
        public bool JoinWar(Relation.War war, Country join, Country ally) {
            HashSet<Country> enemyParticipants;

            if (ally == war.Sides[0]) {
                war.Participants1.Add(join);
                enemyParticipants = war.Participants2;
            }
            else if (ally == war.Sides[1]) {
                war.Participants2.Add(join);
                enemyParticipants = war.Participants1;
            }
            else { 
                return false;
            }

            foreach(Country enemy in enemyParticipants) {
                //pos relations between the enemy and join go away
                foreach (var r in map.Relations.Where(r => r.Type >= 0 && r.Sides.Contains(join) && r.Sides.Contains(enemy)).ToList())
                {
                    map.Relations.Remove(r);
                }
            }

            foreach (var p in join.Provinces) p.Happiness -= WarHappinessPenaltyInitC2;

            return true;
        }
        public void DeclineWar(Country join, Country ally) {
            ally.SetOpinion(join.Id, ally.Opinions[join.Id] - DeclineWarOpinionPenaltyInit);
            EndAlliance((Relation.Alliance)map.Relations.FirstOrDefault(r => r.Type == Relation.RelationType.Alliance && r.Sides.Contains(join) && r.Sides.Contains(ally)));
        }
        public void IntegrateVassal(Relation.Vassalage relation) {
            Country vassal = relation.Sides[1], master = relation.Sides[0];
            map.Relations.Remove(relation);
            var toRemove = new HashSet<Province>();
            foreach(Province p in vassal.Provinces) {
                p.OwnerId = master.Id;
                toRemove.Add(p);
            }
            foreach(var p in toRemove) { 
                vassal.UnassignProvince(p);
            }
        }
    }
}
