using Kasi_Room_Network___KRN.Constants;
using Kasi_Room_Network___KRN.Services;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Common.ViewModel.CreatePropertyWizard;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using KasiRoomNetwork.Common.DTOs;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize(Roles = "Landlord")]
    public class CreatePropertyController : Controller
    {
        private const int MaxWizardPhotoCount = 10;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly ILandlordRepository _landlordRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IPhotoStorageService _photoStorageService;
        private readonly ICreatePropertyService _createPropertyService;
        private readonly ILogger<PropertyController> _logger;

        public CreatePropertyController(IPropertyRepository propertyRepository,
            IProfileRepository profileRepository, 
            ILandlordRepository landlordRepository, 
            IAmenityRepository amenityRepository,
            IPhotoStorageService photoStorageService,
            ICreatePropertyService CreatePropertyService,
            ILogger<PropertyController> logger)
        {
            _propertyRepository = propertyRepository;
            _profileRepository = profileRepository;
            _landlordRepository = landlordRepository;
            _amenityRepository = amenityRepository;
            _photoStorageService = photoStorageService;
            _createPropertyService = CreatePropertyService;
            _logger = logger;
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
            var wizardState = new PropertyWizardStateViewModel
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
        public async Task<IActionResult> BasicPropertyInfo()
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
        public IActionResult BasicPropertyInfo(BasicPropertyInfoStepViewModel model)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
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
                return RedirectToAction(nameof(BasicPropertyInfo));
            }

            if (!HasCompletedBasicPropertyInfo(wizardState))
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
            }

            return View(wizardState.Address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Address(AddressStepViewModel model)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
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
                return RedirectToAction(nameof(BasicPropertyInfo));
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
            var model = new AmenitiesStepViewModel
            {
                Amenities = amenities.ToList(),
                SelectedAmenityIds = wizardState.SelectedAmenityIds.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Amenities(AmenitiesStepViewModel model)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            if (wizardState == null)
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
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
                return RedirectToAction(nameof(BasicPropertyInfo));
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
                return RedirectToAction(nameof(BasicPropertyInfo));
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
                wizardState.UploadedPhotos.Add(new UploadedPhotoViewModel
                {
                    TempPhotoId = Guid.NewGuid().ToString(),
                    TempRelativePath = tempRelativePath,
                    OriginalFileName = Path.GetFileName(photo?.FileName ?? string.Empty)
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
                return RedirectToAction(nameof(BasicPropertyInfo));
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
        public async Task<IActionResult> ReviewAndSubmit()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            var redirectResult = EnsureReadyForReview(wizardState);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return View(await BuildReviewStepViewModel(wizardState!));
        }

        [HttpPost]
        [ActionName(nameof(ReviewAndSubmit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewAndSubmitPost()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var wizardState = GetWizardState(landlordUserId);
            var redirectResult = EnsureReadyForReview(wizardState);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            try
            { 
                var dto = new CreatePropertyWizardDto
                {
                    LandlordUserId = landlordUserId,
                    PropertyType = wizardState.BasicPropertyInfo.PropertyType,
                    TotalRooms = wizardState.BasicPropertyInfo.TotalRooms,
                    PropertyName = wizardState.BasicPropertyInfo.PropertyName,
                    Street = wizardState.Address.Street,
                    Province = wizardState.Address.Province,
                    City = wizardState.Address.City,
                    Suburb = wizardState.Address.Suburb,
                    AmenityIds = wizardState.SelectedAmenityIds.Distinct().ToList(),
                    TemporaryPhotoPaths = GetUniqueUploadedPhotos(wizardState.UploadedPhotos).Select(p => p.TempRelativePath).ToList(),
                    //PrimaryPhotoPath = wizardState.UploadedPhotos.FirstOrDefault(p => p.IsPrimaryPropertyPhoto)?.TempRelativePath,
                };

                var propertyId = await _createPropertyService.CreatePropertyAsync(dto);

                HttpContext.Session.Remove(GetSessionKey(landlordUserId));
               
                _logger.LogInformation("Landlord {LandlordUserId} Created property {CreatedPropertyId} via wizard. Wizard Complete",
                    landlordUserId,
                    propertyId
                );

                TempData["SuccessMessage"] = "Your property was submitted successfully.";
                return RedirectToAction("PropertyDetails", "Property", new { propertyId = propertyId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Wizard submission failed validation. Landlord {LandlordUserId}.", 
                    landlordUserId);

                ModelState.AddModelError("", ex.Message);
                return View(nameof(ReviewAndSubmit), await BuildReviewStepViewModel(wizardState!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Wizard submission failed. Landlord {LandlordUserId}, .",
                    landlordUserId
                );

                ModelState.AddModelError(string.Empty, "Unable to complete your request. Please try again later.");
                return View(nameof(ReviewAndSubmit), await BuildReviewStepViewModel(wizardState!));
            }
        }


        private PropertyWizardStateViewModel? GetWizardState(string landlordUserId)
        {
            var sessionJson = HttpContext.Session.GetString(GetSessionKey(landlordUserId));
            if (string.IsNullOrWhiteSpace(sessionJson))
            {
                return null;
            }

            var wizardState = JsonSerializer.Deserialize<PropertyWizardStateViewModel>(sessionJson);
            if (wizardState != null)
            {
                wizardState.BasicPropertyInfo ??= new BasicPropertyInfoStepViewModel();
                wizardState.Address ??= new AddressStepViewModel();
                wizardState.SelectedAmenityIds ??= new List<int>();
                wizardState.UploadedPhotos ??= new List<UploadedPhotoViewModel>();
            }

            return wizardState;
        }



        private async Task<PropertyReviewStepViewModel> BuildReviewStepViewModel(PropertyWizardStateViewModel wizardState)
        {
            var allAmenities = (await _amenityRepository.GetAllAmenities()).ToList();
            var selectedAmenityIds = wizardState.SelectedAmenityIds.ToHashSet();

            return new PropertyReviewStepViewModel
            {
                BasicPropertyInfo = wizardState.BasicPropertyInfo,
                Address = wizardState.Address,
                SelectedAmenities = allAmenities
                    .Where(amenity => selectedAmenityIds.Contains(amenity.AmenityId))
                    .ToList(),
                PropertyPhotos = GetUniqueUploadedPhotos(wizardState.UploadedPhotos).ToList()
            };
        }

        private IActionResult? EnsureReadyForReview(PropertyWizardStateViewModel? wizardState)
        {
            if (wizardState == null)
            {
                return RedirectToAction(nameof(BasicPropertyInfo));
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
                TempData["PhotoError"] = "Upload at least one photo before reviewing your property.";
                return RedirectToAction(nameof(Photos));
            }

            return null;
        }

        private static IEnumerable<UploadedPhotoViewModel> GetUniqueUploadedPhotos(IEnumerable<UploadedPhotoViewModel> uploadedPhotos)
        {
            return uploadedPhotos
                .Where(photo => !string.IsNullOrWhiteSpace(photo.TempRelativePath))
                .GroupBy(photo => photo.TempRelativePath, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First());
        }

         private void SaveWizardState(string landlordUserId, PropertyWizardStateViewModel wizardState)
        {
            var sessionJson = JsonSerializer.Serialize(wizardState);
            HttpContext.Session.SetString(GetSessionKey(landlordUserId), sessionJson);
        }

        private static bool HasCompletedBasicPropertyInfo(PropertyWizardStateViewModel wizardState)
        {
            return wizardState.BasicPropertyInfo.PropertyType.HasValue;
        }

        private static bool HasCompletedAddress(PropertyWizardStateViewModel wizardState)
        {
            return !string.IsNullOrWhiteSpace(wizardState.Address.Province)
                && !string.IsNullOrWhiteSpace(wizardState.Address.City)
                && !string.IsNullOrWhiteSpace(wizardState.Address.Suburb)
                && !string.IsNullOrWhiteSpace(wizardState.Address.Street);
        }

        private static string GetSessionKey(string landlordUserId)
        {
            return $"CreatePropertyWizard:{landlordUserId}";
        }

    }
}
