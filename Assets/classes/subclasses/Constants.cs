namespace Assets.classes.subclasses.Constants {
    internal static class ProvinceConstants {
        //population needed for a school
        public const ushort SCHOOL_MIN_POP = 3000;

        //minimum amount of resource in province
        public const float MIN_RESOURCE_PROVINCE = 0.1f;

        //default coordinates
        public static readonly (short, short) DEFAULT_CORD = (-1, -1);
        
        //happ
        public const short MIN_HAPP = 0;
        public const short MAX_HAPP = 100;
        
        //opinion
        public const short MIN_OPINION = -200;
        public const short MAX_OPINION = 200;

    }

    internal static class RelationConstants {
        public const byte WAR_HAPP_PENALTY_COST = 2;
        public const byte ALLIANCE_HAPP_BONUS_INIT = 3;
        public const byte ALLIANCE_HAPP_BONUS_COST = 1;
        public const byte VASSALAGE_HAPP_BONUS_C1 = 1;
        public const byte VASSALAGE_HAPP_PENALTY_C2= 1;

        public const short PRAISE_OUR_OPINION_BONUS_INIT = 20;
        public const short PRAISE_THEIR_OPINION_BONUS_INIT = PRAISE_OUR_OPINION_BONUS_INIT / 2;
        public const short INSULT_OUR_OPINION_PENALTY_INIT = 25;
        public const short INSULT_THEIR_OPINION_PENALTY_INIT = INSULT_OUR_OPINION_PENALTY_INIT / 2;


        public const short WAR_OPINION_PENALTY_INIT = -100;
        public const short WAR_OPINION_PENALTY_CONST = -15;

        public const short ALLIANCE_OPINION_BONUS_INIT = 100;
        public const short ALLIANCE_OPINION_BONUS_CONST = 10;

        public const short TRUCE_OPINION_BONUS_INIT = 10;
        public const short TRUCE_OPINION_BONUS_CONST = 0;


        public const short VASSALAGE_OPINION_PENALTY_INIT_C2 = -30;
        public const short VASSALAGE_OPINION_PENALTY_CONST_C2 = -5;

        public const short SUBSIDIES_OPINION_BONUS_INIT = 20;
        public const short SUBSIDIES_OPINION_BONUS_CONST = 5;

        public const byte WAR_HAPP_PENALTY_INIT_C1 = 15;
        public const byte WAR_HAPP_PENALTY_INIT_C2 = 5;
        public const byte VASSALAGE_HAPP_PENALTY_INIT_C2 = 8;
        public const byte VASSALAGE_HAPP_BONUS_INIT_C1 = 5;

        public const short DECLINE_WAR_OPINION_PENALTY_INIT = 100;
    }
}
