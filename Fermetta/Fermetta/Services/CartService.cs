using Fermetta.Data;
using Fermetta.Models;
using Microsoft.EntityFrameworkCore;

namespace Fermetta.Services
{
    /// <summary>
    /// Outcome of a cart operation: whether it succeeded, plus a message for the user.
    /// Keeps the service free of any HTTP/TempData concerns.
    /// </summary>
    public class CartResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;

        public static CartResult Ok(string message = "") => new() { Success = true, Message = message };
        public static CartResult Fail(string message) => new() { Success = false, Message = message };
    }

    public interface ICartService
    {
        Task<ShoppingCart> GetCartAsync(string userId);
        Task<CartResult> AddToCartAsync(string userId, int productId, int quantity);
        Task<CartResult> UpdateQuantityAsync(string userId, int productId, int quantity);
        Task<CartResult> RemoveFromCartAsync(string userId, int productId);
    }

    /// <summary>
    /// All shopping-cart business rules live here: stock validation, quantity rules,
    /// and per-account cart persistence. The controller only translates the result
    /// into a redirect and a TempData message.
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the user's persisted cart (with products loaded), or an empty,
        /// unsaved cart if the user has none yet.
        /// </summary>
        public async Task<ShoppingCart> GetCartAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart ?? new ShoppingCart { UserId = userId, CartItems = new List<CartItem>() };
        }

        public async Task<CartResult> AddToCartAsync(string userId, int productId, int quantity)
        {
            // A quantity below 1 is treated as 1 rather than rejected: the "add" button
            // on the product page posts a default of 1.
            if (quantity < 1) quantity = 1;

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return CartResult.Fail("Product was not found.");

            if (product.Stock <= 0)
                return CartResult.Fail($"Product '{product.Name}' is out of stock.");

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // The cart is created lazily, on the first add, and then persists per account.
            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            var currentQty = cartItem?.Quantity ?? 0;
            var newQty = currentQty + quantity;

            // Stock is checked against what is ALREADY in the cart, not just this request.
            if (newQty > product.Stock)
            {
                var remaining = product.Stock - currentQty;
                return CartResult.Fail(
                    $"Insufficient stock. You already have {currentQty} in cart. You can add max {remaining} more.");
            }

            if (cartItem != null)
            {
                cartItem.Quantity = newQty;
            }
            else
            {
                cartItem = new CartItem
                {
                    ShoppingCartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    Observations = ""
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return CartResult.Ok($"{quantity} x product added to cart!");
        }

        public async Task<CartResult> UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItem = cart?.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return CartResult.Fail("Product is not in the cart.");

            if (quantity < 1)
                return CartResult.Fail("Minimum amount is 1.");

            var stock = cartItem.Product?.Stock ?? 0;

            // Asking for more than the stock is not rejected outright: the quantity is
            // clamped down to the available stock and the user is told why.
            if (quantity > stock)
            {
                cartItem.Quantity = stock;
                _context.Update(cartItem);
                await _context.SaveChangesAsync();
                return CartResult.Fail($"Insufficient stock. Max amount: {stock}.");
            }

            cartItem.Quantity = quantity;
            _context.Update(cartItem);
            await _context.SaveChangesAsync();
            return CartResult.Ok("Cart updated!");
        }

        public async Task<CartResult> RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItem = cart?.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return CartResult.Fail("Product is not in the cart.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return CartResult.Ok("Product deleted from the cart.");
        }
    }
}
