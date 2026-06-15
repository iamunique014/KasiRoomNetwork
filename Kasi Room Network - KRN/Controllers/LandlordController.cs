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
        private readonly IProfileRepository _profileRepository;
        private readonly IMessagingRepository _messagingRepository;
        private readonly IPropertyRepository _propertyRepository;

        public LandlordController(
            UserManager<ApplicationUser> userManager,
            ILandlordRepository landlordRepository,
            IListingRepository listingRepository,
            IProfileRepository profileRepository,
            IMessagingRepository messagingRepository,
            IPropertyRepository propertyRepository)
        {
            _userManager = userManager;
            _landlordRepository = landlordRepository;
            _listingRepository = listingRepository;
            _profileRepository = profileRepository;
            _messagingRepository = messagingRepository;
            _propertyRepository = propertyRepository;
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
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> LandlordDashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(userId);
            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Complete your landlord profile first. It takes under a minute and unlocks room posting.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("LandlordDashboard", "Landlord") });
            }

            var profile = await _profileRepository.GetByUserId(userId);

            ViewBag.ShowWhatsAppPrompt =
                string.IsNullOrWhiteSpace(profile.LandlordProfile?.WhatsAppNumber);

            var properties = await _propertyRepository.GetPropertiesByUser(userId);

            ViewBag.HasProperties = properties.Count != 0;

            ViewBag.UnreadCount = await _messagingRepository.GetUnreadCount(userId);
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