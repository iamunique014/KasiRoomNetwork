using KasiRoomNetwork.Common.ViewModel.PostRoomWizard;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize(Roles = "Landlord")]
    public class PostRoomWizardController : Controller
    {
        private readonly IProfileRepository _profileRepository;

        public PostRoomWizardController(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(landlordUserId);
            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Before posting a room, complete your landlord profile first.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("Start", "PostRoomWizard") });
            }

            var now = DateTime.UtcNow;
            var wizardState = new PostRoomWizardStateViewModel
            {
                LandlordUserId = landlordUserId,
                StartedAtUtc = now,
                UpdatedAtUtc = now
            };

            SaveWizardState(landlordUserId, wizardState);

            return View();
        }

        [HttpPost]
        [ActionName(nameof(Start))]
        [ValidateAntiForgeryToken]
        public IActionResult StartPost()
        {
            return RedirectToAction(nameof(BasicPropertyInfo));
        }

        [HttpGet]
        public IActionResult BasicPropertyInfo()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(Start));
            }

            return View(wizardState.BasicPropertyInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BasicPropertyInfo(PostRoomBasicPropertyInfoStepViewModel model)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(Start));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            wizardState.BasicPropertyInfo = model;
            wizardState.UpdatedAtUtc = DateTime.UtcNow;

            SaveWizardState(landlordUserId, wizardState);

            return RedirectToAction(nameof(Address));
        }

        [HttpGet]
        public IActionResult Address()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(Start));
            }

            if (!HasCompletedBasicPropertyInfo(wizardState))
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
            }

            return View(wizardState.Address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Address(PostRoomAddressStepViewModel model)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(Start));
            }

            if (!HasCompletedBasicPropertyInfo(wizardState))
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            wizardState.Address = model;
            wizardState.UpdatedAtUtc = DateTime.UtcNow;

            SaveWizardState(landlordUserId, wizardState);

            return RedirectToAction(nameof(Amenities));
        }

        [HttpGet]
        public IActionResult Amenities()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(Start));
            }

            if (!HasCompletedBasicPropertyInfo(wizardState))
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
            }

            if (!HasCompletedAddress(wizardState))
            {
                return RedirectToAction(nameof(Address));
            }

            return View();
        }

        private PostRoomWizardStateViewModel? GetWizardState(string landlordUserId)
        {
            var sessionJson = HttpContext.Session.GetString(GetSessionKey(landlordUserId));
            if (string.IsNullOrWhiteSpace(sessionJson))
            {
                return null;
            }

            var wizardState = JsonSerializer.Deserialize<PostRoomWizardStateViewModel>(sessionJson);
            if (wizardState != null)
            {
                wizardState.BasicPropertyInfo ??= new PostRoomBasicPropertyInfoStepViewModel();
                wizardState.Address ??= new PostRoomAddressStepViewModel();
            }

            return wizardState;
        }

        private void SaveWizardState(string landlordUserId, PostRoomWizardStateViewModel wizardState)
        {
            var sessionJson = JsonSerializer.Serialize(wizardState);
            HttpContext.Session.SetString(GetSessionKey(landlordUserId), sessionJson);
        }

        private static bool HasCompletedBasicPropertyInfo(PostRoomWizardStateViewModel wizardState)
        {
            return !string.IsNullOrWhiteSpace(wizardState.BasicPropertyInfo.PropertyType);
        }

        private static bool HasCompletedAddress(PostRoomWizardStateViewModel wizardState)
        {
            return !string.IsNullOrWhiteSpace(wizardState.Address.Province)
                && !string.IsNullOrWhiteSpace(wizardState.Address.City)
                && !string.IsNullOrWhiteSpace(wizardState.Address.Suburb)
                && !string.IsNullOrWhiteSpace(wizardState.Address.Street);
        }

        private static string GetSessionKey(string landlordUserId)
        {
            return $"PostRoomWizard:{landlordUserId}";
        }
    }
}
