using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnvironmentalSustainabilityApp.Models
{
    public class FeaturedContent
    {
        public int ContentID { get; set; }
        public string ContentTitle { get; set; }
        public string ContentDescription { get; set; }
        public string ContentLink { get; set; }
        public string ContentImageFileName { get; set; }
        public bool IsContentActive { get; set; }

        public IFormFile ContentImage { get; set; }
    }
}
