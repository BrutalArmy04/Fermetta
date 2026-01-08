using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fermetta.Data;
using Fermetta.Models;

namespace Fermetta.Controllers
{
    [Authorize(Roles = "User,Contributor")]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Show
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = user.Id, CartItems = new List<CartItem>() };
            }

            return View(cart);
        }

        // Add
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound("Product was not found.");

            if (product.Stock <= 0)
            {
                TempData["Error"] = $"Product '{product.Name}' is out of stock.";
                return RedirectToAction("Catalog", "Products");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = user.Id };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            int currentQty = cartItem?.Quantity ?? 0;
            int newQty = currentQty + 1;

            if (newQty > product.Stock)
            {
                TempData["Error"] = $"Insufficient Stock. Max amount: {product.Stock}.";
                return RedirectToAction("Catalog", "Products");
            }

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new CartItem
                {
                    ShoppingCartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1,
                    Observations = ""
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Product added to cart!";

            return RedirectToAction("Catalog", "Products");
        }

        // Edit
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem != null)
                {
                    if (quantity < 1)
                    {
                        TempData["Error"] = "Minimum amount is 1.";
                    }
                    else if (quantity > cartItem.Product.Stock)
                    {
                        TempData["Error"] = $"Insufficient stock. Max amount: {cartItem.Product.Stock}.";
                        cartItem.Quantity = cartItem.Product.Stock;
                        _context.Update(cartItem);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        cartItem.Quantity = quantity;
                        _context.Update(cartItem);
                        await _context.SaveChangesAsync();
                        TempData["Message"] = "Cart Updated!";
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Delete
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart != null)
            {
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Message"] = "Product deleted from the cart.";
            return RedirectToAction(nameof(Index));
        }
    }
}