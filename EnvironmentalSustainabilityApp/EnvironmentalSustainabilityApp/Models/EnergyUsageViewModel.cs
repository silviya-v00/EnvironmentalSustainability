using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Models
{
    public class EnergyUsageViewModel
    {
        public int ElectricityConsumption { get; set; }
        public string HeatingType { get; set; }
        public int ApplianceUsage { get; set; }
        public string RenewableEnergy { get; set; }
        public int NumberOfPeople { get; set; }
    }
}
