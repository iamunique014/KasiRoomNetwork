using Kasi_Room_Network___KRN.Constants;
using Kasi_Room_Network___KRN.Services;
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
        private readonly ILandlordRepository _landlordRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IPhotoStorageService _photoStorageService;

        public PropertyController(IPropertyRepository propertyRepository,
            IProfileRepository profileRepository, 
            ILandlordRepository landlordRepository, 
            IAmenityRepository amenityRepository,
            IPhotoStorageService photoStorageService)
        {
            _propertyRepository = propertyRepository;
            _profileRepository = profileRepository;
            _landlordRepository = landlordRepository;
            _amenityRepository = amenityRepository;
            _photoStorageService = photoStorageService;
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

            var amenities = await _amenityRepository.GetAllAmenities();
            var model = new CreatePropertyViewModel
            {
                Amenities = amenities.ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(CreatePropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var amenities = await _amenityRepository.GetAllAmenities();
                model.Amenities = amenities.ToList();

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

            int propertyId = await _propertyRepository.CreateProperty(model, landlordUserId);

            if (model.SelectedAmenityIds != null && model.SelectedAmenityIds.Any())
            {
                foreach (var amenityId in model.SelectedAmenityIds)
                {
                    await _amenityRepository.AddPropertyAmenity(propertyId, amenityId, landlordUserId);
                }
            }

            TempData["SuccessMessage"] = "Property created successfully. You can now add a room listing for it.";

            return RedirectToAction(nameof(AddPropertyPhotos), new { propertyId });
        }

        // =========================
        // ADD PHOTOS
        // =========================

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> AddPropertyPhotos(int propertyId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            if (property.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            ViewBag.PropertyId = propertyId;
            ViewBag.PhotoCount = await _propertyRepository.GetPropertyPhotoCount(propertyId);
            return View(); 
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPropertyPhotos(int propertyId, IFormFile photo, bool isPrimary)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            if (property.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            ViewBag.PropertyId = propertyId;
            ViewBag.PhotoCount = await _propertyRepository.GetPropertyPhotoCount(propertyId);

            string? dbPath = null;

            try
            {
                dbPath = await _photoStorageService
                    .SaveOptimizedImageAsync(
                        photo,
                        ImageCategory.Property);

                var added = await _propertyRepository.
                    AddPropertyPhoto(
                    propertyId,
                    dbPath,
                    isPrimary,
                    landlordUserId);

                if (!added)
                {
                    _photoStorageService.DeletePhoto(dbPath);

                    ModelState.AddModelError(
                        "",
                        "Photo could not be uploaded because the listing was not found or you no longer have access.");

                    return View();
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
           
            TempData["PhotoUploaded"] = "Photo uploaded successfully";

            return RedirectToAction(nameof(AddPropertyPhotos), new { propertyId });
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> MyProperties(string? mode = null)
        {
            ViewBag.Mode = mode;

            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var properties = await _landlordRepository.GetAllPropertiesByLandlord(landlordUserId);
            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> PropertyDetails(int propertyId)
        {
            var property = await _propertyRepository.GetPropertyById(propertyId);

            if (property == null)
            {
                return NotFound();
            }

            property.Photos = await _propertyRepository.GetPropertyPhotos(propertyId);

            property.Amenities = (await _amenityRepository
                .GetAmenitiesByPropertyId(propertyId))
                .ToList();

            return View(property);
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> EditProperty(int propertyId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var property = await _propertyRepository
                .GetPropertyForEditAsync(propertyId, landlordUserId);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(EditPropertyViewModel model)
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

            await _propertyRepository.UpdatePropertyAsync(model, landlordUserId);

            TempData["SuccessMessage"] = "Property updated successfully.";

            return RedirectToAction(
                "PropertyDetails",
                new { propertyId = model.PropertyId });
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            await _propertyRepository.DeletePropertyAsync(propertyId, landlordUserId);

            TempData["SuccessMessage"] = "Property deleted successfully.";

            return RedirectToAction(nameof(MyProperties));
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> ManagePropertyPhotos(int propertyId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null || property.LandlordUserId != landlordUserId)
            {
                return NotFound();
            }

            var viewModel = new ManagePropertyPhotosViewModel
            {
                PropertyId = property.PropertyId,
                PropertyName = property.PropertyName,
                Photos = await _propertyRepository.GetPropertyPhotos(propertyId)
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPropertyPhoto(int propertyId, IFormFile photo, bool isPrimary)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null || property.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to upload photos for this property.";
                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }

            string? dbPath = null;

            try
            {
                dbPath = await _photoStorageService
                    .SaveOptimizedImageAsync(
                        photo,
                        ImageCategory.Property);

                var added = await _propertyRepository.
                    AddPropertyPhoto(
                    propertyId,
                    dbPath,
                    isPrimary,
                    landlordUserId);

                if (!added)
                {
                    _photoStorageService.DeletePhoto(dbPath);

                    ModelState.AddModelError(
                        "",
                        "Photo could not be uploaded because the listing was not found or you no longer have access.");

                    return View();
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

            TempData["SuccessMessage"] = "Photo uploaded successfully.";

            return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
        }

        public IActionResult PropertySubmitted(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePropertyPhoto(int propertyId, int photoId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            // Ownership Verification
            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null || property.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete photos for this property.";
                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }

            try
            {
                await _propertyRepository.DeletePropertyPhoto(photoId, propertyId);
                TempData["SuccessMessage"] = "Photo deleted successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the photo.";
            }

            return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryPropertyPhoto(int propertyId, int photoId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            // Ownership Verification
            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null || property.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to modify photos for this property.";
                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }

            try
            {
                await _propertyRepository.SetPrimaryPropertyPhoto(propertyId, photoId);
                TempData["SuccessMessage"] = "Primary photo updated successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the primary photo.";
            }

            return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
        }
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> EditPropertyAmenities(int propertyId)
        {
            var landlordId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(landlordId))
            {
                return Challenge();
            }

            var model = await _amenityRepository.GetPropertyAmenitiesForEditAsync(propertyId,landlordId);

            if (model == null)
                return NotFound();

            return View(model);
        }
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPropertyAmenities( EditPropertyAmenitiesViewModel model)
        {
            var landlordId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(landlordId))
            {
                return Challenge();
            }

            var selectedAmenityIds = model.Amenities
                .Where(a => a.IsSelected)
                .Select(a => a.AmenityId)
                .ToList();

            await _amenityRepository
                .UpdatePropertyAmenitiesAsync(
                    model.PropertyId,
                    selectedAmenityIds,
                    landlordId);

            TempData["Success"] = "Amenities updated successfully.";

            return RedirectToAction(
                nameof(PropertyDetails),
                new { propertyId = model.PropertyId });
        }
    }
}
