using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.classes.subclasses {
    class technicalDefaultResources {
        public static Dictionary<Resource, float> defaultValues = new Dictionary<Resource, float> {
            { Resource.Gold, 100f },
            { Resource.AP, 2.5f },
            { Resource.Wood, 50f },
            { Resource.Iron, 20f },
            { Resource.SciencePoint, 0f }
        };
    }
}
