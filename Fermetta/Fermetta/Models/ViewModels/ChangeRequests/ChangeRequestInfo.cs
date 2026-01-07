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

        // pentru UPDATE
        public int? TargetCategoryId { get; set; }
        public int? TargetProductId { get; set; }

        // category 
        [StringLength(150)]
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public bool CategoryDisponibility { get; set; } = true;

        // product 
        [StringLength(100)]
        public string? ProductName { get; set; }
        public int? Weight { get; set; }
        public DateTime? Valability { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public bool Personalised { get; set; }
        public int? Category_Id { get; set; }

        [StringLength(500)]
        public string? ContributorNote { get; set; }

        // Dropdown data (pentru Update + pentru select categoria produsului)
        public IEnumerable<(int Id, string Name)>? Categories { get; set; }
        public IEnumerable<(int Id, string Name)>? Products { get; set; }
    }
}
