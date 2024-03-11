using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Models
{
    public class TransportationViewModel
    {
        public int KilometersDriven { get; set; }
        public int FlightsTaken { get; set; }
        public string FuelType { get; set; }
        public int NumberOfVehicles { get; set; }
        public bool UsesPublicTransport { get; set; }
    }
}
