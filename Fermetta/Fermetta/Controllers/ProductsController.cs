using Fermetta.Data;
using Fermetta.Models;
using Fermetta.Models.ViewModels; // aici trebuie sa fie ProductDetails
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fermetta.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Products
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Details/5 (cu review-uri)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Product_Id == id);

            if (product == null) return NotFound();

            var reviews = await _context.ProductReviews
                .Include(r => r.User)
                .Where(r => r.Product_Id == product.Product_Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var avg = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating);

            var userId = _userManager.GetUserId(User);
            bool userHasReviewed = false;

            if (!string.IsNullOrEmpty(userId))
                userHasReviewed = reviews.Any(r => r.UserId == userId);

            var vm = new ProductDetails
            {
                Product = product,
                Reviews = reviews,
                AverageRating = avg,
                ReviewsCount = reviews.Count,
                UserHasReviewed = userHasReviewed
            };

            return View(vm);
        }

        // POST: Products/AddReview
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            // validari simple
            if (rating < 1 || rating > 5)
                return RedirectToAction(nameof(Details), new { id = productId });

            if (string.IsNullOrWhiteSpace(comment))
                return RedirectToAction(nameof(Details), new { id = productId });

            if (comment.Length > 1000)
                comment = comment.Substring(0, 1000);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Details), new { id = productId });

            // daca exista deja => update (altfel intra pe constraint-ul unic)
            var existing = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Product_Id == productId && r.UserId == userId);

            if (existing != null)
            {
                existing.Rating = rating;
                existing.Comment = comment;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.ProductReviews.Add(new ProductReview
                {
                    Product_Id = productId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Category_Id"] = new SelectList(_context.Categories, "Category_Id", "Name");
            return View();
        }

        // POST: Products/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Product_Id,Name,Weight,Valability,Price,Stock,Personalised,Category_Id")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Category_Id"] = new SelectList(_context.Categories, "Category_Id", "Name", product.Category_Id);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["Category_Id"] = new SelectList(_context.Categories, "Category_Id", "Name", product.Category_Id);
            return View(product);
        }

        // POST: Products/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Product_Id,Name,Weight,Valability,Price,Stock,Personalised,Category_Id")] Product product)
        {
            if (id != product.Product_Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Product_Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Category_Id"] = new SelectList(_context.Categories, "Category_Id", "Name", product.Category_Id);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Product_Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Product_Id == id);
        }

        public IActionResult Catalog()
        {
            int _perPage = 6;

            var products = _context.Products
                .Include("Category")
                .OrderBy(p => p.Name);

            int totalItems = products.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedProducts = products.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.Products = paginatedProducts;

            ViewBag.PaginationBaseUrl = "/Products/Catalog/?page";

            return View();
        }
    }
}
