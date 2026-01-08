using Fermetta.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fermetta.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // 1) Roles + Users
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                        new IdentityRole { Id = "2", Name = "Contribuitor", NormalizedName = "CONTRIBUITOR" },
                        new IdentityRole { Id = "3", Name = "User", NormalizedName = "USER" }
                    );

                    var hasher = new PasswordHasher<ApplicationUser>();

                    var admin = new ApplicationUser
                    {
                        Id = "admin-id",
                        UserName = "admin@test.com",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        NormalizedEmail = "ADMIN@TEST.COM",
                        EmailConfirmed = true,
                        FirstName = "System",
                        LastName = "Admin",
                        RegistrationDate = DateTime.Now,
                        Status = "Active",
                        LastAuthentiationDate = DateTime.Now,
                        AllRoles = Enumerable.Empty<SelectListItem>()
                    };
                    admin.PasswordHash = hasher.HashPassword(admin, "Admin1!");

                    var contrib = new ApplicationUser
                    {
                        Id = "contrib-id",
                        UserName = "contrib@test.com",
                        Email = "contrib@test.com",
                        NormalizedUserName = "CONTRIB@TEST.COM",
                        NormalizedEmail = "CONTRIB@TEST.COM",
                        EmailConfirmed = true,
                        FirstName = "Content",
                        LastName = "Contributor",
                        RegistrationDate = DateTime.Now,
                        Status = "Active",
                        LastAuthentiationDate = DateTime.Now,
                        AllRoles = Enumerable.Empty<SelectListItem>()
                    };
                    contrib.PasswordHash = hasher.HashPassword(contrib, "Contrib1!");

                    var user = new ApplicationUser
                    {
                        Id = "user-id",
                        UserName = "user@test.com",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        NormalizedEmail = "USER@TEST.COM",
                        EmailConfirmed = true,
                        FirstName = "Standard",
                        LastName = "User",
                        RegistrationDate = DateTime.Now,
                        Status = "Active",
                        LastAuthentiationDate = DateTime.Now,
                        AllRoles = Enumerable.Empty<SelectListItem>()
                    };
                    user.PasswordHash = hasher.HashPassword(user, "User1!");

                    context.Users.AddRange(admin, contrib, user);
                    context.UserRoles.AddRange(
                        new IdentityUserRole<string> { UserId = admin.Id, RoleId = "1" },
                        new IdentityUserRole<string> { UserId = contrib.Id, RoleId = "2" },
                        new IdentityUserRole<string> { UserId = user.Id, RoleId = "3" }
                    );
                    context.SaveChanges();
                }

                // 2) Categories
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category
                        {
                            Name = "Dairy",
                            Description = "Products made from fresh cow and goat milk.",
                            Disponibility = true,
                            ImagePath = "/images/categories/dairy.jpg"
                        },
                        new Category
                        {
                            Name = "Vegetables",
                            Description = "Seasonal vegetables, naturally grown on the farm.",
                            Disponibility = true,
                            ImagePath = "/images/categories/vegetables.jpg"
                        },
                        new Category
                        {
                            Name = "Flowers",
                            Description = "Bouquets and potted seasonal flowers.",
                            Disponibility = true,
                            ImagePath = "/images/categories/flowers.jpg"
                        }
                    );
                    context.SaveChanges();
                }
                // 3) Products
                if (!context.Products.Any())
                {
                    var dairyId = context.Categories.First(c => c.Name == "Dairy").Category_Id;
                    var vegId = context.Categories.First(c => c.Name == "Vegetables").Category_Id;
                    var flowerId = context.Categories.First(c => c.Name == "Flowers").Category_Id;

                    context.Products.AddRange(
                        new Product
                        {
                            Name = "Natural Goat Yogurt",
                            Description = "Fresh goat yogurt, naturally fermented.",
                            Weight = 350,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 9,
                            Stock = 40,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/yoghurt.jpg"
                        },
                        new Product
                        {
                            Name = "Cherry Tomatoes",
                            Description = "Sweet cherry tomatoes, perfect for salads.",
                            Weight = 500,
                            Valability = DateTime.Now.AddDays(5),
                            Price = 15,
                            Stock = 100,
                            Category_Id = vegId,
                            ImagePath = "/images/products/tomatoes.jpg"
                        },
                        new Product
                        {
                            Name = "Tulip Bouquet",
                            Description = "Seasonal tulip bouquet.",
                            Weight = 500,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 55,
                            Stock = 25,
                            Personalised = true,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/tulips.jpg"
                        }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
}