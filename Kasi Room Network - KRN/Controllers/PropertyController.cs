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
        private readonly ILogger<PropertyController> _logger;

        public PropertyController(IPropertyRepository propertyRepository,
            IProfileRepository profileRepository, 
            ILandlordRepository landlordRepository, 
            IAmenityRepository amenityRepository,
            IPhotoStorageService photoStorageService,
            ILogger<PropertyController> logger)
        {
            _propertyRepository = propertyRepository;
            _profileRepository = profileRepository;
            _landlordRepository = landlordRepository;
            _amenityRepository = amenityRepository;
            _photoStorageService = photoStorageService;
            _logger = logger;
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> CreateProperty()
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                _logger.LogWarning("Anonymous user attempted to access CreateProperty.");
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(landlordUserId);
            if (!hasCompleteProfile)
            {
                _logger.LogInformation(
                    "Landlord {LandlordUserId} redirected to complete profile before creating a property.",
                    landlordUserId);

                TempData["ProfilePrompt"] = "Before creating a property, complete your landlord profile first.";

                return RedirectToAction(
                    "MyProfile",
                    "Profile",
                    new { returnUrl = Url.Action("CreateProperty", "Property") });
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
                _logger.LogWarning("Anonymous user attempted to access createProperty");
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(landlordUserId);
            if (!hasCompleteProfile)
            {
                _logger.LogInformation(
                    "Landlord {LandlordUserId} attempted to create a property with incomplete profile.",
                     landlordUserId
                );

                TempData["ProfilePrompt"] = "Complete your profile first to continue creating your property.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("CreateProperty", "Property") });
            }

            try
            {
                int propertyId = await _propertyRepository.CreateProperty(model, landlordUserId);

                if (model.SelectedAmenityIds != null && model.SelectedAmenityIds.Any())
                {
                    foreach (var amenityId in model.SelectedAmenityIds)
                    {
                        await _amenityRepository.AddPropertyAmenity(propertyId, amenityId, landlordUserId);
                    }
                }

                _logger.LogInformation(
                    "Property {PropertyId} created successfully for landlord {LandlordUserId}.", propertyId, landlordUserId);

                TempData["SuccessMessage"] = "Property created successfully. You can now add a room listing for it.";
                return RedirectToAction(nameof(AddPropertyPhotos), new { propertyId });
            }
            catch (Exception ex)
            {
               _logger.LogError(
                    ex,
                    "Failed to create property for landlord {LandlordUserId}.",
                    landlordUserId
                );

                ModelState.AddModelError("", "Unable to create property at this time. Please try again later.");
                var amenities = await _amenityRepository.GetAllAmenities();
                model.Amenities = amenities.ToList();
                return View(model);
            }
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
                _logger.LogWarning(
                    "Anonymous user tried to access AddPropertyPhotos for propertyId {PropertyId}.", propertyId);

                return Challenge();
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null)
            {
                _logger.LogWarning(
                    "Property {PropertyId} was not found. Landlord {LandlordUserId} attempted to add photos.",
                    propertyId,
                    landlordUserId
                );
                return NotFound();
            }

            if (property.LandlordUserId != landlordUserId)
            {
                _logger.LogWarning(
                    "Landlord {LandlordUserId} attempted unauthorized access to AddPropertyPhotos of Property {PropertyId}", landlordUserId, propertyId);

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
                _logger.LogWarning(
                    "Anonymous user tried to upload photos for propertyId {PropertyId}.", propertyId);
                return Challenge();
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);
            if (property == null)
            {
                _logger.LogWarning(
                    "Property {PropertyId} was not found, Landlord {LandlordUserId} tried to add photos.", propertyId, landlordUserId);
                return NotFound();
            }

            if (property.LandlordUserId != landlordUserId)
            {
                _logger.LogWarning(
                    "Landlord {LandlordUserId} attempted unauthorized access to upload photos for Property {PropertyId}", landlordUserId, propertyId);

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
                    _logger.LogWarning(
                        "Repository Rejected photo upload for property {PropertyId}, Removing uploaded files.", propertyId);

                    _photoStorageService.DeletePhoto(dbPath);

                    ModelState.AddModelError(
                        "",
                        "Photo could not be uploaded because the property was not found or you no longer have access.");

                    return View();
                }

                TempData["PhotoUploaded"] = "Photo uploaded successfully";

                return RedirectToAction(nameof(AddPropertyPhotos), new { propertyId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Photo validation failed for property {PropertyId}.", propertyId);

                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch(Exception ex)
            {
                if(!string.IsNullOrWhiteSpace(dbPath)){
                    _photoStorageService.DeletePhoto(dbPath);
                }

                _logger.LogError(
                    ex,
                    "Landlord {LandlordUserId} could not upload photo for property {PropertyId}.",
                    landlordUserId,
                    propertyId
                );

                ModelState.AddModelError("",
                    "Something went wrong while uploading your photo please try again."
                );

                return View();
            }
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> MyProperties(string? mode = null)
        {
            ViewBag.Mode = mode;

            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                _logger.LogWarning("Anonymous user tried accessing My Properties");
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
                _logger.LogWarning("Anonymous user attempted access to EditProperty");
                return Challenge();
            }

            var property = await _propertyRepository
                .GetPropertyForEditAsync(propertyId, landlordUserId);

            if (property == null)
            {
                _logger.LogWarning("Landlord {LandlordUserId} attempted editing unkown property", landlordUserId);
                return NotFound();
            }

            //if (property.LandlordUserId != landlordUserId)
            //{
            //    _logger.LogWarning(
            //        "Landlord {LandlordUserId} attempted unauthorized access to EditProperty of Property {PropertyId}", landlordUserId, propertyId);
//
            //    return Forbid();
            //}

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
                _logger.LogWarning(
                    "Anonymous user tried to Edit Property {PropertyId}.", model.PropertyId);
                return Challenge();
            }

            try
            {
                
                await _propertyRepository.UpdatePropertyAsync(model, landlordUserId);
                TempData["SuccessMessage"] = "Property updated successfully.";

                _logger.LogInformation(
                    "Landlord {LandlordUserId} successfully updated property {PropertyId}",
                    landlordUserId, model.PropertyId
                );

                return RedirectToAction("PropertyDetails", new { propertyId = model.PropertyId });
            }
            catch (Exception ex)
            {
                
                _logger.LogError(
                    ex,
                    "Landlord {LandlordUserId} could not update property {PropertyId}.",
                    landlordUserId, 
                    model.PropertyId
                );

                ModelState.AddModelError("", "Unable to update property at this time. Please try again later.");
                return View(model);
            }
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

            try
            {
               
                await _propertyRepository.DeletePropertyAsync(propertyId, landlordUserId);
                

                _logger.LogInformation(
                    "Property {PropertyId} of Landlord {LandlordID} was deleted successfully.", 
                    propertyId, landlordUserId
                );

                TempData["SuccessMessage"] = "Property deleted successfully.";
                return RedirectToAction(nameof(MyProperties));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Landlord {LandlordUserId} Could not Delete property {PropertyId}.",
                    landlordUserId, 
                    propertyId
                );
                TempData["ErrorMessage"] = "Unable to complete your request. Please try again later.";
                return RedirectToAction(nameof(MyProperties));
            }
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
                _logger.LogWarning("Landlord {LandlordUserId} attempted access to ManagePropertyPhotos, Property unkown or unauthorised access",
                    landlordUserId
                );
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

            var viewModel = new ManagePropertyPhotosViewModel
            {
                PropertyId = property.PropertyId,
                PropertyName = property.PropertyName,
                Photos = await _propertyRepository.GetPropertyPhotos(propertyId)
            };

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
                    _logger.LogWarning(
                        "Repository rejected photo upload of property {PropertyId}. Removing upload files",
                        propertyId
                    );
                    _photoStorageService.DeletePhoto(dbPath);

                    ModelState.AddModelError(
                        "",
                        "Photo could not be uploaded because the property was not found or you no longer have access.");

                    return View("ManagePropertyPhotos", viewModel);
                }

                _logger.LogInformation("Photo uploaded successfully for property {propertyId}.",
                    propertyId
                );

                TempData["SuccessMessage"] = "Photo uploaded successfully.";

                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Landlord {LandlordId} Could not upload photo to property {PropertyId}.",
                    landlordUserId,
                    propertyId
                );

                ModelState.AddModelError("",ex.Message);
                return View("ManagePropertyPhotos", viewModel);
            }
            catch(Exception ex)
            {
                if(!string.IsNullOrWhiteSpace(dbPath)){
                    _photoStorageService.DeletePhoto(dbPath);
                }

                _logger.LogError(
                    ex,
                    "Landlord {LandlordId} Could not upload photo to property {PropertyId}.",
                    landlordUserId,
                    propertyId
                );
                ModelState.AddModelError("",
                    "Something went wrong while uploading your photo please try again."
                );

                return View("ManagePropertyPhotos", viewModel);
            }  
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
                await _propertyRepository.DeletePropertyPhoto(photoId, propertyId, LandlordUserId);
                
                _logger.LogInformation(
                    "Photo {PhotoID} Of Property {PropertyId} of Landlord {LandlordID} was deleted successfully.", 
                    photoId, propertyId, landlordUserId
                );

                TempData["SuccessMessage"] = "Photo deleted successfully.";
                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Landlord {LandlordUserId} Could not delete Photo {PhotoId} of property {PropertyId}.",
                    landlordUserId,
                    photoId, 
                    propertyId
                );

                TempData["ErrorMessage"] = "Unable to complete your request. Please try again later.";
                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }
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
                await _propertyRepository.SetPrimaryPropertyPhoto(propertyId, photoId, landlordUserId);
                TempData["SuccessMessage"] = "Primary photo updated successfully.";

                _logger.LogInformation(
                    "Primary photo of Property {PropertyId} was updated successfully.",
                    propertyId
                );

                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Landlord {LandlordUserId} could not set primary photo for property {PropertyId}.",
                    landlordUserId,
                    propertyId
                );
                TempData["ErrorMessage"] = "Unable to complete your request. Please try again later.";
                return RedirectToAction(nameof(ManagePropertyPhotos), new { propertyId });
            }
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

            try
            {
                await _amenityRepository.UpdatePropertyAmenitiesAsync(model.PropertyId, selectedAmenityIds, landlordId);
                TempData["SuccessMessage"] = "Amenities updated successfully.";

                _logger.LogInformation("Amenities for property {propertyId} were updated successfully.",
                    model.PropertyId
                );
                return RedirectToAction(nameof(PropertyDetails), new { propertyId = model.PropertyId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Landlord {LandlordId} could not update amenities for property {PropertyId}.",
                    landlordId,
                    model.PropertyId
                );
                TempData["ErrorMessage"] = "Unable to update amenities at this time. Please try again later.";
                return RedirectToAction(nameof(PropertyDetails), new { propertyId = model.PropertyId });
            } 
        }
    }
}
