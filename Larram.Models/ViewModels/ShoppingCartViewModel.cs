using System;
using System.Collections.Generic;
using System.Text;

namespace Larram.Models.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        public Order Order { get; set; }
        public ProductAvailability ProductAvailability { get; set; }
    }
}
