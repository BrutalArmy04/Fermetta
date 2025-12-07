using Fermetta.Data;
using Fermetta.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fermetta.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var users = db.Users.OrderBy(u => u.UserName);

            ViewBag.UsersList = users;

            return View();
        }
        public async Task<ActionResult> ShowAsync(string id)
        {
            ApplicationUser? user = db.Users.Find(id);

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(user);

                ViewBag.Roles = roles;

                ViewBag.UserCurent = await _userManager.GetUserAsync(User);

                return View(user);
            }
        }
        // in identity este implementat user - roles many to many
        // este asa pt a fi mai permisiv
        // tinem noi ca logica in app daca vr one to many dar cand vb despre ea va fi many to many
    }
}
