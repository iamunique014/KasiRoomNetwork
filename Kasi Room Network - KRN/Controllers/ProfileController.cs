using KasiRoomNetwork.Common.ViewModel.Profiles;
using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(IProfileRepository profileRepository, UserManager<ApplicationUser> userManager)
        {
            _profileRepository = profileRepository;
            _userManager = userManager;
        }
        public async Task<IActionResult> MyProfile()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await PopulateLandlordProfileAsync(userId);
            var profile = await _profileRepository.GetByUserId(userId);
            return View(profile ?? new ProfileViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(ProfileViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                var invalidUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(invalidUserId))
                {
                    await PopulateLandlordProfileAsync(invalidUserId);
                }

                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await _profileRepository.SaveProfile(model, userId);
            TempData["Success"] = "Profile saved successfully";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(MyProfile));
        }

        private async Task PopulateLandlordProfileAsync(string userId)
        {
            var isLandlord = User.IsInRole("Landlord");
            ViewBag.IsLandlord = isLandlord;

            if (isLandlord)
            {
                ViewBag.LandlordProfile = await _profileRepository.GetLandlordPublic(userId);
            }
        }
    }
}
