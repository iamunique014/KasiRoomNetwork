using Kasi_Room_Network___KRN.Constants;
using KasiRoomNetwork.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Kasi_Room_Network___KRN.Areas.Identity.Data
{
    public static class DBseeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            //seed roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Tenant.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Landlord.ToString()));

            // === DEMO: Admin ===
            var admin = new ApplicationUser
            {
                UserName = "admin@demo.com",
                Email = "admin@demo.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow // Explicit here if needed
            };
            var adminInDb = await userManager.FindByEmailAsync(admin.Email);
            if (adminInDb == null)
            {
                await userManager.CreateAsync(admin, "Demo@123"); // Replace with strong demo password
                await userManager.AddToRoleAsync(admin, Roles.Admin.ToString());
            }

            // === DEMO: Tenant ===
            var tenant = new ApplicationUser
            {
                UserName = "tenant@demo.com",
                Email = "tenant@demo.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var tenantInDb = await userManager.FindByEmailAsync(tenant.Email);
            if (tenantInDb == null)
            {
                await userManager.CreateAsync(tenant, "Demo@123");
                await userManager.AddToRoleAsync(tenant, Roles.Tenant.ToString());
            }

            // === DEMO: Landlord ===
            var landlord = new ApplicationUser
            {
                UserName = "landlord@demo.com",
                Email = "landlord@demo.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var landlordInDb = await userManager.FindByEmailAsync(landlord.Email);
            if (landlordInDb == null)
            {
                await userManager.CreateAsync(landlord, "Demo@123");
                await userManager.AddToRoleAsync(landlord, Roles.Landlord.ToString());
            }


            var user = new ApplicationUser
            {
                UserName = "mradmin@gmail.com",
                Email = "mradmin@gmail.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var userInDb = await userManager.FindByEmailAsync(user.Email);

            if (userInDb == null)
            {
                await userManager.CreateAsync(user, "MrAdmin@123");
                await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }
        }
    }
}
