using System;

namespace Assets.classes.subclasses {
    [Serializable]
    public class Status {
        public enum StatusType {
            positive = 1,
            negative = -1,
            neutral = 0
        }

        public int Id { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
        public StatusType Type { get; set; }

        public Status(int duration, StatusType type, string description, int id) {
            Duration = duration;
            Type = type;
            Description = description;
            Id = id;
        }

        public virtual void applyEffect(Province province) {}
    }

    [Serializable]
    internal class TaxBreak : Status {

        public static readonly float TaxMod = 0f;
        public static readonly float HappMod = 0.2f;
        public static readonly float HappStatic = 5f;

        public TaxBreak(int duration) : base(duration, StatusType.positive, 
            "The province is exempted from paying tax", 1) {}

        public override void applyEffect(Province province) {
            province.Modifiers.TaxMod = TaxMod;
            province.Modifiers.HappMod += HappMod;
            province.Modifiers.HappStatic += HappStatic;
        }
    }

	[Serializable]
	internal class Festivities : Status {

        public static readonly float ProdMod = -0.15f;
        public static readonly float PopMod = 0.15f;
        public static readonly float HappStatic = 3f;

        public Festivities(int duration) : base(duration, StatusType.positive, 
            "Festivities are taking place in this province", 2) {}

        public override void applyEffect(Province province) {
            province.Modifiers.ProdMod += ProdMod;
            province.Modifiers.PopMod += PopMod;
            province.Modifiers.HappStatic += HappStatic;
        }
    }

	[Serializable]
	internal class ProdBoom : Status {
        public ProdBoom(int duration) : base(duration, StatusType.positive, 
            "This province is experiencing a temporary production boom", 3) {}

        public override void applyEffect(Province province) {
            province.Modifiers.ProdMod += 0.15f;
        }
    }

	[Serializable]
	internal class ProdDown : Status {
        public ProdDown(int duration) : base(duration, StatusType.negative, 
            "This province is experiencing a temporary recession", 4) {}

        public override void applyEffect(Province province) {
            province.Modifiers.ProdMod -= 0.15f;
        }
    }

	[Serializable]
	internal class Tribal : Status {
        public Tribal(int duration) : base (duration, StatusType.neutral, 
            "No civilization has been introduced in this province", 0) {}

        public override void applyEffect(Province province) {
            province.Modifiers.PopMod = 0.5f;
            province.Modifiers.HappMod = 0;
            province.Modifiers.HappStatic = 0;
            province.Modifiers.ProdMod = 1;
            province.Modifiers.RecPop = 0;
            province.Modifiers.TaxMod = 0;
        }
    }

	[Serializable]
	internal class Illness : Status {
        public Illness(int duration) : base(duration, StatusType.negative, 
            "This province is going through a plague", 5) {}

        public override void applyEffect(Province province) {
            province.Modifiers.PopMod -= 0.6f;
            province.Modifiers.PopStatic -= province.Population / 10;
            province.Modifiers.HappStatic -= 4;
        }
    }

	[Serializable]
	internal class Disaster : Status {
        public Disaster(int duration) : base(duration, StatusType.negative, 
            "A disaster has struck this province", 6) {}

        public override void applyEffect(Province province) {
            province.Modifiers.PopMod -= 0.1f;
            province.Modifiers.ProdMod -= 0.3f;
        }
    }

	[Serializable]
	internal class Occupation : Status
    {
        public int Occupier_id { get; private set; }
        public Occupation(int duration, int occupierId) : base(duration, StatusType.negative, 
            "This province is currently occupied", 7) 
        {
            this.Occupier_id = occupierId;
        }

        public override void applyEffect(Province province)
        {
            province.OccupationInfo.OccupyingCountryId = this.Occupier_id;
        }
    }

	[Serializable]
	internal class RecBoom : Status {
        public RecBoom(int duration) : base(duration, StatusType.neutral, 
            "More recruits appear, hindering your economic growth", 8) {}

        public override void applyEffect(Province province) {
            province.Modifiers.RecPop += 0.02f;
            province.Modifiers.ProdMod -= 0.03f;
        }
    }

	[Serializable]
	internal class FloodStatus : Status
    {
        public FloodStatus(int duration) : base(duration, StatusType.negative, 
            "This province is flooded.", 9) {}

        public override void applyEffect(Province province)
        {
            province.Modifiers.RecPop -= 0.05f;
            province.Modifiers.ProdMod -= 0.5f;
        }
    }

	[Serializable]
	internal class FireStatus : Status
    {
        public FireStatus(int duration) : base(duration, StatusType.negative, 
            "This province is on fire!", 10) {}

        public override void applyEffect(Province province)
        {
            province.Modifiers.RecPop -= 0.15f;
            province.Modifiers.ProdMod -= .7f;
        }
    }
}

