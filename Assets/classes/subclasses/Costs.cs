using System.Collections.Generic;
using System.Linq;
using static Assets.classes.TurnAction;

namespace Assets.classes.subclasses
{
    public static class CostsCalculator
    {
        private static Dictionary<Resource, float> BuildingCost(BuildingType buildingType, int upgradeLevel)
        {
            switch (buildingType)
            {
                case BuildingType.Infrastructure:
                    switch (upgradeLevel)
                    {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 15f },
                                { Resource.Wood, 25f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 25f },
                                { Resource.Wood, 10f },
                                { Resource.Iron, 25f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                case BuildingType.Fort:
                    switch (upgradeLevel)
                    {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                { Resource.Wood, 20f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.Wood, 10f },
                                { Resource.Iron, 25f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 30f },
                                { Resource.Wood, 10f },
                                { Resource.Iron, 40f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                case BuildingType.Mine:
                    switch (upgradeLevel)
                    {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.Wood, 10f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.Wood, 30f },
                                { Resource.Iron, 5f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                case BuildingType.School:
                    switch (upgradeLevel)
                    {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 100f },
                                { Resource.Wood, 50f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 150f },
                                { Resource.Wood, 300f },
                                { Resource.Iron, 50f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 250f },
                                { Resource.Wood, 100f },
                                { Resource.Iron, 300f },
                                { Resource.SciencePoint, 10f },
                                { Resource.AP, TurnActionApCost(ActionType.BuildingUpgrade) }
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
                { Resource.AP, TurnActionApCost(ActionType.TechnologyUpgrade) },
                { Resource.SciencePoint, 10 + (10 * techLvl) + (5 * allTechLvl) }
            };
        }

        private static float IntegrateVassalApCost(Relation vassalage)
        {
            return (vassalage?.Sides[1].Provinces.Count / 5) ?? 1f;
        }

        private static Dictionary<Resource, float> ArmyRecruitmentCost(Country.TechnologyInterpreter techStats)
        {
            var armyCost = techStats.armyCost;
            return new Dictionary<Resource, float> {
                { Resource.Gold, 1f * armyCost},
                { Resource.AP, TurnActionApCost(ActionType.ArmyRecruitment) },
            };
        }

        /// <summary>
        /// Generic method for non-building, non-technology and non-recruitment based actions
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> TurnActionFullCost(ActionType actionType)
        {
            return new Dictionary<Resource, float> {
                { Resource.AP, TurnActionApCost(actionType) }
            };
        }

        /// <summary>
        /// Method for am army recruitment action
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="tech"></param>
        /// <param name="techType"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> TurnActionFullCost(ActionType actionType, Country.TechnologyInterpreter techStats)
        {
            if (actionType == ActionType.ArmyRecruitment)
            {
                return ArmyRecruitmentCost(techStats);
            }
            return TurnActionFullCost(actionType);
        }

        /// <summary>
        /// Method for technology based actions
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="tech"></param>
        /// <param name="techType"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> TurnActionFullCost(ActionType actionType,
            Dictionary<Technology, int> tech, Technology techType)
        {
            if (actionType == ActionType.TechnologyUpgrade)
            {
                return TechCost(tech, techType);
            }
            return TurnActionFullCost(actionType);
        }

        /// <summary>
        /// Method for building based actions
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="buildingType"></param>
        /// <param name="upgradeLevel"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> TurnActionFullCost(ActionType actionType, BuildingType buildingType, int upgradeLevel)
        {
            if (actionType == ActionType.BuildingUpgrade)
            {
                return BuildingCost(buildingType, upgradeLevel);
            }
            return TurnActionFullCost(actionType);
        }

        /// <summary>
        /// Method for rebellion
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="vassalage"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> TurnActionFullCost(ActionType actionType, Relation vassalage)
        {
            if (actionType == ActionType.IntegrateVassal)
            {
                return new Dictionary<Resource, float> {
                    { Resource.AP, IntegrateVassalApCost(vassalage) },
                };
            }
            else
            {
                return TurnActionFullCost(actionType);
            }
        }

        public static Dictionary<Resource, float> TurnActionAltCost(ActionType actionType)
        {
            var fullCost = TurnActionFullCost(actionType);
            return RemoveApCost(fullCost);
        }
        public static Dictionary<Resource, float> TurnActionAltCost(ActionType actionType, Country.TechnologyInterpreter techStats)
        {
            var fullCost = TurnActionFullCost(actionType, techStats);
            return RemoveApCost(fullCost);
        }

        public static Dictionary<Resource, float> TurnActionAltCost(ActionType actionType,
            Dictionary<Technology, int> tech, Technology techType)
        {
            var fullCost = TurnActionFullCost(actionType, tech, techType);
            return RemoveApCost(fullCost);
        }

        public static Dictionary<Resource, float> TurnActionAltCost(ActionType actionType, BuildingType bType, int lvl)
        {
            var fullCost = TurnActionFullCost(actionType, bType, lvl);
            return RemoveApCost(fullCost);
        }

        public static Dictionary<Resource, float> TurnActionAltCost(ActionType actionType, Relation vassalage)
        {
            var fullCost = TurnActionFullCost(actionType, vassalage);
            return RemoveApCost(fullCost);
        }

        private static Dictionary<Resource, float> RemoveApCost(Dictionary<Resource, float> fullCost)
        {
            fullCost.Remove(Resource.AP);
            return fullCost.Count > 0 ? fullCost : new Dictionary<Resource, float>();
        }
        private const float RebelSupCost = 2.5f;
        private const float HardActionCost = 1f;
        private const float SoftActionCost = 0.1f;

        public static float TurnActionApCost(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.RebelSuppresion:
                    return RebelSupCost;
                case ActionType.ArmyRecruitment:
                case ActionType.ArmyMove:
                case ActionType.StartWar:
                case ActionType.MilAccOffer:
                case ActionType.AllianceOffer:
                case ActionType.AllianceEnd:
                case ActionType.MilAccRequest:
                case ActionType.Praise:
                case ActionType.Insult:
                case ActionType.SubsRequest:
                case ActionType.CallToWar:
                case ActionType.SubsOffer:
                case ActionType.VassalizationOffer:
                case ActionType.VassalRebel:
                case ActionType.BuildingUpgrade:
                    return HardActionCost;
                case ActionType.WarEnd:
                case ActionType.ArmyDisbandment:
                case ActionType.MilAccEndMaster:
                case ActionType.MilAccEndSlave:
                case ActionType.SubsEnd:
                case ActionType.TechnologyUpgrade:
                case ActionType.BuildingDowngrade:
                case ActionType.FestivitiesOrganization:
                case ActionType.TaxBreakIntroduction:
                    return SoftActionCost;
                default:
                    return SoftActionCost;
            }
        }

        public static float TurnActionApCost(ActionType actionType, Relation vassalage)
        {
            if (actionType == ActionType.IntegrateVassal)
            {
                return IntegrateVassalApCost(vassalage);
            }
            return TurnActionApCost(actionType);
        }
    }
}