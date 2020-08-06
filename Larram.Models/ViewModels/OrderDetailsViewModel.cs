using System;
using System.Collections.Generic;
using System.Text;

namespace Larram.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<OrderDetails> OrderDetails { get; set; }
        public IEnumerable<ProductAvailability> ProductAvailabilities { get; set; }
    }
}
