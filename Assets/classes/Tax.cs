using System;

namespace Assets.classes {
    namespace Tax {
        
        public abstract class ATax {
            public virtual float GoldP { get; }
            public virtual int HappP { get; }
            public virtual string Name { get; }
            public virtual void applyCountryTax(Country country) {
                float sum = 0;
                foreach (var prov in country.Provinces) {
                    applyProvinceTax(prov, country, ref sum);
                }
                sum = (float)Math.Round(sum, 1) * country.techStats.taxFactor;
                country.modifyResource((Resource.Gold), sum);
            }
            public virtual void applyProvinceTax(Province province, Country country, ref float sum) {
                sum += GoldP * province.Population / 500 * province.Modifiers.TaxMod;
                province.Happiness += HappP;
            }
            public virtual float getProjectedTax(Country country) {
                float goldsum = 0;
                foreach (var prov in country.Provinces) {
                    goldsum += GoldP * prov.Population / 100 * prov.Modifiers.TaxMod;
                }
                return (float)Math.Round(goldsum, 1) * country.techStats.taxFactor;
            }
        }
        internal class LowTaxes : ATax {
            public float GoldP { get => 0.01f; }

            public int HappP { get => 8; }

            public string Name { get => "Low Taxation"; }

        }
        internal class HighTaxes : ATax {
            public float GoldP { get => 0.35f; }

            public int HappP { get => -8; }

            public string Name { get => "High Taxation"; }
            public override void applyProvinceTax(Province province, Country country, ref float sum) {
                base.applyProvinceTax(province, country, ref sum);
                province.RecruitablePopulation -= (int)(province.RecruitablePopulation * 0.25);
            }
        }
        internal class MediumTaxes : ATax {
            public float GoldP { get => 0.12f; }

            public int HappP { get => -3; }

            public string Name { get => "Normal Taxation"; }
        }
        internal class WarTaxes : ATax {
            public float GoldP { get => 0.35f; }

            public int HappP { get => 0; }
            public string Name { get => "War Taxation"; }
            public override void applyProvinceTax(Province province, Country country, ref float sum) {
                base.applyProvinceTax(province, country, ref sum);
                province.ResourceAmount -= 0.1f;
                if (!country.AtWar) {
                    province.Happiness -= 15;
                }
            }
        }
        internal class InvesmentTaxes : ATax {
            public float GoldP { get => 0.05f; }

            public int HappP { get => -3; }

            public string Name { get => "Investments"; }
            public override void applyProvinceTax(Province province, Country country, ref float sum) {
                base.applyProvinceTax(province, country, ref sum);
                province.ResourceAmount += 0.15f;
                province.RecruitablePopulation /= 2;
            }
        }
    }
}
