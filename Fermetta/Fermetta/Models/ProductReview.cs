using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Product_Id { get; set; }

        [ForeignKey(nameof(Product_Id))]
        public Product? Product { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
