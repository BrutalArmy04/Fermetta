

using System.ComponentModel.DataAnnotations;

namespace Fermetta.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        
        [Required]
        public string Phone { get; set; } = null!;
        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public ApplicationUser User { get; set; } = null!;
    }
}
