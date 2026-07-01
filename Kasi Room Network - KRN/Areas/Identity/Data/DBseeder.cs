using Kasi_Room_Network___KRN.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KasiRoomNetwork.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Kasi_Room_Network___KRN.Areas.Identity.Data
{
    public static class DBseeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var configuration = service.GetRequiredService<IConfiguration>();
            var email = configuration["SeedAdmin:Email"];
            var password = configuration["SeedAdmin:Password"];

            //seed roles
            var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            if(!await roleManager.RoleExistsAsync(Roles.Admin.ToString())) 
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            if(!await roleManager.RoleExistsAsync(Roles.Tenant.ToString())) 
                await roleManager.CreateAsync(new IdentityRole(Roles.Tenant.ToString()));
            if(!await roleManager.RoleExistsAsync(Roles.Landlord.ToString())) 
                await roleManager.CreateAsync(new IdentityRole(Roles.Landlord.ToString()));

            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("SeedAdmin:Email is missing");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("SeedAdmin:Password is missing");

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var userInDb = await userManager.FindByEmailAsync(user.Email);

            if (userInDb == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                }
                
            }
        }
    }
}
