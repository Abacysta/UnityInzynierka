using System;

namespace Assets.classes {
    namespace Tax {
        
        public interface ITax {
            public abstract float GoldP { get; }
            public abstract int HappP { get; }
            public abstract string Name { get; }
            public void applyCountryTax(Country country) {
                float sum = 0;
                foreach (var prov in country.Provinces) {
                    applyProvinceTax(prov, country, ref sum);
                }
                sum = (float)Math.Round(sum, 1) * country.techStats.taxFactor;
                country.modifyResource((Resource.Gold), sum);
            }
            public void applyProvinceTax(Province province, Country country, ref float sum) {
                sum += GoldP * province.Population / 100 * province.Modifiers.TaxMod;
                province.Happiness += HappP;
            }
            public float getProjectedTax(Country country) {
                float goldsum = 0;
                foreach (var prov in country.Provinces) {
                    goldsum += GoldP * prov.Population / 100 * prov.Modifiers.TaxMod;
                }
                return (float)Math.Round(goldsum, 1) * country.techStats.taxFactor;
            }
        }
        internal class LowTaxes : ITax {
            public float GoldP { get => 0.02f; }

            public int HappP { get => 8; }

            public string Name { get => "Low Taxation"; }
        }
        internal class HighTaxes : ITax {
            public float GoldP { get => 0.35f; }

            public int HappP { get => -8; }

            public string Name { get => "High Taxation"; }

            public void applyProvinceTax(Province province, Country country, ref float sum) { 
                ((ITax)this).applyProvinceTax(province, country, ref sum);
                province.RecruitablePopulation -= (int)(province.RecruitablePopulation * 0.25);
            }
        }
        internal class MediumTaxes : ITax {
            public float GoldP { get => 0.15f; }

            public int HappP { get => -3; }

            public string Name { get => "Normal Taxation"; }
        }
        internal class WarTaxes : ITax {
            public float GoldP { get => 0.35f; }

            public int HappP { get => 0; }
            public string Name { get => "War Taxation"; }
            public void applyProvinceTax(Province province, Country country, ref float sum) {
                ((ITax)this).applyProvinceTax(province, country, ref sum);
                province.Resources_amount -= 0.1f;
                if (!country.AtWar) {
                    province.Happiness -= 15;
                }
            }
        }
        internal class InvesmentTaxes : ITax {
            public float GoldP { get => 0.05f; }

            public int HappP { get => -3; }

            public string Name { get => "Investments"; }
            public void applyProvinceTax(Province province, Country country, ref float sum) {
                ((ITax)this).applyProvinceTax(province, country, ref sum);
                province.Resources_amount += 0.15f;
                province.RecruitablePopulation /= 2;
            }
        }
    }
}
