using System;
using System.ComponentModel.DataAnnotations;
using Fermetta.Models.ChangeRequests;

namespace Fermetta.Models.ViewModels.ChangeRequests
{
    public class ChangeRequestInfo
    {
        [Required]
        public ChangeRequestType Type { get; set; }

        [Required]
        public ChangeAction RequestAction { get; set; }

        // pentru update
        public int? TargetCategoryId { get; set; }
        public int? TargetProductId { get; set; }

        // category 
        [StringLength(150)]
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public bool CategoryAvailability { get; set; } = true;

        // product 
        [StringLength(100)]
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        [Range(0, 100000, ErrorMessage = "Weight must be 0 or greater.")]
        public int? Weight { get; set; }
        public DateTime? Validity { get; set; }
        [Range(0.01, 100000, ErrorMessage = "Price must be strictly positive.")]
        public decimal? Price { get; set; }
        [Range(0, 100000, ErrorMessage = "Stock cannot be negative.")]
        public int? Stock { get; set; }
        public bool Personalised { get; set; }
        public int? Category_Id { get; set; }

        [StringLength(500)]
        public string? ContribuitorNote { get; set; }

        // dropdown data (pentru update + pentru select categoria produsului)
        public IEnumerable<(int Id, string Name)>? Categories { get; set; }
        public IEnumerable<(int Id, string Name)>? Products { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }
        public string? ProposedProductDescription { get; set; }
        public string? DraftProductDescription { get; set; }
    }
}
