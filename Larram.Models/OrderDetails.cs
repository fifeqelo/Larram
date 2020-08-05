using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Larram.Models
{
    public class OrderDetails
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        public int ProductAvailabilityId { get; set; }
        [ForeignKey("ProductAvailabilityId")]
        public ProductAvailability ProductAvailability { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
    }
}
