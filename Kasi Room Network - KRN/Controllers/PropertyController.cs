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

        public PropertyController(IPropertyRepository propertyRepository,
            IProfileRepository profileRepository, 
            ILandlordRepository landlordRepository, 
            IAmenityRepository amenityRepository)
        {
            _propertyRepository = propertyRepository;
            _profileRepository = profileRepository;
            _landlordRepository = landlordRepository;
            _amenityRepository = amenityRepository;
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
                    await _amenityRepository.AddPropertyAmenity(propertyId, amenityId);
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
            ViewBag.PropertyId = propertyId;
            ViewBag.PhotoCount = await _propertyRepository.GetPropertyPhotoCount(propertyId);
            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPropertyPhotos(int propertyId, IFormFile photo, bool isPrimary)
        {
            if (photo == null || photo.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a photo.");
                ViewBag.PropertyId = propertyId;
                ViewBag.PhotoCount = await _propertyRepository.GetPropertyPhotoCount(propertyId);
                return View();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Only JPG and PNG images are allowed.");
                ViewBag.PropertyId = propertyId;
                ViewBag.PhotoCount = await _propertyRepository.GetPropertyPhotoCount(propertyId);
                return View();
            }

            // Validate file size (max 2MB)
            if (photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Image size cannot exceed 2MB.");
                ViewBag.PropertyId = propertyId;
                ViewBag.PhotoCount = await _propertyRepository.GetPropertyPhotoCount(propertyId);
                return View();
            }

            // Create uploads path
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/properties"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate safe file name
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Store relative path in DB
            var dbPath = "/uploads/properties/" + fileName;

            await _propertyRepository.AddPropertyPhoto(propertyId, dbPath, isPrimary);
            TempData["PhotoUploaded"] = "Photo uploaded successfully";

            return RedirectToAction(nameof(AddPropertyPhotos), new { propertyId });
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> MyProperties()
        {
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

            // Load amenities
            property.Amenities = (await _amenityRepository
                .GetAmenitiesByPropertyId(propertyId))
                .ToList();

            return View(property);
        }

        public IActionResult PropertySubmitted(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }

    }
}
