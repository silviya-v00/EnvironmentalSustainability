using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Models
{
    public class CarbonFootprintTestState
    {
        public int? CompletedCategoryCount { get; set; }
        public int? TotalCategoryCount { get; set; }
        public int? CarbonFootprintCategoryID { get; set; }
        public string CarbonFootprintCategoryKey { get; set; }
        public string CarbonFootprintCategoryName { get; set; }
    }

    public class CarbonFootprintChartData
    {
        public string CategoryName { get; set; }
        public decimal? UserResult { get; set; }
        public decimal? TotalAvgResult { get; set; }
        public int? Seq { get; set; }
    }
}
