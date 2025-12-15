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

                context.SaveChanges();
            }
        }
    }
}