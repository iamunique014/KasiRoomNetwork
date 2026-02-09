using KasiRoomNetwork.Common.ViewModel.Profiles;
using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
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
            var profile = await _profileRepository.GetByUserId(userId);
            return View(profile ?? new ProfileViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(ProfileViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            await _profileRepository.SaveProfile(model, userId);
            TempData["Success"] = "Profile updated";
            return RedirectToAction(nameof(MyProfile));
        }
    }
}
