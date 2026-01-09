using Fermetta.Data;
using Fermetta.Models;
using Fermetta.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Fermetta.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Fermetta.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products (Admin List)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
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
        public async Task<IActionResult> Create([Bind("Product_Id,Name,Description,Weight,Valability,Price,Stock,Personalised,Category_Id,ImageFile,ImagePath")] Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImagePath = "/images/products/" + uniqueFileName;
                }

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
        public async Task<IActionResult> Edit(int id, [Bind("Product_Id,Name,Description,Weight,Valability,Price,Stock,Personalised,Category_Id,ImageFile,ImagePath")] Product product)
        {
            if (id != product.Product_Id) return NotFound();

            var existingProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Product_Id == id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (product.ImageFile != null)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(fileStream);
                        }
                        product.ImagePath = "/images/products/" + uniqueFileName;
                    }
                    else
                    {
                        if (existingProduct != null)
                        {
                            product.ImagePath = existingProduct.ImagePath;
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Product_Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (existingProduct != null && string.IsNullOrEmpty(product.ImagePath))
            {
                product.ImagePath = existingProduct.ImagePath;
                ModelState.Remove("ImagePath");
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

        // Filtered Catalog View
        public async Task<IActionResult> Catalog(string searchString, string category, int? categoryId, string sortOrder, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            if (string.IsNullOrEmpty(category) && categoryId.HasValue)
            {
                var cat = await _context.Categories.FindAsync(categoryId.Value);
                if (cat != null) category = cat.Name;
            }
            ViewData["CurrentCategory"] = category;

            ViewBag.Categories = await _context.Categories.Select(c => c.Name).Distinct().ToListAsync();

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews) 
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Name == category);
            }
            else if (categoryId.HasValue)
            {
                products = products.Where(p => p.Category_Id == categoryId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "rating_asc":
                    products = products.OrderBy(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0);
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0);
                    break;
                default:
                    products = products.OrderBy(p => p.Name); 
                    break;
            }

            int _perPage = 6;
            int totalItems = await products.CountAsync();
            int currentPage = page ?? 1;

            var paginatedProducts = await products
                .Skip((currentPage - 1) * _perPage)
                .Take(_perPage)
                .ToListAsync();

            ViewBag.lastPage = (int)Math.Ceiling((double)totalItems / _perPage);
            ViewBag.CurrentPage = currentPage; 
            ViewBag.Products = paginatedProducts;
            ViewBag.CategoryId = categoryId; 

            return View();
        }

        // AI ASSISTANT
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskAssistant(int productId, string question, [FromServices] IProductAssistantService assistant)
        {
            const string UnknownAnswer = "At the moment we don't have details about this.";
            question = (question ?? "").Trim();
            if (question.Length < 2) return Json(new { ok = true, answer = "Please write a longer question." });

            var exists = await _context.Products.AnyAsync(p => p.Product_Id == productId);
            if (!exists) return Json(new { ok = false, answer = UnknownAnswer });

            var answer = await assistant.AskAsync(productId, question);
            if (string.IsNullOrWhiteSpace(answer)) answer = UnknownAnswer;

            return Json(new { ok = true, answer });
        }

        // POST: Products/AddReview
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating invalid.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Comentariul este obligatoriu.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Product_Id == productId && r.UserId == user.Id);

            if (review == null)
            {
                review = new ProductReview
                {
                    Product_Id = productId,
                    UserId = user.Id,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ProductReviews.Add(review);
                TempData["Message"] = "Review Added!";
            }
            else
            {
                review.Rating = rating;
                review.Comment = comment;
                review.CreatedAt = DateTime.UtcNow;
                _context.Update(review);
                TempData["Message"] = "Review Updated!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // POST: Products/DeleteReview
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Product_Id == productId && r.UserId == user.Id);

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (review == null && isAdmin)
            {
                // Admin logic simplified
            }

            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Review Deleted.";
            }
            else
            {
                TempData["Error"] = "Review could not be deleted.";
            }

            return RedirectToAction(nameof(Details), new { id = productId });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Product_Id == id);
        }

        [AllowAnonymous]
        public IActionResult RequireLogin(int productId, string intent)
        {
            TempData["Message"] = "To continue, please sign up or log in.";
            string resumeUrl = Url.Action("ResumeIntent", "Products", new { productId = productId, intent = intent });
            return RedirectToPage("/Account/Login", new { area = "Identity", ReturnUrl = resumeUrl });
        }

        [Authorize]
        public async Task<IActionResult> ResumeIntent(int productId, string intent)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(nameof(Index));
            }
            if (intent == "cart")
            {
                var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (cart == null)
                {
                    cart = new ShoppingCart { UserId = user.Id };
                    _context.Add(cart);
                    await _context.SaveChangesAsync();
                }
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.ShoppingCartId == cart.Id && ci.ProductId == productId);

                if (cartItem == null)
                {
                    _context.CartItems.Add(new CartItem { ShoppingCartId = cart.Id, ProductId = productId, Quantity = 1 });
                }
                else
                {
                    cartItem.Quantity++;
                    _context.Update(cartItem);
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Product added to Cart (after you have logged in)!";
            }
            else if (intent == "wishlist")
            {
                var exists = await _context.WishlistItems
                    .AnyAsync(w => w.UserId == user.Id && w.Product_Id == productId);

                if (!exists)
                {
                    _context.WishlistItems.Add(new WishlistItem
                    {
                        UserId = user.Id,
                        Product_Id = productId,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Product saved in Wishlist (after you have logged in)!";
                }
                else
                {
                    TempData["Message"] = "Product was already in the wishlist.";
                }
            }

            return RedirectToAction(nameof(Catalog));
        }
    }
}