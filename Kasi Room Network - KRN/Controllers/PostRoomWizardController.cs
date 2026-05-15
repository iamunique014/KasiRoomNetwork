using Kasi_Room_Network___KRN.Services;
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
        private const int MaxWizardPhotoCount = 10;
        private readonly IProfileRepository _profileRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IPhotoStorageService _photoStorageService;

        public PostRoomWizardController(
            IProfileRepository profileRepository,
            IAmenityRepository amenityRepository,
            IPhotoStorageService photoStorageService)
        {
            _profileRepository = profileRepository;
            _amenityRepository = amenityRepository;
            _photoStorageService = photoStorageService;
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

            // CLEAN OLD TEMP FILES
            _photoStorageService.DeleteLandlordTemporaryPhotos(
                landlordUserId);

            // CLEAR OLD SESSION STATE
            HttpContext.Session.Remove(
                GetSessionKey(landlordUserId));

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
        public async Task<IActionResult> Amenities()
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

            var amenities = await _amenityRepository.GetAllAmenities();
            var model = new PostRoomAmenitiesStepViewModel
            {
                Amenities = amenities.ToList(),
                SelectedAmenityIds = wizardState.SelectedAmenityIds.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Amenities(PostRoomAmenitiesStepViewModel model)
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

            wizardState.SelectedAmenityIds = (model.SelectedAmenityIds ?? new List<int>()).Distinct().ToList();
            wizardState.UpdatedAtUtc = DateTime.UtcNow;

            SaveWizardState(landlordUserId, wizardState);

            return RedirectToAction(nameof(Photos));
        }

        [HttpGet]
        public IActionResult Photos()
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

            return View(wizardState.UploadedPhotos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(IFormFile? photo)
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

            if (!HasCompletedAddress(wizardState))
            {
                return RedirectToAction(nameof(Address));
            }

            if (wizardState.UploadedPhotos.Count >= MaxWizardPhotoCount)
            {
                TempData["PhotoError"] = $"You can upload a maximum of {MaxWizardPhotoCount} photos.";
                return RedirectToAction(nameof(Photos));
            }

            try
            {
                var tempRelativePath = await _photoStorageService.SaveTemporaryPhotoAsync(photo, landlordUserId);
                wizardState.UploadedPhotos.Add(new PostRoomUploadedPhotoViewModel
                {
                    TempPhotoId = Guid.NewGuid().ToString(),
                    TempRelativePath = tempRelativePath,
                    OriginalFileName = Path.GetFileName(photo?.FileName ?? string.Empty),
                    UseForRoom = false
                });
                wizardState.UpdatedAtUtc = DateTime.UtcNow;

                SaveWizardState(landlordUserId, wizardState);
                TempData["PhotoSuccess"] = "Photo uploaded successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["PhotoError"] = ex.Message;
            }

            return RedirectToAction(nameof(Photos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePhoto(string tempPhotoId)
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

            var photo = wizardState.UploadedPhotos.FirstOrDefault(uploadedPhoto => uploadedPhoto.TempPhotoId == tempPhotoId);
            if (photo != null)
            {
                _photoStorageService.DeleteTemporaryPhoto(photo.TempRelativePath);
                wizardState.UploadedPhotos.Remove(photo);

                // CLEAN EMPTY TEMP FOLDER
                if (!wizardState.UploadedPhotos.Any())
                {
                    _photoStorageService.DeleteLandlordTemporaryPhotos(
                        landlordUserId);
                }

                wizardState.UpdatedAtUtc = DateTime.UtcNow;
                SaveWizardState(landlordUserId, wizardState);
                TempData["PhotoSuccess"] = "Photo removed.";
            }

            return RedirectToAction(nameof(Photos));
        }

        [HttpGet]
        public IActionResult RoomDetails()
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

            if (!wizardState.UploadedPhotos.Any())
            {
                TempData["PhotoError"] = "Upload at least one photo before continuing.";
                return RedirectToAction(nameof(Photos));
            }

            return View(wizardState.RoomDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RoomDetails(PostRoomDetailsStepViewModel model)
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

            if (!wizardState.UploadedPhotos.Any())
            {
                TempData["PhotoError"] = "Upload at least one photo before continuing.";
                return RedirectToAction(nameof(Photos));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            wizardState.RoomDetails = model;
            wizardState.UpdatedAtUtc = DateTime.UtcNow;

            SaveWizardState(landlordUserId, wizardState);

            return RedirectToAction(nameof(SelectRoomPhotos));
        }

        [HttpGet]
        public IActionResult SelectRoomPhotos()
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

            if (!wizardState.UploadedPhotos.Any())
            {
                TempData["PhotoError"] = "Upload at least one photo before continuing.";
                return RedirectToAction(nameof(Photos));
            }

            if (!HasCompletedRoomDetails(wizardState))
            {
                return RedirectToAction(nameof(RoomDetails));
            }

            return View(BuildSelectRoomPhotosViewModel(wizardState));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectRoomPhotos(SelectRoomPhotosStepViewModel model)
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

            if (!wizardState.UploadedPhotos.Any())
            {
                TempData["PhotoError"] = "Upload at least one photo before continuing.";
                return RedirectToAction(nameof(Photos));
            }

            if (!HasCompletedRoomDetails(wizardState))
            {
                return RedirectToAction(nameof(RoomDetails));
            }

            var selectedTempPhotoIds = (model.Photos ?? new List<RoomPhotoSelectionItemViewModel>())
                .Where(photo => photo.UseForRoom)
                .Select(photo => photo.TempPhotoId.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var hasSelectedUploadedPhoto = wizardState.UploadedPhotos
                .Any(uploadedPhoto => selectedTempPhotoIds.Contains(uploadedPhoto.TempPhotoId));

            if (!hasSelectedUploadedPhoto)
            {
                ModelState.AddModelError(string.Empty, "Select at least one room photo before continuing.");
                return View(BuildSelectRoomPhotosViewModel(wizardState, selectedTempPhotoIds));
            }

            foreach (var uploadedPhoto in wizardState.UploadedPhotos)
            {
                uploadedPhoto.UseForRoom = selectedTempPhotoIds.Contains(uploadedPhoto.TempPhotoId);
            }

            wizardState.UpdatedAtUtc = DateTime.UtcNow;
            SaveWizardState(landlordUserId, wizardState);

            return RedirectToAction("ReviewAndSubmit");
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
                wizardState.RoomDetails ??= new PostRoomDetailsStepViewModel();
                wizardState.SelectedAmenityIds ??= new List<int>();
                wizardState.UploadedPhotos ??= new List<PostRoomUploadedPhotoViewModel>();
            }

            return wizardState;
        }


        private static SelectRoomPhotosStepViewModel BuildSelectRoomPhotosViewModel(
            PostRoomWizardStateViewModel wizardState,
            HashSet<string>? selectedTempPhotoIds = null)
        {
            return new SelectRoomPhotosStepViewModel
            {
                Photos = wizardState.UploadedPhotos
                    .Select(photo => new RoomPhotoSelectionItemViewModel
                    {
                        TempPhotoId = Guid.TryParse(photo.TempPhotoId, out var tempPhotoId)
                            ? tempPhotoId
                            : Guid.Empty,
                        PhotoPath = photo.TempRelativePath,
                        OriginalFileName = photo.OriginalFileName,
                        UseForRoom = selectedTempPhotoIds?.Contains(photo.TempPhotoId) ?? photo.UseForRoom
                    })
                    .ToList()
            };
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

        private static bool HasCompletedRoomDetails(PostRoomWizardStateViewModel wizardState)
        {
            return !string.IsNullOrWhiteSpace(wizardState.RoomDetails.Title)
                && wizardState.RoomDetails.Price >= 0;
        }

        private static string GetSessionKey(string landlordUserId)
        {
            return $"PostRoomWizard:{landlordUserId}";
        }


        //Cleanup After Final Submit
        //        _photoStorageService.DeleteLandlordTemporaryPhotos(
        //    landlordUserId);

        //HttpContext.Session.Remove(
        //    GetSessionKey(landlordUserId));
    }
}
