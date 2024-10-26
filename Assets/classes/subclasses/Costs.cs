using System.Collections.Generic;
using System.Linq;
using static Assets.classes.actionContainer.TurnAction;

namespace Assets.classes.subclasses {
    public static class CostsCalculator
    {
        public static Dictionary<Resource, float> bCost(BuildingType type, int lvl)
        {
            switch (type)
            {
                case BuildingType.Infrastructure:
                    switch (lvl)
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
                    switch (lvl)
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
                    switch (lvl)
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
                    switch (lvl)
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

        public static float IntegrateVassalApCost(Relation vassalage)
        {
            return (vassalage?.Sides[1].Provinces.Count / 5) ?? 1f;
        }

        public static Dictionary<Resource, float> TurnActionFullCost(ActionType actionType, 
            Dictionary<Technology, int> tech = null, Technology techType = Technology.Military,
            BuildingType bType = BuildingType.Fort, int lvl = 1,
            Relation vassalage = null)
        {
            switch(actionType)
            {
                case ActionType.BuildingUpgrade:
                    return bCost(bType, lvl);
                case ActionType.TechnologyUpgrade:
                    return TechCost(tech, techType);
                case ActionType.IntegrateVassal:
                    return new Dictionary<Resource, float> {
                        { Resource.AP, IntegrateVassalApCost(vassalage) },
                    };
                case ActionType.ArmyRecruitment:
                    return new Dictionary<Resource, float> {
                        { Resource.Gold, 1f },
                        { Resource.AP, TurnActionApCost(ActionType.ArmyRecruitment) },
                    };
                default:
                    return new Dictionary<Resource, float> {
                    { Resource.AP, TurnActionApCost(actionType) }
                };
            }
        }

        public static Dictionary<Resource, float> TurnActionAltCost(ActionType actionType,
            Dictionary<Technology, int> tech = null, Technology techType = Technology.Military,
            BuildingType bType = BuildingType.Fort, int lvl = 1,
            Relation vassalage = null)
        {
            var fullCost = TurnActionFullCost(actionType, tech, techType, bType, lvl, vassalage);

            fullCost.Remove(Resource.AP);

            return fullCost.Count > 0 ? fullCost : new Dictionary<Resource, float>();
        }

        public static readonly float HardActionCost = 1f;
        public static readonly float SoftActionCost = 0.1f;

        public static float TurnActionApCost(ActionType actionType, Relation vassalage = null)
        {
            switch (actionType)
            {
                case ActionType.IntegrateVassal:
                    return IntegrateVassalApCost(vassalage);
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
                    return SoftActionCost;
                default:
                    return SoftActionCost;
            }
        }
    }
}