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
                // in kg CO2 per unit
                const double ElectricityEmissionsFactor = 0.309;
                const double GasEmissionsFactor = 0.21;
                const double OilEmissionsFactor = 0.28;
                const double WoodPelletEmissionsFactor = 0.04;
                const double HeatPumpEmissionsFactor = 0.59;
                const double SolarEmissionsFactor = 0;
                const double ElectricHeatingEmissionsFactor = 0.59;
                const double ApplianceEmissionsFactor = 0.1; // in kg CO2 per hour
                const double RenewableEnergyReductionFactor = 0.2; // 20% reduction in emissions
                const int MonthsInYear = 12;
                const int DaysInYear = 365;

                double electricityConsumptionPerMonth = model.ElectricityConsumption;
                string heatingType = model.HeatingType;
                double applianceUsagePerDay = model.ApplianceUsage;
                string renewableEnergy = model.RenewableEnergy;
                int numberOfPeople = model.NumberOfPeople;

                double electricityEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * ElectricityEmissionsFactor;

                double heatingFactor = 0;
                double heatingEmissionsPerYear = 0;
                switch (heatingType)
                {
                    case "Gas":
                        heatingFactor = GasEmissionsFactor;
                        break;
                    case "Oil":
                        heatingFactor = OilEmissionsFactor;
                        break;
                    case "WoodPellet":
                        heatingFactor = WoodPelletEmissionsFactor;
                        break;
                    case "HeatPump":
                        heatingFactor = HeatPumpEmissionsFactor;
                        break;
                    case "Solar":
                        heatingFactor = SolarEmissionsFactor;
                        break;
                    case "Electric":
                        heatingFactor = ElectricHeatingEmissionsFactor;
                        break;
                }

                heatingEmissionsPerYear = electricityConsumptionPerMonth * MonthsInYear * heatingFactor;

                double applianceEmissionsPerYear = applianceUsagePerDay * DaysInYear * ApplianceEmissionsFactor;

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

        public async Task<IActionResult> Transportation()
        {
            var currentUser = await GetApplicationUser();
            decimal? carbonFootprintResult = _dbUtil.GetCarbonFootprintByCategory(currentUser.Id, CommonUtil.Transportation);
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
        public async Task<IActionResult> CalculateTransportationFootprint(TransportationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // in kg CO2 per kilometer
                const double DrivingEmissionsPetrol = 0.54689; 
                const double DrivingEmissionsDiesel = 0.54689;
                const double DrivingEmissionsHybrid = 0.38544;
                const double DrivingEmissionsElectric = 0.177;
                const double DrivingEmissionsOther = 0.563;
                const double FlightEmissionsFactor = 1.206; // (per passenger kilometer)
                const int FlightAverageKm = 1500;
                const double PublicTransportEmissionsFactor = 0.338;
                const int PublicTransportAverageKmPerWeek = 30;
                const int WeeksInYear = 52;

                double kilometersDrivenPerWeek = model.KilometersDriven;
                int flightsTakenPerYear = model.FlightsTaken;
                string fuelType = model.FuelType;
                int numberOfVehicles = model.NumberOfVehicles;
                bool usesPublicTransport = model.UsesPublicTransport;

                double drivingEmissions = 0;
                double fuelEmissionsFactor = 0;
                switch (fuelType)
                {
                    case "Petrol":
                        fuelEmissionsFactor = DrivingEmissionsPetrol;
                        break;
                    case "Diesel":
                        fuelEmissionsFactor = DrivingEmissionsDiesel;
                        break;
                    case "Electric":
                        fuelEmissionsFactor = DrivingEmissionsElectric;
                        break;
                    case "Hybrid":
                        fuelEmissionsFactor = DrivingEmissionsHybrid;
                        break;
                    case "Other":
                        fuelEmissionsFactor = DrivingEmissionsOther;
                        break;
                }

                if (numberOfVehicles != 0)
                    drivingEmissions = (kilometersDrivenPerWeek * WeeksInYear * fuelEmissionsFactor) / numberOfVehicles;

                double flightEmissions = flightsTakenPerYear * FlightAverageKm * FlightEmissionsFactor;

                double publicTransportEmissions = 0;
                if (usesPublicTransport)
                {
                    publicTransportEmissions = PublicTransportAverageKmPerWeek * WeeksInYear * PublicTransportEmissionsFactor;
                }

                double totalEmissions = drivingEmissions + flightEmissions + publicTransportEmissions;

                var currentUser = await GetApplicationUser();
                _dbUtil.SaveCarbonFootprintByCategory(currentUser.Id, CommonUtil.Transportation, (decimal)totalEmissions);

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> FoodAndDiet()
        {
            var currentUser = await GetApplicationUser();
            decimal? carbonFootprintResult = _dbUtil.GetCarbonFootprintByCategory(currentUser.Id, CommonUtil.FoodAndDiet);
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
        public async Task<IActionResult> CalculateFoodAndDietFootprint(FoodAndDietViewModel model)
        {
            if (ModelState.IsValid)
            {
                const double MeatEmissions = 6.7;
                const double VegetarianEmissions = 4.0;
                const double VeganEmissions = 2.0;
                const double OrganicFoodEmissionsReduction = 0.5;
                const int WeeksInYear = 52;

                int meatConsumption = model.MeatConsumption;
                int vegetarianMeals = model.VegetarianMeals;
                int veganMeals = model.VeganMeals;
                bool organicFood = model.OrganicFood;

                double meatEmissionsTotal = meatConsumption * MeatEmissions;
                double vegetarianEmissionsTotal = vegetarianMeals * VegetarianEmissions;
                double veganEmissionsTotal = veganMeals * VeganEmissions;

                if (organicFood)
                {
                    meatEmissionsTotal -= OrganicFoodEmissionsReduction * meatConsumption;
                    vegetarianEmissionsTotal -= OrganicFoodEmissionsReduction * vegetarianMeals;
                    veganEmissionsTotal -= OrganicFoodEmissionsReduction * veganMeals;
                }

                double totalEmissions = (meatEmissionsTotal + vegetarianEmissionsTotal + veganEmissionsTotal) * WeeksInYear;

                var currentUser = await GetApplicationUser();
                _dbUtil.SaveCarbonFootprintByCategory(currentUser.Id, CommonUtil.FoodAndDiet, (decimal)totalEmissions);

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> WasteManagement()
        {
            var currentUser = await GetApplicationUser();
            decimal? carbonFootprintResult = _dbUtil.GetCarbonFootprintByCategory(currentUser.Id, CommonUtil.WasteManagement);
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
        public async Task<IActionResult> CalculateWasteManagementFootprint(WasteManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                // in kg CO2
                const double WasteEmissionFactor = 1.5;
                const double RecyclingEmissionFactor = 0.5;
                const double TransportEmissionFactor = 0.15;
                const int WeeksInYear = 52;

                int wasteProducedPerWeek = model.WasteProducedPerWeek;
                int percentageRecycled = model.PercentageRecycled;

                double wasteEmissions = wasteProducedPerWeek * WasteEmissionFactor;
                double transportEmissions = wasteProducedPerWeek * TransportEmissionFactor;

                double recyclingEmissions = 0;
                if (percentageRecycled != 0)
                {
                    wasteEmissions = ((100 - (double)percentageRecycled) / 100) * wasteProducedPerWeek * WasteEmissionFactor;
                    recyclingEmissions = ((double)percentageRecycled / 100) * wasteProducedPerWeek * RecyclingEmissionFactor;
                }

                double totalEmissions = (wasteEmissions + recyclingEmissions + transportEmissions) * WeeksInYear;

                var currentUser = await GetApplicationUser();
                _dbUtil.SaveCarbonFootprintByCategory(currentUser.Id, CommonUtil.WasteManagement, (decimal)totalEmissions);

                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
