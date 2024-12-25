using System.Collections.Generic;
using System.Linq;
using static Assets.classes.TurnAction;

namespace Assets.classes.subclasses
{
    public static class CostCalculator
    {
        private static Dictionary<Resource, float> GetBuildingCost(BuildingType buildingType, int upgradeLevel)
        {
            switch (buildingType)
            {
                case BuildingType.Infrastructure:
                    switch (upgradeLevel)
                    {
                        case 1:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 15f },
                                { Resource.Wood, 25f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 25f },
                                { Resource.Wood, 10f },
                                { Resource.Iron, 25f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
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
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.Wood, 10f },
                                { Resource.Iron, 25f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 30f },
                                { Resource.Wood, 10f },
                                { Resource.Iron, 40f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
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
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.Wood, 10f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 10f },
                                { Resource.Wood, 30f },
                                { Resource.Iron, 5f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
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
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 2:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 150f },
                                { Resource.Wood, 300f },
                                { Resource.Iron, 50f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        case 3:
                            return new Dictionary<Resource, float>() {
                                { Resource.Gold, 250f },
                                { Resource.Wood, 100f },
                                { Resource.Iron, 300f },
                                { Resource.SciencePoint, 10f },
                                { Resource.AP, GetTurnActionApCost(ActionType.BuildingUpgrade) }
                            };
                        default:
                            return new Dictionary<Resource, float>();
                    }
                default:
                    return new Dictionary<Resource, float>();
            }
        }
        public static Dictionary<Resource, float> GetTechCost(Dictionary<Technology, int> tech, Technology type)
        {
            int techLvl = tech[type];
            int allTechLvl = tech.Values.Sum();

            return new Dictionary<Resource, float> {
                { Resource.AP, GetTurnActionApCost(ActionType.TechnologyUpgrade) },
                { Resource.SciencePoint, 10 + (10 * techLvl) + (5 * allTechLvl) }
            };
        }

        private static float GetIntegrateVassalApCost(Relation vassalage)
        {
            return (vassalage?.Sides[1].Provinces.Count / 5) ?? 1f;
        }

        private static Dictionary<Resource, float> GetArmyRecruitmentCost(TechnologyInterpreter techStats)
        {
            var armyCost = techStats.ArmyCost;
            return new Dictionary<Resource, float> {
                { Resource.Gold, 1f * armyCost},
                { Resource.AP, GetTurnActionApCost(ActionType.ArmyRecruitment) },
            };
        }

        /// <summary>
        /// A generic method for most actions
        /// </summary>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> GetTurnActionFullCost(ActionType actionType)
        {
            return new Dictionary<Resource, float> {
                { Resource.AP, GetTurnActionApCost(actionType) }
            };
        }

        /// <summary>
        /// Method for army recruitment action
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="tech"></param>
        /// <param name="techType"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> GetTurnActionFullCost(ActionType actionType, TechnologyInterpreter techStats)
        {
            if (actionType == ActionType.ArmyRecruitment)
            {
                return GetArmyRecruitmentCost(techStats);
            }
            return GetTurnActionFullCost(actionType);
        }

        /// <summary>
        /// Method for technology upgrade action
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="tech"></param>
        /// <param name="techType"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> GetTurnActionFullCost(ActionType actionType,
            Dictionary<Technology, int> tech, Technology techType)
        {
            if (actionType == ActionType.TechnologyUpgrade)
            {
                return GetTechCost(tech, techType);
            }
            return GetTurnActionFullCost(actionType);
        }

        /// <summary>
        /// Method for building upgrade action
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="buildingType"></param>
        /// <param name="upgradeLevel"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> GetTurnActionFullCost(ActionType actionType, BuildingType buildingType, int upgradeLevel)
        {
            if (actionType == ActionType.BuildingUpgrade)
            {
                return GetBuildingCost(buildingType, upgradeLevel);
            }
            return GetTurnActionFullCost(actionType);
        }

        /// <summary>
        /// Method for integrate vassal action
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="vassalage"></param>
        /// <returns></returns>
        public static Dictionary<Resource, float> GetTurnActionFullCost(ActionType actionType, Relation vassalage)
        {
            if (actionType == ActionType.IntegrateVassal)
            {
                return new Dictionary<Resource, float> {
                    { Resource.AP, GetIntegrateVassalApCost(vassalage) },
                };
            }
            else
            {
                return GetTurnActionFullCost(actionType);
            }
        }

        /// <summary>
        /// A generic method for most actions
        /// </summary>
        public static Dictionary<Resource, float> GetTurnActionAltCost(ActionType actionType)
        {
            var fullCost = GetTurnActionFullCost(actionType);
            return RemoveApCost(fullCost);
        }

        /// <summary>
        /// Method for army recruitment action
        /// </summary>
        public static Dictionary<Resource, float> GetTurnActionAltCost(ActionType actionType, TechnologyInterpreter techStats)
        {
            var fullCost = GetTurnActionFullCost(actionType, techStats);
            return RemoveApCost(fullCost);
        }

        /// <summary>
        /// Method for technology upgrade action
        /// </summary>
        public static Dictionary<Resource, float> GetTurnActionAltCost(ActionType actionType,
            Dictionary<Technology, int> tech, Technology techType)
        {
            var fullCost = GetTurnActionFullCost(actionType, tech, techType);
            return RemoveApCost(fullCost);
        }

        /// <summary>
        /// Method for building upgrade action
        /// </summary>
        public static Dictionary<Resource, float> GetTurnActionAltCost(ActionType actionType, BuildingType bType, int lvl)
        {
            var fullCost = GetTurnActionFullCost(actionType, bType, lvl);
            return RemoveApCost(fullCost);
        }

        /// <summary>
        /// Method for integrate vassal action
        /// </summary>
        public static Dictionary<Resource, float> GetTurnActionAltCost(ActionType actionType, Relation vassalage)
        {
            var fullCost = GetTurnActionFullCost(actionType, vassalage);
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

        public static float GetTurnActionApCost(ActionType actionType)
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
                return GetIntegrateVassalApCost(vassalage);
            }
            return GetTurnActionApCost(actionType);
        }
    }
}