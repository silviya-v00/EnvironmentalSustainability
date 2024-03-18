using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Utils
{
    public static class CommonUtil
    {
        public const string AdminRole = "ADMIN";
        public const string RegularUserRole = "REGULARUSER";

        private static Dictionary<string, string[]> ViewPermissions = new Dictionary<string, string[]>
        {
            // CarbonFootprint
            { "EnergyUsage", new string[] { RegularUserRole } },
            { "FoodAndDiet", new string[] { RegularUserRole } },
            { "Transportation", new string[] { RegularUserRole } },
            { "WasteManagement", new string[] { RegularUserRole } },
            // Home
            { "AdminHome", new string[] { AdminRole } },
            { "AirQuality", new string[] { RegularUserRole } },
            { "Index", new string[] { RegularUserRole } },
            { "Privacy", new string[] { AdminRole, RegularUserRole } },
            { "Profile", new string[] { AdminRole, RegularUserRole } }
        };

        private static List<string> LoginActions = new List<string>
        {
            "LoginPage",
            "Login",
            "Register",
            "ForgotPassword",
            "ForgotPasswordConfirmation",
            "ResetPassword",
            "ResetPasswordConfirmation"
        };

        public static bool IsUserAuthorizedForPage(string viewName, string userRole)
        {
            if (ViewPermissions.TryGetValue(viewName, out var allowedRoles))
            {
                return allowedRoles.Contains(userRole);
            }

            return true;
        }

        public static bool IsLoginAction(string viewName)
        {
            return LoginActions.Contains(viewName);
        }

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
