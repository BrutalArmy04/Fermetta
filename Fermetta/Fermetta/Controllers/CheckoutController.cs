using Fermetta.Data;
using Fermetta.Models;
using Fermetta.Models.ViewModels;
using Fermetta.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fermetta.Controllers
{
    [Authorize(Roles = "User,Contribuitor")]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Checkout _checkoutService;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            Checkout checkoutService)
        {
            _context = context;
            _userManager = userManager;
            _checkoutService = checkoutService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            cart ??= new ShoppingCart { UserId = user.Id };

            var vm = new CheckoutInfo { Cart = cart };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutInfo vm)
        {
      
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Index");
            }


            if (!ModelState.IsValid)
            {
                vm.Cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id)
                    ?? new ShoppingCart { UserId = user.Id };

                return View("Index", vm);
            }

            try
            {
                var order = await _checkoutService.PlaceOrderAsync(
                    user.Id,
                    vm.DeliveryAddress,
                    vm.Observations,
                    vm.PaymentMethod
                );

                TempData["Message"] = "Order was successfully placed!";
                return RedirectToAction("Details", "Orders", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
