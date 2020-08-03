using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Larram.Models
{
    public class ProductAvailability
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        public int SizeId { get; set; }
        [ForeignKey("SizeId")]
        public Size Size { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [Range(1, 100000, ErrorMessage = "Wybierz wartość z przedziały od 1 do 100000.")]
        public double Quantity { get; set; }
    }
}
