using Fermetta.Data;
using Fermetta.Models;
using Fermetta.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Fermetta.Tests;

/// <summary>
/// Unit tests for the shopping-cart business rules.
///
/// Each test runs against a fresh in-memory EF Core database, so tests are isolated
/// and need no SQL Server instance. The in-memory provider does not enforce relational
/// constraints, which is an accepted trade-off here: these tests target the cart's
/// business rules (stock validation, quantity rules, persistence), not the schema.
/// </summary>
public class CartServiceTests
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    private static DbContextOptions<ApplicationDbContext> NewDatabase() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private static Product SeedProduct(ApplicationDbContext context, int stock, decimal price = 10m, string name = "Milk")
    {
        var product = new Product
        {
            Name = name,
            Description = "Test product",
            Weight = 1000,
            Validity = DateTime.Today.AddDays(30),
            Price = price,
            Stock = stock,
            Category_Id = 1
        };

        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }

    // ---------- AddToCart ----------

    [Fact]
    public async Task AddToCart_NewProduct_AddsItemWithRequestedQuantity()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);

        var result = await service.AddToCartAsync(UserId, product.Product_Id, quantity: 3);

        Assert.True(result.Success);
        var cart = await service.GetCartAsync(UserId);
        var item = Assert.Single(cart.CartItems);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(product.Product_Id, item.ProductId);
    }

    [Fact]
    public async Task AddToCart_SameProductTwice_IncrementsQuantityInsteadOfDuplicating()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);

        await service.AddToCartAsync(UserId, product.Product_Id, quantity: 2);
        var result = await service.AddToCartAsync(UserId, product.Product_Id, quantity: 3);

        Assert.True(result.Success);
        var cart = await service.GetCartAsync(UserId);
        var item = Assert.Single(cart.CartItems); // one line, not two
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public async Task AddToCart_QuantityBelowOne_IsNormalisedToOne()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);

        var result = await service.AddToCartAsync(UserId, product.Product_Id, quantity: 0);

        Assert.True(result.Success);
        var cart = await service.GetCartAsync(UserId);
        Assert.Equal(1, Assert.Single(cart.CartItems).Quantity);
    }

    [Fact]
    public async Task AddToCart_OutOfStockProduct_FailsAndAddsNothing()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 0, name: "Butter");
        var service = new CartService(context);

        var result = await service.AddToCartAsync(UserId, product.Product_Id, quantity: 1);

        Assert.False(result.Success);
        Assert.Contains("out of stock", result.Message);
        var cart = await service.GetCartAsync(UserId);
        Assert.Empty(cart.CartItems);
    }

    [Fact]
    public async Task AddToCart_MoreThanStock_FailsAndAddsNothing()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 5);
        var service = new CartService(context);

        var result = await service.AddToCartAsync(UserId, product.Product_Id, quantity: 6);

        Assert.False(result.Success);
        Assert.Contains("Insufficient stock", result.Message);
        var cart = await service.GetCartAsync(UserId);
        Assert.Empty(cart.CartItems);
    }

    [Fact]
    public async Task AddToCart_StockIsCheckedAgainstQuantityAlreadyInCart()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 5);
        var service = new CartService(context);

        await service.AddToCartAsync(UserId, product.Product_Id, quantity: 4);

        // 4 in cart + 2 more = 6 > stock of 5 -> rejected, cart untouched
        var result = await service.AddToCartAsync(UserId, product.Product_Id, quantity: 2);

        Assert.False(result.Success);
        var cart = await service.GetCartAsync(UserId);
        Assert.Equal(4, Assert.Single(cart.CartItems).Quantity);
    }

    [Fact]
    public async Task AddToCart_UnknownProduct_Fails()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var service = new CartService(context);

        var result = await service.AddToCartAsync(UserId, productId: 999, quantity: 1);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    // ---------- UpdateQuantity ----------

    [Fact]
    public async Task UpdateQuantity_ValidQuantity_UpdatesItem()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);
        await service.AddToCartAsync(UserId, product.Product_Id, quantity: 2);

        var result = await service.UpdateQuantityAsync(UserId, product.Product_Id, quantity: 7);

        Assert.True(result.Success);
        var cart = await service.GetCartAsync(UserId);
        Assert.Equal(7, Assert.Single(cart.CartItems).Quantity);
    }

    [Fact]
    public async Task UpdateQuantity_BelowOne_FailsAndLeavesQuantityUnchanged()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);
        await service.AddToCartAsync(UserId, product.Product_Id, quantity: 3);

        var result = await service.UpdateQuantityAsync(UserId, product.Product_Id, quantity: 0);

        Assert.False(result.Success);
        Assert.Contains("Minimum amount", result.Message);
        var cart = await service.GetCartAsync(UserId);
        Assert.Equal(3, Assert.Single(cart.CartItems).Quantity);
    }

    [Fact]
    public async Task UpdateQuantity_AboveStock_ClampsQuantityToStock()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 5);
        var service = new CartService(context);
        await service.AddToCartAsync(UserId, product.Product_Id, quantity: 2);

        var result = await service.UpdateQuantityAsync(UserId, product.Product_Id, quantity: 50);

        Assert.False(result.Success);                    // the user is told why
        Assert.Contains("Insufficient stock", result.Message);
        var cart = await service.GetCartAsync(UserId);
        Assert.Equal(5, Assert.Single(cart.CartItems).Quantity); // clamped, not rejected
    }

    [Fact]
    public async Task UpdateQuantity_ProductNotInCart_Fails()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 5);
        var service = new CartService(context);

        var result = await service.UpdateQuantityAsync(UserId, product.Product_Id, quantity: 2);

        Assert.False(result.Success);
    }

    // ---------- RemoveFromCart ----------

    [Fact]
    public async Task RemoveFromCart_RemovesOnlyThatItem()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var milk = SeedProduct(context, stock: 10, name: "Milk");
        var butter = SeedProduct(context, stock: 10, name: "Butter");
        var service = new CartService(context);
        await service.AddToCartAsync(UserId, milk.Product_Id, quantity: 1);
        await service.AddToCartAsync(UserId, butter.Product_Id, quantity: 1);

        var result = await service.RemoveFromCartAsync(UserId, milk.Product_Id);

        Assert.True(result.Success);
        var cart = await service.GetCartAsync(UserId);
        Assert.Equal(butter.Product_Id, Assert.Single(cart.CartItems).ProductId);
    }

    [Fact]
    public async Task RemoveFromCart_ProductNotInCart_Fails()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);

        var result = await service.RemoveFromCartAsync(UserId, product.Product_Id);

        Assert.False(result.Success);
    }

    // ---------- totals & persistence ----------

    [Fact]
    public async Task TotalAmount_IsSumOfQuantityTimesPrice()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var milk = SeedProduct(context, stock: 10, price: 5.50m, name: "Milk");
        var butter = SeedProduct(context, stock: 10, price: 12.00m, name: "Butter");
        var service = new CartService(context);
        await service.AddToCartAsync(UserId, milk.Product_Id, quantity: 2);   // 11.00
        await service.AddToCartAsync(UserId, butter.Product_Id, quantity: 3); // 36.00

        var cart = await service.GetCartAsync(UserId);

        Assert.Equal(47.00m, cart.TotalAmount);
    }

    [Fact]
    public async Task Cart_PersistsForTheAccountAcrossSessions()
    {
        var options = NewDatabase(); // same database, two separate contexts = two "sessions"

        using (var context = new ApplicationDbContext(options))
        {
            var product = SeedProduct(context, stock: 10);
            var service = new CartService(context);
            await service.AddToCartAsync(UserId, product.Product_Id, quantity: 4);
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = new CartService(context);
            var cart = await service.GetCartAsync(UserId);

            Assert.Equal(4, Assert.Single(cart.CartItems).Quantity);
        }
    }

    [Fact]
    public async Task Cart_IsIsolatedPerUser()
    {
        var options = NewDatabase();
        using var context = new ApplicationDbContext(options);
        var product = SeedProduct(context, stock: 10);
        var service = new CartService(context);

        await service.AddToCartAsync(UserId, product.Product_Id, quantity: 2);

        var otherCart = await service.GetCartAsync(OtherUserId);
        Assert.Empty(otherCart.CartItems);
    }
}
