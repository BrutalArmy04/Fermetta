using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class CartItem
    {

        [Required]
        public int ShoppingCartId { get; set; } // FK catre Cos

        [Required]
        public int ProductId { get; set; } // FK catre Produs

        [Required]
        [Range(1, 100, ErrorMessage = "Cantitatea trebuie să fie cel puțin 1.")]
        public int Quantity { get; set; }

        public string? Observations { get; set; } 

        // Proprietăți de navigare
        [ForeignKey("ShoppingCartId")]
        public virtual ShoppingCart? ShoppingCart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}