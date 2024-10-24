using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.classes.subclasses {
    public static class CostsCalculator { 
        public static Dictionary<Resource, float> bCost(BuildingType type, int lvl) {
            switch(type) {
                case BuildingType.Infrastructure:
                    switch(lvl) {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 10 }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 15 },
                                {Resource.Wood, 25 }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 25 },
                                {Resource.Wood, 10 },
                                {Resource.Iron, 25 },
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                case BuildingType.Fort:
                    switch(lvl) {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                {Resource.Wood, 20}
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10},
                                {Resource.Wood, 10},
                                {Resource.Iron, 25}
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 30 },
                                {Resource.Wood, 10},
                                {Resource.Iron, 40}
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                case BuildingType.Mine:
                    switch(lvl) {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 10 }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 10 },
                                {Resource.Wood, 10}
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 10 },
                                {Resource.Wood, 30},
                                {Resource.Iron, 5 }
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                case BuildingType.School:
                    switch(lvl) {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                {Resource.Gold, 100 },
                                {Resource.Wood, 50},
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 150 },
                                { Resource.Wood, 300 },
                                { Resource.Iron, 50 }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 250 },
                                { Resource.Wood, 100 },
                                { Resource.Iron, 300 },
                                { Resource.SciencePoint, 10 }
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                default:
                    return new Dictionary<Resource, float>();
            }
        }
        public static Dictionary<Resource, float> TechCost(Dictionary<Technology, int> tech, Technology type)
        {
            int techLvl = tech[type];
            int allTechLvl = tech.Values.Sum();

            return new Dictionary<Resource, float> {
                { Resource.AP, 0.1f },
                { Resource.SciencePoint, 10 + (10 * techLvl) + (5 * allTechLvl) }
            };
        }
    }
}

