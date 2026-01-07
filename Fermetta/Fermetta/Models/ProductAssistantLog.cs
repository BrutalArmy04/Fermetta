using System;
using System.ComponentModel.DataAnnotations;

namespace Fermetta.Models
{
    public class ProductAssistantLog
    {
        [Key]
        public int ProductAssistantLog_Id { get; set; }

        [Required]
        public int Product_Id { get; set; }

        public string? UserId { get; set; } // null pentru vizitatori

        [Required, StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required, StringLength(1500)]
        public string Answer { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
