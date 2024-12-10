using System.Collections.Generic;

namespace Assets.classes.subclasses {
    class TechnicalDefaultResources {
        public static Dictionary<Resource, float> defaultValues = new() {
            { Resource.Gold, 100f },
            { Resource.AP, 2.5f },
            { Resource.Wood, 50f },
            { Resource.Iron, 20f },
            { Resource.SciencePoint, 0f }
        };
    }
}
