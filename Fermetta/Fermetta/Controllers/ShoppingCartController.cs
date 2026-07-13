using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Fermetta.Models;
using Fermetta.Services;

namespace Fermetta.Controllers
{
    [Authorize(Roles = "User,Contribuitor")]
    public class ShoppingCartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        // Show
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _cartService.GetCartAsync(user.Id);
            return View(cart);
        }

        // Add
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _cartService.AddToCartAsync(user.Id, productId, quantity);

            if (result.Message == "Product was not found.")
                return NotFound(result.Message);

            SetFeedback(result);
            return Back(returnUrl);
        }

        // Edit
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _cartService.UpdateQuantityAsync(user.Id, productId, quantity);
            SetFeedback(result);

            return RedirectToAction(nameof(Index));
        }

        // Delete
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _cartService.RemoveFromCartAsync(user.Id, productId);
            SetFeedback(result);

            return RedirectToAction(nameof(Index));
        }

        // -- helpers: the only things the controller is still responsible for --

        private void SetFeedback(CartResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Message)) return;

            if (result.Success)
                TempData["Message"] = result.Message;
            else
                TempData["Error"] = result.Message;
        }

        private IActionResult Back(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Catalog", "Products");
        }
    }
}
