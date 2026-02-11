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
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(userId);

            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Before contacting a landlord, complete your profile so they can trust who is reaching out.";
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
