using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Fermetta.Models
{
    public class Product
    {
        [Key]
        public int Product_Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int Weight {  get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Valability { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }
        public bool Personalised { get; set; }
        [Display(Name = "Categorie")]
        [Range(1, int.MaxValue, ErrorMessage = "Selectează o categorie.")]
        public int Category_Id { get; set; }
        [ForeignKey(nameof(Category_Id))]
        public Category? Category { get; set; }

        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

    }
}