using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

       [NotMapped]
        public decimal TotalAmount
        {
            get
            {
                if (CartItems == null) return 0;
                return CartItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));
            }
        }
    }
}