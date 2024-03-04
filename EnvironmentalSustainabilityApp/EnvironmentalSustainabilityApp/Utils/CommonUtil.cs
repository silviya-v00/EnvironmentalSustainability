using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Utils
{
    public static class CommonUtil
    {
        public const string EnergyUsage = "ENERGY_USAGE";
        public const string Transportation = "TRANSPORTATION";
        public const string FoodAndDiet = "FOOD_AND_DIET";
        public const string WasteManagement = "WASTE_MANAGEMENT";

        public static string GetPageName(string categoryKey)
        {
            switch (categoryKey)
            {
                case EnergyUsage:
                    return "EnergyUsage";
                case Transportation:
                    return "Transportation";
                case FoodAndDiet:
                    return "FoodAndDiet";
                case WasteManagement:
                    return "WasteManagement";
                default:
                    throw new ArgumentException("Invalid category key", nameof(categoryKey));
            }
        }
    }
}
