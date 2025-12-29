using KasiRoomNetwork.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class LandlordController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LandlordController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> start()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Landlord"))
                {
                    return RedirectToAction("LandLordDashboard", "Landlord");
                }
                
            }

            return View();
        }

        public IActionResult LandlordDashboard()
        {
            return View();
        }
    }
}
