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
                // ----------------------------
                // 1) ROLES + USERS (doar dacă NU există)
                // ----------------------------
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new IdentityRole
                        {
                            Id = "1",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new IdentityRole
                        {
                            Id = "2",
                            Name = "Editor",
                            NormalizedName = "EDITOR"
                        },
                        new IdentityRole
                        {
                            Id = "3",
                            Name = "User",
                            NormalizedName = "USER"
                        }
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

                    var editor = new ApplicationUser
                    {
                        Id = "editor-id",
                        UserName = "editor@test.com",
                        Email = "editor@test.com",
                        NormalizedUserName = "EDITOR@TEST.COM",
                        NormalizedEmail = "EDITOR@TEST.COM",
                        EmailConfirmed = true,
                        FirstName = "Content",
                        LastName = "Editor",
                        RegistrationDate = DateTime.Now,
                        Status = "Active",
                        LastAuthentiationDate = DateTime.Now,
                        AllRoles = Enumerable.Empty<SelectListItem>()
                    };
                    editor.PasswordHash = hasher.HashPassword(editor, "Editor1!");

                    context.Users.AddRange(admin, editor);

                    context.UserRoles.AddRange(
                        new IdentityUserRole<string> { UserId = admin.Id, RoleId = "1" },
                        new IdentityUserRole<string> { UserId = editor.Id, RoleId = "2" }
                    );

                    context.SaveChanges();
                }

                // ----------------------------
                // 2) CATEGORIES (doar dacă NU există)
                // ----------------------------
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category
                        {
                            Name = "Dairy",
                            Description = "Products made from fresh cow and goat milk.",
                            Disponibility = true
                        },
                        new Category
                        {
                            Name = "Vegetables",
                            Description = "Seasonal vegetables, naturally grown on the farm.",
                            Disponibility = true
                        },
                        new Category
                        {
                            Name = "Flowers",
                            Description = "Bouquets and potted seasonal flowers.",
                            Disponibility = true
                        }
                    );

                    context.SaveChanges();
                }

                // ----------------------------
                // 3) PRODUCTS (doar dacă NU există)
                // ----------------------------
                
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
                            Category_Id = dairyId
                        },
                        new Product
                        {
                            Name = "Cherry Tomatoes",
                            Description = "Sweet cherry tomatoes, perfect for salads.",
                            Weight = 500,
                            Valability = DateTime.Now.AddDays(5),
                            Price = 15,
                            Stock = 100,
                            Category_Id = vegId
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
                            Category_Id = flowerId
                        }
                    );
                
                    context.SaveChanges();
                }
                
            }
        }
    }
}
