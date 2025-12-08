using Fermetta.Data;
using Fermetta.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            //ApplicationUser? user = db.Users.Find(id);
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return NotFound();
            }
            else
            {
                //var roles = await _userManager.GetRolesAsync(user);
                var allRolesListItems = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();

                user.AllRoles = allRolesListItems;

                var currentRoles = await _userManager.GetRolesAsync(user);
                ViewBag.Roles = currentRoles;

                var currentRoleName = currentRoles.FirstOrDefault();
                if (currentRoleName != null)
                {
                    var selectedRole = allRolesListItems.FirstOrDefault(r => r.Value == currentRoleName);
                    if (selectedRole != null)
                    {
                        selectedRole.Selected = true; 

                    }
                }

                //ViewBag.Roles = _roleManager.Roles.ToList();
                ViewBag.CurrentUserId = _userManager.GetUserId(User); // ca sa nu se poata modifica pe sine
                ViewBag.UserCurent = await _userManager.GetUserAsync(User);

                return View(user);
            }
        }
        // in identity este implementat user - roles many to many
        // este asa pt a fi mai permisiv
        // tinem noi ca logica in app daca vr one to many dar cand vb despre ea va fi many to many

        public async Task<ActionResult> ChangeRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var allRoles = _roleManager.Roles.ToList();

            user.AllRoles = allRoles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            });

            var currentRoles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = currentRoles.FirstOrDefault();

            if (ViewBag.CurrentRole != null)
            {
                var selectedRole = user.AllRoles.FirstOrDefault(r => r.Value == ViewBag.CurrentRole);
                if (selectedRole != null)
                {
                    selectedRole.Selected = true;
                }
            }

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeRole(string id, [FromForm] string newRole)
        {
            string? currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot modify your own role.";
                return RedirectToAction("ShowAsync", new { id = id });
            }


            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var oldRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, oldRoles);

            if (!removeResult.Succeeded)
            {
                TempData["Error"] = "Could not edit role.";
                return RedirectToAction("Show", new { id = id });
            }

            
            var addResult = await _userManager.AddToRoleAsync(user, newRole);

            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Could not change role.";
                return RedirectToAction("Show", new { id = id });
            }

            TempData["Message"] = $"{user.UserName}'s role is now {newRole}!";
            return RedirectToAction("Show", new { id = id });
        }
        


    }       

        
}
