using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class MessagesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProfileRepository _profileRepository;
        public MessagesController(UserManager<ApplicationUser> userManager, IProfileRepository profileRepository)
        {
            _userManager = userManager;
            _profileRepository = profileRepository;
        }

        [Authorize(Roles = "Tenant")]
        public async Task<IActionResult> ContactLandlord(int listingId)
        {
            var userId = _userManager.GetUserId(User);

            var hasProfile = await _profileRepository.Exists(userId);

            if (!hasProfile)
            {
                return RedirectToAction(
                    "MyProfile",
                    "Profile",
                    new { returnUrl = Url.Action("ContactLandlord", new { listingId }) }
                );
            }

            // if profile exists continue
            return RedirectToAction("StartConversation", new { listingId });
        }
    }
}
