using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class WishlistItem
    {
        [Key]
        public int WishlistItem_Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int Product_Id { get; set; }

        [ForeignKey(nameof(Product_Id))]
        public Product? Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
