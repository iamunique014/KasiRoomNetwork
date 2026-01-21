using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize(Roles = "Landlord")]
    public class LandlordController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILandlordRepository _landlordRepository;

        public LandlordController(UserManager<ApplicationUser> userManager, ILandlordRepository landlordRepository)
        {
            _userManager = userManager;
            _landlordRepository = landlordRepository;
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

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> MyRooms() 
        {
            var landlordId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = await _landlordRepository.GetAllLandlordListings(landlordId);

            return View(model);
        }
    }
}