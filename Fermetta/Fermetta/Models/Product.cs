using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; 


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
        public DateTime Validity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }
        public bool Personalised { get; set; }
        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Select a category.")]
        public int Category_Id { get; set; }
        [ForeignKey(nameof(Category_Id))]
        public Category? Category { get; set; }

        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        // partea cu imagini

        //  va stoca calea catre imagine in baza de date 
        public string? ImagePath { get; set; }

        //  nu ajunge in baza de date, este doar pentru a transporta fisierul din formular in controller
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

    }
}