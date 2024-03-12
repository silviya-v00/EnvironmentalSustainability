using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Models
{
    public class FoodAndDietViewModel
    {
        public int MeatConsumption { get; set; }
        public int VegetarianMeals { get; set; }
        public int VeganMeals { get; set; }
        public bool OrganicFood { get; set; }
    }
}
