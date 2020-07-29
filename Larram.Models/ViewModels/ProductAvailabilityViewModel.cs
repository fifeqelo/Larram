using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Larram.Models.ViewModels
{
    public class ProductAvailabilityViewModel
    {
        ProductAvailability ProductAvailability { get; set; }
        public IEnumerable<SelectListItem> ColorList { get; set; }
        public IEnumerable<SelectListItem> SizeList { get; set; }
    }
}
