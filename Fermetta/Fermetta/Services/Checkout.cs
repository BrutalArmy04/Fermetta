using Fermetta.Data;
using Fermetta.Models;
using Microsoft.EntityFrameworkCore;


namespace Fermetta.Services
{

    public class Checkout
    {
        private readonly ApplicationDbContext _context;

        public Checkout(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> PlaceOrderAsync(
            string userId,
            string address,
            string? observations,
            PaymentMethod paymentMethod)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new InvalidOperationException("Coșul nu există.");

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.ShoppingCartId == cart.Id)
                .ToListAsync();

            if (!cartItems.Any())
                throw new InvalidOperationException("Coșul este gol.");

            if (string.IsNullOrWhiteSpace(address))
                throw new InvalidOperationException("Adresa este obligatorie.");

            var order = new Order
            {
                UserId = userId,
                DeliveryAddress = address.Trim(),
                Observations = observations?.Trim(),
                PaymentMethod = paymentMethod,
                Status = OrderStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var ci in cartItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Product_Id == ci.ProductId)
                    ?? throw new InvalidOperationException("Produs inexistent.");

                if (ci.Quantity > product.Stock)
                    throw new InvalidOperationException($"Stoc insuficient pentru {product.Name}");

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Product_Id,
                    Quantity = ci.Quantity,
                    UnitPrice = product.Price
                });

                product.Stock -= ci.Quantity;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
    }
}
