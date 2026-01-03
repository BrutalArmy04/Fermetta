using Fermetta.Models;
using Fermetta.Models.ChangeRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fermetta.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        /*
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Account)
                .WithOne(a => a.User)
                .HasForeignKey<ApplicationUser>(u => u.AccountId);
        }*/

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Relatia Category -> Product
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.Category_Id);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(8,2)");

            builder.Entity<CartItem>()
                .HasKey(ci => new { ci.ShoppingCartId, ci.ProductId });

            // Relatia CartItem -> ShoppingCart
            builder.Entity<CartItem>()
                .HasOne(ci => ci.ShoppingCart)
                .WithMany(sc => sc.CartItems)
                .HasForeignKey(ci => ci.ShoppingCartId);

            // Relatia CartItem -> Product
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany() 
                .HasForeignKey(ci => ci.ProductId);

           
            builder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductId });

            // Relatia OrderItem -> ORder
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Relatia OrderItem -> Product
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
            // Relatia WishlistItem -> User, Product
            builder.Entity<WishlistItem>()
                .HasIndex(w => new { w.UserId, w.Product_Id })
                .IsUnique();


        }

    }
}