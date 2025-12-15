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
        public static void Initialize(IServiceProvider
        serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul
                // De aceea facem return pentru a nu insera rolurile inca o data
                // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }

                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(

                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                },

                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    Name = "Contributor",
                    NormalizedName = "Contributor".ToUpper()
                },

                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                    Name = "User",
                    NormalizedName = "User".ToUpper()
                }

                );

                // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();

                // Admin
                var adminUser = new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    // primary key
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    FirstName = "System",
                    LastName = "Admin",
                    RegistrationDate = DateTime.Now,
                    Status = "Active",
                    LastAuthentiationDate = DateTime.Now,
                    AllRoles = Enumerable.Empty<SelectListItem>()
                };
                adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin1!");

                // Contributor
                var contribUser = new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    // primary key
                    UserName = "contrib@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "CONTRIB@TEST.COM",
                    Email = "contrib@test.com",
                    NormalizedUserName = "CONTRIB@TEST.COM",
                    FirstName = "Content",
                    LastName = "Contributor",
                    RegistrationDate = DateTime.Now,
                    Status = "Active",
                    LastAuthentiationDate = DateTime.Now,
                    AllRoles = Enumerable.Empty<SelectListItem>()
                };
                contribUser.PasswordHash = hasher.HashPassword(contribUser, "Contrib1!");

                // User
                var normalUser = new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                    // primary key
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    FirstName = "Standard",
                    LastName = "User",
                    RegistrationDate = DateTime.Now,
                    Status = "Active",
                    LastAuthentiationDate = DateTime.Now,
                    AllRoles = Enumerable.Empty<SelectListItem>()
                };
                normalUser.PasswordHash = hasher.HashPassword(normalUser, "User1!");

                context.Users.AddRange(adminUser, contribUser, normalUser);

                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(

                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },

                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },

                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                }
                );

                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Dairy", Description = "Products made from fresh cow and goat milk.", Disponibility = true },
                        new Category { Name = "Vegetables", Description = "Seasonal vegetables, naturally grown on the farm.", Disponibility = true },
                        new Category { Name = "Flowers", Description = "Bouquets and potted seasonal flowers.", Disponibility = true }
                    );

                    context.SaveChanges(); // Save categories to get Category_Id values
                }


                if (!context.Products.Any())
                {
                    var dairyId = context.Categories.First(c => c.Name == "Dairy").Category_Id;
                    var vegetablesId = context.Categories.First(c => c.Name == "Vegetables").Category_Id;
                    var flowersId = context.Categories.First(c => c.Name == "Flowers").Category_Id;


                    context.Products.AddRange(
                        // Dairy
                        new Product { Name = "Natural Goat Yogurt", Weight = 350, Valability = DateTime.Now.AddDays(7), Price = 9.00M, Stock = 40, Personalised = false, Category_Id = dairyId },
                        new Product { Name = "Homemade Cheese (Cașcaval)", Weight = 500, Valability = DateTime.Now.AddDays(45), Price = 32.00M, Stock = 15, Personalised = false, Category_Id = dairyId },

                        // Vegetables
                        new Product { Name = "Cherry Tomatoes", Weight = 500, Valability = DateTime.Now.AddDays(5), Price = 15.00M, Stock = 100, Personalised = false, Category_Id = vegetablesId },
                        new Product { Name = "Zucchini", Weight = 300, Valability = DateTime.Now.AddDays(8), Price = 6.00M, Stock = 80, Personalised = false, Category_Id = vegetablesId },

                        // Flowers
                        new Product { Name = "Tulip Bouquet", Weight = 500, Valability = DateTime.Now.AddDays(7), Price = 55.00M, Stock = 25, Personalised = true, Category_Id = flowersId },
                        new Product { Name = "Potted Lavender", Weight = 1500, Valability = DateTime.Now.AddMonths(12), Price = 35.00M, Stock = 15, Personalised = false, Category_Id = flowersId }
                    );
                }

                context.SaveChanges();
            }
        }
    }
}