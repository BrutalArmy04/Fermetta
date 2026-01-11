using System.ComponentModel.DataAnnotations;

namespace Fermetta.Models.ViewModels
{
    public class ProductDetails
    {
        public Product Product { get; set; }

        public List<ProductReview> Reviews { get; set; } = new();

        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }

        // form input
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        public bool UserHasReviewed { get; set; }
    }
}
