using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fermetta.Models
{
    public class ApplicationUser : IdentityUser
    {

        // Principalele proprietăți ale clasei IdentityUser sunt
        // – Id, UserName, Email, PasswordHash, etc.

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? PostalCode { get; set; }    
        public string? Country { get; set; }
        public DateTime LastAuthentiationDate { get; set; }
        public required IEnumerable<SelectListItem> AllRoles { get; set; }




    }
}
