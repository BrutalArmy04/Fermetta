using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace Fermetta.Models.ViewModels
{
    public class CheckoutInfo
    {
        [ValidateNever]
        public ShoppingCart? Cart { get; set; }


        [Required, StringLength(250)]
        public string DeliveryAddress { get; set; } = "";

        [StringLength(500)]
        public string? Observations { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}
