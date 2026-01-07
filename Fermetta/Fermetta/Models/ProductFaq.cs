using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class ProductFaq
    {
        [Key]
        public int ProductFaq_Id { get; set; }

        [Required]
        public int Product_Id { get; set; }

        [ForeignKey(nameof(Product_Id))]
        public Product? Product { get; set; }

        [Required, StringLength(200)]
        public string Question { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Answer { get; set; }

        public int AskedCount { get; set; } = 0;
        public DateTime LastAskedAt { get; set; } = DateTime.UtcNow;
    }
}
