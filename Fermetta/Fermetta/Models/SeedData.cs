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
                    // Obținem ID-urile categoriilor existente
                    var dairyId = context.Categories.First(c => c.Name == "Dairy").Category_Id;
                    var vegId = context.Categories.First(c => c.Name == "Vegetables").Category_Id;
                    var flowerId = context.Categories.First(c => c.Name == "Flowers").Category_Id;

                    context.Products.AddRange(
                        // --- DAIRY PRODUCTS (8 items) ---
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
                            Name = "Fresh Cow Milk",
                            Description = "Organic cow milk, pasteurized and bottled fresh.",
                            Weight = 1000,
                            Valability = DateTime.Now.AddDays(5),
                            Price = 8,
                            Stock = 50,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/milk.jpg"
                        },
                        new Product
                        {
                            Name = "Aged Sheep Cheese",
                            Description = "Traditional sheep cheese aged for 6 months.",
                            Weight = 500,
                            Valability = DateTime.Now.AddMonths(3),
                            Price = 45,
                            Stock = 20,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/sheep_cheese.jpg"
                        },
                        new Product
                        {
                            Name = "Homemade Butter",
                            Description = "Creamy butter made from fresh cream, salted.",
                            Weight = 200,
                            Valability = DateTime.Now.AddDays(20),
                            Price = 12,
                            Stock = 30,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/butter.jpg"
                        },
                        new Product
                        {
                            Name = "Sour Cream",
                            Description = "Thick and rich sour cream, perfect for soups.",
                            Weight = 300,
                            Valability = DateTime.Now.AddDays(10),
                            Price = 10,
                            Stock = 25,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/sour_cream.jpg"
                        },
                        new Product
                        {
                            Name = "Kefir Drink",
                            Description = "Probiotic-rich fermented milk drink.",
                            Weight = 500,
                            Valability = DateTime.Now.AddDays(14),
                            Price = 7,
                            Stock = 35,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/kefir.jpg"
                        },
                        new Product
                        {
                            Name = "Fresh Mozzarella",
                            Description = "Soft cheese balls in brine, ideal for caprese.",
                            Weight = 250,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 14,
                            Stock = 15,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/mozzarella.jpg"
                        },
                        new Product
                        {
                            Name = "Cheddar Block",
                            Description = "Sharp cheddar cheese, perfect for sandwiches.",
                            Weight = 300,
                            Valability = DateTime.Now.AddMonths(2),
                            Price = 22,
                            Stock = 40,
                            Category_Id = dairyId,
                            ImagePath = "/images/products/cheddar.jpg"
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
                            Name = "Organic Potatoes",
                            Description = "Freshly harvested potatoes, great for baking.",
                            Weight = 1000,
                            Valability = DateTime.Now.AddMonths(1),
                            Price = 5,
                            Stock = 200,
                            Category_Id = vegId,
                            ImagePath = "/images/products/potatoes.jpg"
                        },
                        new Product
                        {
                            Name = "Crunchy Carrots",
                            Description = "Sweet orange carrots, washed and ready to eat.",
                            Weight = 1000,
                            Valability = DateTime.Now.AddDays(14),
                            Price = 6,
                            Stock = 150,
                            Category_Id = vegId,
                            ImagePath = "/images/products/carrots.jpg"
                        },
                        new Product
                        {
                            Name = "Red Onions",
                            Description = "Mild red onions, perfect for raw salads.",
                            Weight = 1000,
                            Valability = DateTime.Now.AddMonths(2),
                            Price = 7,
                            Stock = 120,
                            Category_Id = vegId,
                            ImagePath = "/images/products/onions.jpg"
                        },
                        new Product
                        {
                            Name = "Green Cucumbers",
                            Description = "Crisp and refreshing farm cucumbers.",
                            Weight = 1000,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 8,
                            Stock = 80,
                            Category_Id = vegId,
                            ImagePath = "/images/products/cucumbers.jpg"
                        },
                        new Product
                        {
                            Name = "Bell Peppers Mix",
                            Description = "A colorful mix of red, yellow, and green peppers.",
                            Weight = 500,
                            Valability = DateTime.Now.AddDays(10),
                            Price = 12,
                            Stock = 60,
                            Category_Id = vegId,
                            ImagePath = "/images/products/peppers.jpg"
                        },
                        new Product
                        {
                            Name = "Fresh Spinach",
                            Description = "Young spinach leaves, washed and bagged.",
                            Weight = 200,
                            Valability = DateTime.Now.AddDays(4),
                            Price = 9,
                            Stock = 40,
                            Category_Id = vegId,
                            ImagePath = "/images/products/spinach.jpg"
                        },
                        new Product
                        {
                            Name = "Garlic Bulbs",
                            Description = "Aromatic local garlic.",
                            Weight = 100,
                            Valability = DateTime.Now.AddMonths(3),
                            Price = 4,
                            Stock = 90,
                            Category_Id = vegId,
                            ImagePath = "/images/products/garlic.jpg"
                        },
                        new Product
                        {
                            Name = "Zucchini",
                            Description = "Tender green zucchini squashes.",
                            Weight = 1000,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 8,
                            Stock = 50,
                            Category_Id = vegId,
                            ImagePath = "/images/products/zucchini.jpg"
                        },

                        new Product
                        {
                            Name = "Tulip Bouquet",
                            Description = "Seasonal tulip bouquet (10 stems).",
                            Weight = 500,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 55,
                            Stock = 25,
                            Personalised = true,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/tulips.jpg"
                        },
                        new Product
                        {
                            Name = "Red Rose Bouquet",
                            Description = "Classic red roses for special occasions (12 stems).",
                            Weight = 600,
                            Valability = DateTime.Now.AddDays(6),
                            Price = 80,
                            Stock = 20,
                            Personalised = true,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/roses.jpg"
                        },
                        new Product
                        {
                            Name = "Sunflower Bundle",
                            Description = "Bright and cheerful sunflowers (5 stems).",
                            Weight = 800,
                            Valability = DateTime.Now.AddDays(8),
                            Price = 40,
                            Stock = 30,
                            Personalised = false,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/sunflowers.jpg"
                        },
                        new Product
                        {
                            Name = "White Orchid Pot",
                            Description = "Elegant potted white orchid plant.",
                            Weight = 1200,
                            Valability = DateTime.Now.AddMonths(6),
                            Price = 65,
                            Stock = 15,
                            Personalised = false,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/orchid.jpg"
                        },
                        new Product
                        {
                            Name = "Lavender Bunch",
                            Description = "Dried aromatic lavender bunch.",
                            Weight = 100,
                            Valability = DateTime.Now.AddYears(1),
                            Price = 25,
                            Stock = 100,
                            Personalised = false,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/lavender.jpg"
                        },
                        new Product
                        {
                            Name = "Peony Arrangement",
                            Description = "Luxurious pink peonies in a vase.",
                            Weight = 700,
                            Valability = DateTime.Now.AddDays(5),
                            Price = 90,
                            Stock = 10,
                            Personalised = true,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/peonies.jpg"
                        },
                        new Product
                        {
                            Name = "Wildflower Mix",
                            Description = "A rustic mix of seasonal wildflowers.",
                            Weight = 400,
                            Valability = DateTime.Now.AddDays(6),
                            Price = 35,
                            Stock = 40,
                            Personalised = false,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/wildflowers.jpg"
                        },
                        new Product
                        {
                            Name = "Daisy Basket",
                            Description = "A small basket filled with white daisies.",
                            Weight = 600,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 45,
                            Stock = 20,
                            Personalised = true,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/daisies.jpg"
                        },

                        new Product
                        {
                            Name = "Red Spider Lilies",
                            Description = "Striking flower known for its vibrant red blooms",
                            Weight = 600,
                            Valability = DateTime.Now.AddDays(7),
                            Price = 45,
                            Stock = 20,
                            Personalised = true,
                            Category_Id = flowerId,
                            ImagePath = "/images/products/lilies.jpg"
                        }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
}