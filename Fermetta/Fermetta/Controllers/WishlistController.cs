using Fermetta.Data;
using Fermetta.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fermetta.Controllers
{
    [Authorize(Roles = "User,Contribuitor")]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var items = await _db.WishlistItems
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .Include(w => w.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // POST: /Wishlist/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id) // id = Product_Id
        {
            var userId = _userManager.GetUserId(User);

            var exists = await _db.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.Product_Id == id);

            if (!exists)
            {
                _db.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId!,
                    Product_Id = id,
                    CreatedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
                TempData["Message"] = "Added to wishlist.";
            }
            else
            {
                TempData["Message"] = "This product is already in your wishlist.";
            }

            // te întorci de unde ai venit
            if (Request.Headers.TryGetValue("Referer", out var referer) && !string.IsNullOrWhiteSpace(referer))
                return Redirect(referer.ToString());

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id) // id = Product_Id
        {
            var userId = _userManager.GetUserId(User);

            var item = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Product_Id == id);

            if (item != null)
            {
                _db.WishlistItems.Remove(item);
                await _db.SaveChangesAsync();
                TempData["Message"] = "Removed from wishlist.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = _userManager.GetUserId(User);

            var items = await _db.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (items.Any())
            {
                _db.WishlistItems.RemoveRange(items);
                await _db.SaveChangesAsync();
            }

            TempData["Message"] = "Wishlist cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}
