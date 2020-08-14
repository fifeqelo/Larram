using System;
using System.Collections.Generic;
using System.Text;

namespace Larram.Models.ViewModels
{
    public class ProductDisplayViewModel
    {
        public IEnumerable<Product> MenProducts { get; set; }
        public IEnumerable<Product> WomenProducts { get; set; }
    }
}
