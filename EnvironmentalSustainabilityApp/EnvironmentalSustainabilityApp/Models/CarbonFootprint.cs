using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Models
{
    public class CarbonFootprintCategory
    {
        public int CarbonFootprintCategoryID { get; set; }
        public string CarbonFootprintCategoryKey { get; set; }
        public string CarbonFootprintCategoryName { get; set; }
    }

    public class CarbonFootprintUser
    {
        public int CarbonFootprintID { get; set; }
        public string UserID { get; set; }
        public int CarbonFootprintCategoryID { get; set; }
        public decimal CarbonFootprintResult { get; set; }
        public CarbonFootprintCategory CFCategory { get; set; }
    }

    public class CarbonFootprintTestResult
    {
        public int? CompletedCategoryCount { get; set; }
        public int? TotalCategoryCount { get; set; }
        public int? CarbonFootprintCategoryID { get; set; }
        public string CarbonFootprintCategoryKey { get; set; }
        public string CarbonFootprintCategoryName { get; set; }
        public decimal? CarbonFootprintResult { get; set; }
    }
}
