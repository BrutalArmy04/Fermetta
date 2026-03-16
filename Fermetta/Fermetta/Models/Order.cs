using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.New;
        [Required, StringLength(250)]
        public string DeliveryAddress { get; set; } = null!;
        [StringLength(500)]
        public string? Observations { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [NotMapped]
        public decimal TotalAmount => OrderItems.Sum(i => i.Quantity * i.UnitPrice);
    }
}
