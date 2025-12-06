using Microsoft.AspNetCore.Identity;

namespace Fermetta.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? AccountId { get; set; }
        public Account? Account { get; set; }

    }
}
