using Assets.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.map.scripts {
    internal class diplomatic_relations_manager : MonoBehaviour{
        [SerializeField] private Map map;

        public void turnCalc() {
            HashSet<Relation> toRemove = new HashSet<Relation>();
            foreach(Relation r in map.Relations) {
                r.turnEffect();
                if(r.type == Relation.RelationType.Truce) {
                    var R = r as Relation.Truce;
                    if(R.Duration == 0) {
                        toRemove.Add(r);
                    }
                }
                else if(r.type == Relation.RelationType.Subsidies) {
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

        public void endRelation(Relation relation) {
            switch(relation.type) {
                case Relation.RelationType.War:
                    endWar((Relation.War)relation);
                    break;
                case Relation.RelationType.Alliance:
                    endAlliace((Relation.Alliance)relation);
                    break;
                case Relation.RelationType.Truce:
                    endTruce((Relation.Truce)relation);
                    break;
                case Relation.RelationType.Vassalage:
                    
                    break;
                case Relation.RelationType.Subsidies:
                    endSub((Relation.Subsidies)relation);
                    break;
                case Relation.RelationType.MilitaryAccess:
                    endAccess((Relation.MilitaryAccess)relation);
                    break;
                default:
                    break;
            }
        }

        public void startWar(Country c1, Country c2) {
            //pos relations go away
            foreach(var r in map.Relations.Where(r => r.type >= 0).ToList()) { 
                map.Relations.Remove(r);
            }
            map.Relations.Add(new Relation.War(c1, c2));
        }
        private void endWar(Relation.War relation) { 
            map.Relations.Remove(relation);
            //dodaj zeby okupowane ale jeszcze nie przejete zostaly przejete
            map.Relations.Add(new Relation.Truce(relation.Sides[0], relation.Sides[1], 5));
        }
        private void endTruce(Relation.Truce relation) {
            map.Relations.Remove(relation);
        }
        public void startAlliance(Country c1, Country c2) {
            map.Relations.Add(new Relation.Alliance(c1, c2));
            //dodaj wszyskie kratki sojusznika byly odkryte po zawarciu sojuszu, chyba to:
            foreach(var tile in c1.Provinces) {
                c2.SeenTiles.Add(tile.coordinates);
                //c2.RevealedTiles.Add(tile.coordinates);
            }
            foreach(var tile in c2.Provinces) {
                c1.SeenTiles.Add(tile.coordinates);
                //c1.RevealedTiles.Add(tile.coordinates);
            }
        }
        private void endAlliace(Relation.Alliance relation) {
            map.Relations.Remove(relation);
            map.Relations.Add(new Relation.Truce(relation.Sides[0], relation.Sides[1], 3));
        }
        public void startAccess(Country c1, Country c2) {
            map.Relations.Add(new Relation.MilitaryAccess(c1, c2));
        }
        private void endAccess(Relation.MilitaryAccess relation) {
            map.Relations.Remove(relation);
        }
        public void startSub(Country c1, Country c2, int amount, bool indf = true, int duration = 0) {
            if(!indf) {
                map.Relations.Add(new Relation.Subsidies(c1, c2, amount, duration));
            }
            map.Relations.Add(new Relation.Subsidies(c1, c2, amount));
        }
        private void endSub(Relation.Subsidies relation) {
            map.Relations.Remove(relation);
        }
        public void startVassalage(Country c1, Country c2) {
            map.Relations.Add(new Relation.Vassalage(c1, c2));

            //odkrycie kafelkow itp
            foreach(var tile in c1.Provinces) {
                c2.SeenTiles.Add(tile.coordinates);
                //c2.RevealedTiles.Add(tile.coordinates);
            }
            foreach(var tile in c2.Provinces) {
                c1.SeenTiles.Add(tile.coordinates);
                //c1.RevealedTiles.Add(tile.coordinates);
            }
        }
        public bool joinWar(Relation.War war, Country join, Country ally) {
            HashSet<Country> side;
            if(ally == war.Sides[0]) {
                war.participants1.Add(join);
                side = war.participants1;
            }
            else if(ally == war.Sides[1]) {
                war.participants2.Add(join);
                side = war.participants2;
            }
            else { 
                return false;
            }
            foreach(Country c in side) {
                var alliance = map.Relations.FirstOrDefault(r => r.type == Relation.RelationType.Alliance && r.Sides.Contains(join) && r.Sides.Contains(c));
                if(alliance != null) {
                    endAlliace((Relation.Alliance)alliance);
                }
                var truce = map.Relations.FirstOrDefault(r => r.type == Relation.RelationType.Truce && r.Sides.Contains(join) && r.Sides.Contains(c));
                if(truce != null) { 
                    endTruce((Relation.Truce)truce);
                }
            }
            return true;
        }
        public void declineWar(Country join, Country ally) {
            ally.Opinions[join.Id] -= 100;
            endAlliace((Relation.Alliance)map.Relations.FirstOrDefault(r => r.type == Relation.RelationType.Alliance && r.Sides.Contains(join) && r.Sides.Contains(ally)));
        }
        public void integrateVassal(Relation.Vassalage relation) {
            Country vassal = relation.Sides[1], master = relation.Sides[0];
            map.Relations.Remove(relation);
            var toRemove = new HashSet<Province>();
            foreach(Province p in vassal.Provinces) {
                p.Owner_id = master.Id;
                toRemove.Add(p);
            }
            foreach(var p in toRemove) { 
                vassal.unassignProvince(p);
            }
        }
    }
}
