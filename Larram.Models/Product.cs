using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Larram.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [DisplayName("Product Name")]
        [MaxLength(50, ErrorMessage = "Maksymalna długość wynosi 50 znaków.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [EnumDataType(typeof(Gender))]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        public int ColorId { get; set; }
        [ForeignKey("ColorId")]
        public Color Color { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [Range(1, 100000, ErrorMessage = "Wybierz wartość z przedziały od 1 do 100000.")]
        public double Price { get; set; }

        [Range(0, 100000, ErrorMessage = "Wybierz wartość z przedziały od 0 do 100000.")]
        public double DiscountPrice { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [MaxLength(50)]
        public string HexValue { get; set; }

        public string ImageUrl { get; set; }
    }
    public enum Gender
    {
        Mężczyzna,
        Kobieta,
        Unisex
    }
}
