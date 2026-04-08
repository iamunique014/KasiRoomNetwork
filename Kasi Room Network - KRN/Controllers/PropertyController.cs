using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class PropertyController : Controller
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IProfileRepository _profileRepository;

        public PropertyController(IPropertyRepository propertyRepository, IProfileRepository profileRepository)
        {
            _propertyRepository = propertyRepository;
            _profileRepository = profileRepository;
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> CreateProperty()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(landlordUserId);
            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Before creating a property, complete your landlord profile first.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("CreateProperty", "Property") });
            }

            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(CreatePropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(landlordUserId);
            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Complete your profile first to continue creating your property.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("CreateProperty", "Property") });
            }

            await _propertyRepository.CreateProperty(model, landlordUserId);
            TempData["SuccessMessage"] = "Property created successfully. You can now add a room listing for it.";

            return RedirectToAction(nameof(MyProperties));
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public IActionResult MyProperties()
        {
            return View();
        }
    }
}
