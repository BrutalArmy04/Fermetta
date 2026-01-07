using System;
using System.ComponentModel.DataAnnotations;
using Fermetta.Models.ChangeRequests;

namespace Fermetta.Models.ViewModels.ChangeRequests
{
    public class ChangeRequestReview
    {
        public int Id { get; set; }
        public ChangeRequestType Type { get; set; }
        public ChangeAction RequestAction { get; set; }
        public ChangeRequestStatus Status { get; set; }

        public string? ContributorNote { get; set; }
        public string? AdminNote { get; set; }

        // PROPOSED 
        public string? ProposedCategoryName { get; set; }
        public string? ProposedCategoryDescription { get; set; }
        public bool ProposedCategoryDisponibility { get; set; }

        public string? ProposedProductName { get; set; }
        public int? ProposedWeight { get; set; }
        public DateTime? ProposedValability { get; set; }
        public decimal? ProposedPrice { get; set; }
        public int? ProposedStock { get; set; }
        public bool ProposedPersonalised { get; set; }
        public int? ProposedCategory_Id { get; set; }

        // DRAFT (editable)
        [StringLength(150)]
        public string? DraftCategoryName { get; set; }
        public string? DraftCategoryDescription { get; set; }
        public bool DraftCategoryDisponibility { get; set; }

        [StringLength(100)]
        public string? DraftProductName { get; set; }
        public int? DraftWeight { get; set; }
        public DateTime? DraftValability { get; set; }
        public decimal? DraftPrice { get; set; }
        public int? DraftStock { get; set; }
        public bool DraftPersonalised { get; set; }
        public int? DraftCategory_Id { get; set; }

        
        public IEnumerable<(int Id, string Name)>? Categories { get; set; }
    }
}
