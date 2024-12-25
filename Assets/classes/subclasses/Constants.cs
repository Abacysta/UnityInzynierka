using UnityEngine;

namespace Assets.classes.subclasses.Constants {
    internal static class ProvinceConstants {
        //extremes
        public const int MAX_POP = 1000000000;

        //population needed for a school
        public const ushort SCHOOL_MIN_POP = 3000;

        //minimum amount of resource in province
        public const float MIN_RESOURCE_PROVINCE = 0.1f;

        //default coordinates
        public static readonly (short, short) DEFAULT_CORD = (-1, -1);
        
        //happ
        public const short MIN_HAPP = 0;
        public const short MAX_HAPP = 100;
        public const short GROWTH_HAPP = 3;
        
        //opinion
        public const short MIN_OPINION = -200;
        public const short MAX_OPINION = 200;

    }

    internal static class RelationConstants {
        public const short WAR_OPINION_PENALTY_INIT = -100;
        public const byte WAR_HAPP_PENALTY_INIT_C1 = 15;
        public const byte WAR_HAPP_PENALTY_INIT_C2 = 5;
        public const short WAR_OPINION_PENALTY_CONST = -15;
        public const byte WAR_HAPP_PENALTY_CONST = 2;


        public const short ALLIANCE_OPINION_BONUS_INIT = 100;
        public const byte ALLIANCE_HAPP_BONUS_INIT = 3;
        public const short ALLIANCE_OPINION_BONUS_CONST = 10;
        public const byte ALLIANCE_HAPP_BONUS_CONST = 1;


        public const short VASSALAGE_OPINION_PENALTY_INIT_C2 = -30;
        public const byte VASSALAGE_HAPP_PENALTY_INIT_C2 = 8;
        public const byte VASSALAGE_HAPP_BONUS_INIT_C1 = 5;
        public const short VASSALAGE_OPINION_PENALTY_CONST_C2 = -5;
        public const byte VASSALAGE_HAPP_BONUS_CONST_C1 = 1;
        public const byte VASSALAGE_HAPP_PENALTY_CONST_C2 = 1;


        public const short PRAISE_OUR_OPINION_BONUS_INIT = 20;
        public const short PRAISE_THEIR_OPINION_BONUS_INIT = PRAISE_OUR_OPINION_BONUS_INIT / 2;

        public const short INSULT_OUR_OPINION_PENALTY_INIT = 25;
        public const short INSULT_THEIR_OPINION_PENALTY_INIT = INSULT_OUR_OPINION_PENALTY_INIT / 2;

        public const short TRUCE_OPINION_BONUS_INIT = 10;
        public const short TRUCE_OPINION_BONUS_CONST = 0;

        public const short SUBSIDIES_OPINION_BONUS_INIT = 20;
        public const short SUBSIDIES_OPINION_BONUS_CONST = 5;

        public const short DECLINE_WAR_OPINION_PENALTY_INIT = 100;

        // relation colors
        public static readonly Color WAR_COLOR = new(1f, 0f, 0f); // Red
        public static readonly Color TRUCE_COLOR = new(0.8f, 0.9f, 0.8f); // Light Green
        public static readonly Color ALLIANCE_COLOR = new(0.5f, 0.8f, 1f); // Light Blue
        public static readonly Color VASSALAGE_COLOR = new(0.6f, 0.4f, 0.8f); // Purple
        public static readonly Color REBELLION_COLOR = new(0.8f, 0.4f, 0.8f); // Pink
        public static readonly Color DEFAULT_COLOR = new(0.96f, 0.76f, 0.76f); // Soft Salmon
        public static readonly Color TRIBAL_COLOR = new(0.9f, 0.75f, 0.6f); // Light Beige
        public static readonly Color CURRENT_PLAYER_COLOR = new(0.97f, 0.92f, 0.46f); // Yellow

        public static Color GetDiplomaticColor(Relation.RelationType? relationType)
        {
            switch (relationType)
            {
                case Relation.RelationType.War:
                    return WAR_COLOR;
                case Relation.RelationType.Truce:
                    return TRUCE_COLOR;
                case Relation.RelationType.Alliance:
                    return ALLIANCE_COLOR;
                case Relation.RelationType.Vassalage:
                    return VASSALAGE_COLOR;
                case Relation.RelationType.Rebellion:
                    return REBELLION_COLOR;
                default:
                    return DEFAULT_COLOR;
            }
        }
    }
}
