using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class ListingController : Controller
    {
        private readonly IListingRepository _listingRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IPropertyRepository _propertyRepository;

        public ListingController(IListingRepository listingRepository, IProfileRepository profileRepository, IPropertyRepository propertyRepository)
        {
            _listingRepository = listingRepository;
            _profileRepository = profileRepository;
            _propertyRepository = propertyRepository;
        }

        // =========================
        // CREATE LISTING
        // =========================

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> CreateListing(int propertyId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var hasCompleteProfile = await _profileRepository.IsComplete(landlordUserId);
            if (!hasCompleteProfile)
            {
                TempData["ProfilePrompt"] = "Before posting a room, complete your landlord profile so tenants can trust your listing.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("CreateListing", "Listing") });
            }

            var property = await _propertyRepository.GetPropertyById(propertyId);

            var model = new CreateListingViewModel
            {
                PropertyId = propertyId,
                PropertyName = property.PropertyName,
                Suburb = property.Suburb,
                City = property.City
            };

            return View(model);
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListing(CreateListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var property = await _propertyRepository.GetPropertyById(model.PropertyId);

                model.PropertyName = property.PropertyName;
                model.Suburb = property.Suburb;
                model.City = property.City;

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
                TempData["ProfilePrompt"] = "Complete your profile first to continue posting your room.";
                return RedirectToAction("MyProfile", "Profile", new { returnUrl = Url.Action("CreateListing", "Listing") });
            }

            int listingId = await _listingRepository.CreateListing(model, landlordUserId);

            return RedirectToAction(nameof(AddListingPhotos), new { listingId });
        }

        // =========================
        // ADD PHOTOS
        // =========================

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> AddListingPhotos(int listingId)
        {
            ViewBag.ListingId = listingId;
            ViewBag.PhotoCount = await _listingRepository.GetListingPhotoCount(listingId);
            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddListingPhotos(int listingId, IFormFile photo, bool isPrimary)
        {
            if (photo == null || photo.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a photo.");
                ViewBag.ListingId = listingId;
                ViewBag.PhotoCount = await _listingRepository.GetListingPhotoCount(listingId);
                return View();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Only JPG and PNG images are allowed.");
                ViewBag.ListingId = listingId;
                ViewBag.PhotoCount = await _listingRepository.GetListingPhotoCount(listingId);
                return View();
            }

            // Validate file size (max 2MB)
            if (photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Image size cannot exceed 2MB.");
                ViewBag.ListingId = listingId;
                ViewBag.PhotoCount = await _listingRepository.GetListingPhotoCount(listingId);
                return View();
            }

            // Create uploads path
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/listings"
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
            var dbPath = "/uploads/listings/" + fileName;

            await _listingRepository.AddListingPhoto(listingId, dbPath, isPrimary);
            TempData["PhotoUploaded"] = "Photo uploaded successfully";

            return RedirectToAction(nameof(AddListingPhotos), new { listingId });
        }

        // =========================
        // LISTING DETAILS
        // =========================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ListingDetails(int listingId)
        {
            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null)
                return NotFound();

            listing.Photos = await _listingRepository.GetListingPhotos(listingId);

            return View(listing);
        }

        // =========================
        // SEARCH
        // =========================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SearchListings(ListingSearchViewModel model)
        {
            if (Request.Query.Any())
            {
                model.Province = string.IsNullOrWhiteSpace(model.Province) ? null : model.Province;
                model.City = string.IsNullOrWhiteSpace(model.City) ? null : model.City;
                model.Suburb = string.IsNullOrWhiteSpace(model.Suburb) ? null : model.Suburb;

                model.MinPrice = model.MinPrice <= 0 ? null : model.MinPrice;
                model.MaxPrice = model.MaxPrice <= 0 ? null : model.MaxPrice;

                model.Results = await _listingRepository.SearchListings(model);
            }

            return View(model);
        }

        // ==============================
        // LISTING SUBMITTED CONFIRMATION
        // ==============================

        public IActionResult ListingSubmitted(int id)
        {
            ViewBag.ListingId = id;
            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> EditListing(int listingId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var listing = await _listingRepository
                .GetListingForEdit(listingId, landlordUserId);

            if (listing == null)
            {
                return NotFound();
            }

            return View(listing);
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditListing(EditListingViewModel model)
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

            // Ownership Verification
            var listing = await _listingRepository.GetListingById(model.ListingId);
            if (listing == null || listing.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this listing.";
                return RedirectToAction(nameof(EditListing), new { listingId = model.ListingId });
            }

            try
            {
                await _listingRepository.UpdateListing(model, landlordUserId);
                TempData["SuccessMessage"] = "Listing updated successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the listing.";
            }

            return RedirectToAction(
                "ListingDetails",
                new { listingId = model.ListingId });
        }
        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public async Task<IActionResult> ManageListingPhotos(int listingId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null || listing.LandlordUserId != landlordUserId)
            {
                return NotFound();
            }

            var viewModel = new ManageListingPhotosViewModel
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Photos = await _listingRepository.GetListingPhotos(listingId)
            };

            return View(viewModel);
        }
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadListingPhoto(int listingId, IFormFile photo, bool isPrimary)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null || listing.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to upload photos for this listing.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            if (photo == null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a photo to upload.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Only JPG and PNG images are allowed.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            if (photo.Length > 2 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Image size cannot exceed 2MB.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/uploads/listings"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            var dbPath = "/uploads/listings/" + fileName;

            await _listingRepository.AddListingPhoto(listingId, dbPath, isPrimary);
            TempData["SuccessMessage"] = "Photo uploaded successfully.";

            return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
        }
        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteListingPhoto(int listingId, int photoId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            // Ownership Verification
            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null || listing.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete photos for this listing.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            try
            {
                await _listingRepository.DeleteListingPhoto(photoId, listingId);
                TempData["SuccessMessage"] = "Photo deleted successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the photo.";
            }

            return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
        }
        

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryListingPhoto(int listingId, int photoId)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            // Ownership Verification
            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null || listing.LandlordUserId != landlordUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to modify photos for this listing.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            try
            {
                await _listingRepository.SetPrimaryListingPhoto(listingId, photoId);
                TempData["SuccessMessage"] = "Primary photo updated successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the primary photo.";
            }

            return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
        }
    }
}
