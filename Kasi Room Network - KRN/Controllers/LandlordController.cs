using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kasi_Room_Network___KRN.Controllers
{
    //Authorize(Roles = "Landlord")]
    public class LandlordController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILandlordRepository _landlordRepository;
        private readonly IListingRepository _listingRepository;

        public LandlordController(UserManager<ApplicationUser> userManager, ILandlordRepository landlordRepository, IListingRepository listingRepository)
        {
            _userManager = userManager;
            _landlordRepository = landlordRepository;
            _listingRepository = listingRepository;
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

        // =========================
        // LISTING DETAILS
        // =========================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ReviewListingDetails(int id)
        {
            var listing = await _listingRepository.GetListingById(id);
            if (listing == null)
                return NotFound();

            listing.Photos = await _listingRepository.GetListingPhotos(id);

            return View(listing);
        }
    }
}