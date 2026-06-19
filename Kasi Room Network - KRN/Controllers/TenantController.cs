using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class TenantController( 
        UserManager<ApplicationUser> userManager,
       IProfileRepository profileRepository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> TenantDashboard()
        {
            var userId = userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            ViewBag.ProfileComplete = await profileRepository.IsComplete(userId);

            return View();
        }
    }
}
