using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Larram.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            Quantity = 1;
        }
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int ProductAvailabilityId { get; set; }
        [ForeignKey("ProductAvailabilityId")]
        public ProductAvailability ProductAvailability { get; set; }

        [Range(1, 100000, ErrorMessage = "Wybierz wartość z przedziały od 1 do 100000.")]
        public double Quantity { get; set; }

        [NotMapped]
        public double Price { get; set; }
    }
}
