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

        public const float TaxMod = 0f;
        public const float HappMod = 0.2f;
        public const float HappStatic = 5f;

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

        public const float ProdMod = -0.15f;
        public const float PopMod = 0.15f;
        public const float HappStatic = 3f;

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
        public const float ProdMod = 0.15f;

        public ProdBoom(int duration) : base(duration, StatusType.positive, 
            "This province is experiencing a temporary production boom", 3) {}

        public override void applyEffect(Province province) {
            province.Modifiers.ProdMod += ProdMod;
        }
    }

	[Serializable]
	internal class ProdDown : Status {
        public const float ProdMod = -0.15f;

        public ProdDown(int duration) : base(duration, StatusType.negative, 
            "This province is experiencing a temporary recession", 4) {}

        public override void applyEffect(Province province) {
            province.Modifiers.ProdMod += ProdMod;
        }
    }

	[Serializable]
	internal class Tribal : Status {
        public const float PopMod = 0.5f;
        public const float HappMod = 0f;
        public const float HappStatic = 0f;
        public const float ProdMod = 1f;
        public const float RecPop = 0f;
        public const float TaxMod = 0f;

        public Tribal(int duration) : base (duration, StatusType.neutral, 
            "No civilization has been introduced in this province", 0) {}

        public override void applyEffect(Province province) {
            province.Modifiers.PopMod = PopMod;
            province.Modifiers.HappMod = HappMod;
            province.Modifiers.HappStatic = HappStatic;
            province.Modifiers.ProdMod = ProdMod;
            province.Modifiers.RecPop = RecPop;
            province.Modifiers.TaxMod = TaxMod;
        }
    }

	[Serializable]
	internal class Illness : Status {
        public const float PopMod = -0.6f;
        public const float HappStatic = -4f;
        public const float PopulationDivisor = 10;

        public Illness(int duration) : base(duration, StatusType.negative, 
            "This province is going through a plague", 5) {}

        public override void applyEffect(Province province) {
            province.Modifiers.PopMod += PopMod;
            province.Modifiers.PopStatic -= province.Population / PopulationDivisor;
            province.Modifiers.HappStatic += HappStatic;
        }
    }

	[Serializable]
	internal class Disaster : Status {
        public const float PopMod = -0.1f;
        public const float ProdMod = -0.3f;

        public Disaster(int duration) : base(duration, StatusType.negative, 
            "A disaster has struck this province", 6) {}

        public override void applyEffect(Province province) {
            province.Modifiers.PopMod += PopMod;
            province.Modifiers.ProdMod += ProdMod;
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
        public const float RecPop = 0.02f;
        public const float ProdMod = -0.03f;

        public RecBoom(int duration) : base(duration, StatusType.neutral, 
            "More recruits appear, hindering your economic growth", 8) {}

        public override void applyEffect(Province province) {
            province.Modifiers.RecPop += RecPop;
            province.Modifiers.ProdMod += ProdMod;
        }
    }

	[Serializable]
	internal class FloodStatus : Status
    {
        public const float RecPop = -0.05f;
        public const float ProdMod = -0.5f;

        public FloodStatus(int duration) : base(duration, StatusType.negative, 
            "This province is flooded.", 9) {}

        public override void applyEffect(Province province)
        {
            province.Modifiers.RecPop += RecPop;
            province.Modifiers.ProdMod += ProdMod;
        }
    }

	[Serializable]
	internal class FireStatus : Status
    {
        public const float RecPop = -0.15f;
        public const float ProdMod = -0.7f;

        public FireStatus(int duration) : base(duration, StatusType.negative, 
            "This province is on fire!", 10) {}

        public override void applyEffect(Province province)
        {
            province.Modifiers.RecPop += RecPop;
            province.Modifiers.ProdMod += ProdMod;
        }
    }
}

