using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Larram.Models.ViewModels
{
    public class ProductAvailabilityViewModel
    {
        public ProductAvailability ProductAvailability { get; set; }
        public Product Product { get; set; }
        public IEnumerable<ProductAvailability> ProductAvailabilities { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<SelectListItem> SizeList { get; set; }
    }
}
