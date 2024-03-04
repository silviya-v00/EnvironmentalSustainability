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

        public IActionResult EnergyUsage()
        {
            return View();
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
