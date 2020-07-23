using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Larram.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane.")]
        [DisplayName("Category Name")]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
