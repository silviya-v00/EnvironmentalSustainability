using EnvironmentalSustainabilityApp.Models;
using EnvironmentalSustainabilityApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Controllers
{
    public class CarbonFootprintController : Controller
    {
        private readonly ILogger<CarbonFootprintController> _logger;
        private readonly IConfiguration _configuration;
        private DBUtil _dbUtil;
        private readonly UserManager<IdentityUser> _userManager;

        public CarbonFootprintController(ILogger<CarbonFootprintController> logger,
                                         IConfiguration configuration,
                                         UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _configuration = configuration;
            _dbUtil = new DBUtil(_configuration.GetConnectionString("DefaultConnection"));
            _userManager = userManager;
        }

        public async Task<IdentityUser> GetApplicationUser()
        {
            return await _userManager.GetUserAsync(User);
        }

        public async Task<IActionResult> EnergyUsage()
        {
            var currentUser = await GetApplicationUser();
            decimal? carbonFootprintResult = _dbUtil.GetCarbonFootprintByCategory(currentUser.Id, CommonUtil.EnergyUsage);
            bool hasResult = false;

            if (carbonFootprintResult.HasValue)
                hasResult = true;
            else
                carbonFootprintResult = 0;

            ViewBag.CarbonFootprintResult = carbonFootprintResult.ToString();
            ViewBag.HasCarbonFootprintResult = hasResult;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CalculateEnergyFootprint(EnergyUsageViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Constants for carbon emissions factors (in kg CO2 per unit)
                const double ElectricityEmissionsFactor = 0.4;
                const double GasEmissionsFactor = 0.184;
                const double OilEmissionsFactor = 0.278;
                const double WoodPelletEmissionsFactor = 0.01;
                const double HeatPumpEmissionsFactor = 0.05;
                const double SolarEmissionsFactor = 0;
                const double ElectricHeatingEmissionsFactor = 0.4;

                // Constants for appliance emissions factors (in kg CO2 per hour)
                const double ApplianceEmissionsFactor = 0.1;

                // Constants for renewable energy usage
                const double RenewableEnergyReductionFactor = 0.2; // 20% reduction in emissions

                // Constants for time periods
                const double MonthsInYear = 12;
                const double HoursInYear = 24 * 365;

                double electricityConsumptionPerMonth = model.ElectricityConsumption;
                string heatingType = model.HeatingType;
                double applianceUsagePerDay = model.ApplianceUsage;
                string renewableEnergy = model.RenewableEnergy;
                int numberOfPeople = model.NumberOfPeople;

                // electricity
                double electricityEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * ElectricityEmissionsFactor;

                // heating
                double heatingEmissionsPerYear = 0;
                switch (heatingType)
                {
                    case "Gas":
                        heatingEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * GasEmissionsFactor;
                        break;
                    case "Oil":
                        heatingEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * OilEmissionsFactor;
                        break;
                    case "WoodPellet":
                        heatingEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * WoodPelletEmissionsFactor;
                        break;
                    case "HeatPump":
                        heatingEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * HeatPumpEmissionsFactor;
                        break;
                    case "Solar":
                        heatingEmissionsPerYear = SolarEmissionsFactor;
                        break;
                    case "Electric":
                        heatingEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * ElectricHeatingEmissionsFactor;
                        break;
                }

                // appliance usage
                double applianceEmissionsPerYear = applianceUsagePerDay * HoursInYear * ApplianceEmissionsFactor;

                // renewable energy usage
                if (renewableEnergy == "Yes")
                {
                    electricityEmissionsPerYear *= (1 - RenewableEnergyReductionFactor);
                    heatingEmissionsPerYear *= (1 - RenewableEnergyReductionFactor);
                }

                double totalEmissionsPerYear = electricityEmissionsPerYear + heatingEmissionsPerYear + applianceEmissionsPerYear;
                double carbonFootprintPerPersonPerYear = totalEmissionsPerYear / numberOfPeople;

                var currentUser = await GetApplicationUser();
                _dbUtil.SaveCarbonFootprintByCategory(currentUser.Id, CommonUtil.EnergyUsage, (decimal)carbonFootprintPerPersonPerYear);

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public IActionResult Transportation()
        {
            return View();
        }

        public IActionResult FoodAndDiet()
        {
            return View();
        }

        public IActionResult WasteManagement()
        {
            return View();
        }
    }
}
