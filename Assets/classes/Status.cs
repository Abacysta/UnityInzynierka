using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.classes.subclasses {
    [Serializable]
    public class Status {
        public enum StatusType {
            positive = 1,
            negative = -1,
            neutral = 0
        }
        public int id;
        public int duration;
        public string description;
        public StatusType type;

        public Status(int duration, StatusType type, string description, int id) {
            this.duration = duration;
            this.type = type;
            this.description = description;
            this.id = id;
        }

        public virtual void applyEffect(Province province) { }
    }
    [Serializable]
    internal class TaxBreak:Status {
        public TaxBreak(int duration) : base(duration, StatusType.positive, "The province is exempted from paying tax", 1) {
        }

        public override void applyEffect(Province province) {
            province.Tax_mod = 0;
            province.Happ_mod += 0.2f;
            province.Happ_static += 5;
        }
    }
	[Serializable]
	internal class Festivities:Status {
        public Festivities(int duration) : base(duration, StatusType.positive, "Festivities are taking place in this province", 2){}

        public override void applyEffect(Province province) {
            province.Prod_mod -= 0.15f;
            province.Pop_mod += 0.15f;
            province.Happ_static += 3;
        }
    }

	[Serializable]
	internal class ProdBoom:Status {
        public ProdBoom(int duration) : base(duration, StatusType.positive, "This province is experiencing a temporary production boom", 3) { }

        public override void applyEffect(Province province) {
            province.Prod_mod += 0.15f;
        }
    }
	[Serializable]
	internal class ProdDown:Status {
        public ProdDown(int duration) : base(duration, StatusType.negative, "This province is experiencing a temporary recession", 4) { }

        public override void applyEffect(Province province) {
            province.Prod_mod -= 0.15f;
        }
    }
	[Serializable]
	internal class Tribal:Status {
        public Tribal(int duration) : base (duration, StatusType.neutral, "No civilization has been introduced in this province", 0) { }

        public override void applyEffect(Province province) {
            province.Pop_mod = 0.5f;
            province.Happ_mod = 0;
            province.Happ_static = 0;
            province.Prod_mod = 1;
            province.Rec_pop = 0;
            province.Tax_mod = 0;
        }
    }
	[Serializable]
	internal class Illness:Status {
        public Illness(int duration) : base(duration, StatusType.negative, "This province is going through a plague", 5) { }

        public override void applyEffect(Province province) {
            province.Pop_mod -= 0.6f;
            province.Pop_static -= province.Population / 10;
            province.Happ_static -= 4;
        }
    }
	[Serializable]
	internal class Disaster:Status {
        public Disaster(int duration) : base(duration, StatusType.negative, "A disaster has struck this province", 6) { }

        public override void applyEffect(Province province) {
            province.Pop_mod -= 0.1f;
            province.Prod_mod -= 0.3f;
        }
    }
	[Serializable]
	internal class Occupation : Status
    {
        public int Occupier_id { get; private set; }
        public Occupation(int duration, int occupierId) : base(duration, StatusType.negative, "This province is currently occupied", 7) 
        {
            this.Occupier_id = occupierId;
        }

        public override void applyEffect(Province province)
        {
            province.OccupationInfo.OccupyingCountryId = this.Occupier_id;
        }
    }
	[Serializable]
	internal class RecBoom:Status {
        public RecBoom(int duration) : base(duration, StatusType.neutral, "More recruits appear, hindering your economic growth", 8) {
        }

        public override void applyEffect(Province province) {
            province.Rec_pop += 0.02f;
            province.Prod_mod -= 0.03f;
        }
    }
	[Serializable]
	internal class Flood:Status
    {
        public Flood(int duration) : base(duration, StatusType.negative, "This province is flooded.", 9)
        {
        }

        public override void applyEffect(Province province)
        {
            province.Rec_pop -= 0.05f;
            province.Prod_mod -= 0.5f;
        }
    }
	[Serializable]
	internal class Fire : Status
    {
        public Fire(int duration) : base(duration, StatusType.negative, "This province is on fire!", 10)
        {
        }

        public override void applyEffect(Province province)
        {
            province.Rec_pop -= 0.15f;
            province.Prod_mod -= 1.5f;
        }
    }
}

