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


        private async Task<ListingDetailsViewModel?> GetOwnedListingOrNull(int listingId, string landlordUserId)
        {
            var listing = await _listingRepository.GetListingById(listingId);

            if (listing == null || listing.LandlordUserId != landlordUserId)
            {
                return null;
            }

            return listing;
        }

        private static bool IsPubliclyVisible(ListingDetailsViewModel listing)
        {
            return listing.IsAvailable && listing.IsVerified && listing.PropertyVerified;
        }

        private async Task<string> SaveListingPhoto(IFormFile photo, string extension)
        {
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

            return "/uploads/listings/" + fileName;
        }

        private void DeleteSavedListingPhoto(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                return;
            }

            var relativePath = dbPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.StartsWith("wwwroot") ? relativePath[8..] : relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
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
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            ViewBag.ListingId = listingId;
            ViewBag.PhotoCount = await _listingRepository.GetListingPhotoCount(listingId);
            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddListingPhotos(int listingId, IFormFile photo, bool isPrimary)
        {
            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(landlordUserId))
            {
                return Challenge();
            }

            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            ViewBag.ListingId = listingId;
            ViewBag.PhotoCount = await _listingRepository.GetListingPhotoCount(listingId);

            if (photo == null || photo.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a photo.");
                return View();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", "Only JPG and PNG images are allowed.");
                return View();
            }

            if (photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Image size cannot exceed 2MB.");
                return View();
            }

            var dbPath = await SaveListingPhoto(photo, extension);
            var added = await _listingRepository.AddListingPhoto(listingId, dbPath, isPrimary, landlordUserId);

            if (!added)
            {
                DeleteSavedListingPhoto(dbPath);
                ModelState.AddModelError("", "Photo could not be uploaded because the listing was not found or you no longer have access.");
                return View();
            }

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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = User.Identity?.IsAuthenticated == true && listing.LandlordUserId == currentUserId;

            if (!isOwner && !IsPubliclyVisible(listing))
            {
                return NotFound();
            }

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

            var listing = await _listingRepository.GetListingById(model.ListingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            try
            {
                var updated = await _listingRepository.UpdateListing(model, landlordUserId);
                if (!updated)
                {
                    ModelState.AddModelError("", "Listing could not be updated. It may have been removed or you may no longer have access.");
                    TempData["ErrorMessage"] = "Listing could not be updated.";
                    return View(model);
                }

                TempData["SuccessMessage"] = "Listing updated successfully. Verification has been reset and the listing must be approved before it appears publicly again.";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while updating the listing.");
                TempData["ErrorMessage"] = "An error occurred while updating the listing.";
                return View(model);
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
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
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
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            if (photo == null || photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a photo to upload.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

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

            var dbPath = await SaveListingPhoto(photo, extension);
            var added = await _listingRepository.AddListingPhoto(listingId, dbPath, isPrimary, landlordUserId);

            if (!added)
            {
                DeleteSavedListingPhoto(dbPath);
                TempData["ErrorMessage"] = "Photo could not be uploaded because the listing was not found or you no longer have access.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

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

            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            try
            {
                var deleted = await _listingRepository.DeleteListingPhoto(photoId, listingId, landlordUserId);
                TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
                    ? "Photo deleted successfully."
                    : "Photo could not be deleted because it was not found for this listing.";
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

            var listing = await _listingRepository.GetListingById(listingId);
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.LandlordUserId != landlordUserId)
            {
                return Forbid();
            }

            try
            {
                var updated = await _listingRepository.SetPrimaryListingPhoto(listingId, photoId, landlordUserId);
                TempData[updated ? "SuccessMessage" : "ErrorMessage"] = updated
                    ? "Primary photo updated successfully."
                    : "Primary photo could not be updated because the photo was not found for this listing.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the primary photo.";
            }

            return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
        }
    }
}
