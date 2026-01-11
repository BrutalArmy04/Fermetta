using System;
using System.ComponentModel.DataAnnotations;

namespace Fermetta.Models.ChangeRequests
{
    public class ChangeRequest
    {
        [Key]
        public int ChangeRequest_Id { get; set; }

        [Required]
        public ChangeRequestType Type { get; set; }

        [Required]
        public ChangeAction Action { get; set; }

        [Required]
        public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;

        [Required]
        public string CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // pentru update
        public int? TargetCategoryId { get; set; }
        public int? TargetProductId { get; set; }

        // snapshot Contribuitor
        [Required]
        public string ProposedJson { get; set; }

        // draft admin
        public string? AdminDraftJson { get; set; }

        [StringLength(500)]
        public string? ContribuitorNote { get; set; }

        [StringLength(500)]
        public string? AdminNote { get; set; }

        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }

    }
}
