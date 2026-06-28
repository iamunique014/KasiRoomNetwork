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
            var configuration = service.GetRequiredService<IConfinguration>();
            var email = configuration.["SeedAmin:Email"];
            var password =  configuration.["SeedAdmin:Password"];

            //seed roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Tenant.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Landlord.ToString()));

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
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }
        }
    }
}
