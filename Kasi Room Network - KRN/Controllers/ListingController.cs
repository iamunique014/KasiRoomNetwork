using Kasi_Room_Network___KRN.Constants;
using Kasi_Room_Network___KRN.Services;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class ListingController(IListingRepository listingRepository, IProfileRepository profileRepository, IPropertyRepository propertyRepository, IAmenityRepository amenityRepository, ILogger<ListingController> logger, IPhotoStorageService photoStorageService) : Controller
    {
        private readonly IListingRepository _listingRepository = listingRepository;
        private readonly IProfileRepository _profileRepository = profileRepository;
        private readonly IPropertyRepository _propertyRepository = propertyRepository;
        private readonly IAmenityRepository _amenityRepository = amenityRepository;
        private readonly ILogger<ListingController> _logger = logger;
        private readonly IPhotoStorageService _photoStorageService = photoStorageService;

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

            try
            {
                int listingId = await _listingRepository.CreateListing(model, landlordUserId);
                _logger.LogInformation("Created Listing for landlord {LandlordUserId}. Moving to Photos.",
                    landlordUserId
                );
                return RedirectToAction(nameof(AddListingPhotos), new { listingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred while creating listing for landlord {LandlordUserId}.",
                    landlordUserId
                );

                ModelState.AddModelError("", "Unable to create listing at this time. Please try again later.");
                var property = await _propertyRepository.GetPropertyById(model.PropertyId);
                model.PropertyName = property.PropertyName;
                model.Suburb = property.Suburb;
                model.City = property.City;
                return View(model);
            }
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

            string? dbPath = null;

            try
            {
                _logger.LogInformation("Uploading ListingPhoto of Listing {listingId}.", listingId);

                dbPath = await _photoStorageService
                    .SaveOptimizedImageAsync(
                        photo,
                        ImageCategory.Listing);

                var added = await _listingRepository
                    .AddListingPhoto(
                        listingId,
                        dbPath,
                        isPrimary,
                        landlordUserId);

                if (!added)
                {
                    _logger.LogWarning(
                        "Repository Rejected photo upload for listing {listingId}, Removing uploaded files.", 
                            listingId
                    );

                    _photoStorageService.DeletePhoto(dbPath);

                    ModelState.AddModelError(
                        "",
                        "Photo could not be uploaded because the listing was not found or you no longer have access."
                    );

                    return View();
                }

                _logger.LogInformation("Photo uploaded successfully for listing {listingId}.", listingId);

                TempData["PhotoUploaded"] = "Photo uploaded successfully";
                return RedirectToAction(nameof(AddListingPhotos), new { listingId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Photo validation failed for listing {listingId}.", listingId);

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
                    "Unexpected error while uploading photo for listing {listingId}.", 
                    listingId
                );

                ModelState.AddModelError("",
                    "Something went wrong while uploading your photo please try again."
                );

                return View();
            }
        }

        // =========================
        // LISTING DETAILS
        // =========================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> MyListingDetails(int listingId)
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ListingDetails(int listingId)
        {
            var listing = await _listingRepository.GetListingDetailsById(listingId);
            if (listing == null)
                return NotFound();

            listing.Photos = [.. await _listingRepository.GetListingPhotos(listingId)];
            listing.Amenities = (await _amenityRepository.GetAmenitiesByPropertyId(listing.PropertyId)).ToList();
            return View(listing);
        }

        // =========================
        // SEARCH
        // =========================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SearchListings(ListingSearchViewModel model)
        {
            bool hasSearch = !string.IsNullOrWhiteSpace(model.City) ||
             !string.IsNullOrWhiteSpace(model.Suburb) ||
             !string.IsNullOrWhiteSpace(model.Province) ||
             model.MinPrice.HasValue ||
             model.MaxPrice.HasValue;

            if (hasSearch)
            {
                _logger.LogInformation("Listing search. City={City}, Suburb={Suburb}, Province={Province}",
                    model.City,
                    model.Suburb, 
                    model.Province
                );
                model.Results =
                    await _listingRepository.SearchListings(model);
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
                    _logger.LogWarning(
                        "Landlord {LandlordUserId} Failed updating listing {listingId}", 
                        model.ListingId,
                        landlordUserId
                    );

                    ModelState.AddModelError("", "Listing could not be updated. It may have been removed or you may no longer have access.");
                    TempData["ErrorMessage"] = "Listing could not be updated.";
                    return View(model);
                }

                _logger.LogInformation(
                    "Landlord {LandlordUserId} successfully  updated listing {ListingId}",
                    landlordUserId, model.ListingId
                );
                TempData["SuccessMessage"] = "Listing updated successfully. Verification has been reset and the listing must be approved before it appears publicly again.";
                return RedirectToAction(
                    "ListingDetails",
                    new { listingId = model.ListingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred while updating Listing {listingId} of Landlord {LandlordUserId}.",
                    model.ListingId, landlordUserId
                );

                ModelState.AddModelError("", "Unable to update listing at this time. Please try again later.");
                TempData["ErrorMessage"] = "Unable to complete your request. Please try again later.";
                return View(model);
            }
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

            var viewModel = new ManageListingPhotosViewModel
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Photos = await _listingRepository.GetListingPhotos(listingId)
            };
            string? dbPath = null;

            try
            {
                _logger.LogInformation("Uploading ListingPhoto of Listing {listingId}.", listingId);

                dbPath = await _photoStorageService
                    .SaveOptimizedImageAsync(
                        photo,
                        ImageCategory.Listing);

                var added = await _listingRepository
                    .AddListingPhoto(
                        listingId,
                        dbPath,
                        isPrimary,
                        landlordUserId);

                if (!added)
                {
                    _logger.LogWarning(
                        "Repository Rejected photo upload for listing {listingId}, Removing uploaded files.", 
                            listingId
                    );

                    _photoStorageService.DeletePhoto(dbPath);

                    ModelState.AddModelError(
                        "",
                        "Photo could not be uploaded because the listing was not found or you no longer have access.");

                    return View("ManageListingPhotos", viewModel);
                }

                _logger.LogInformation("Photo uploaded successfully for listing {listingId}.", listingId);

                TempData["SuccessMessage"] = "Photo uploaded successfully";
            
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Photo validation failed for listing {ListingId}.", 
                    listingId
                );

                ModelState.AddModelError("", ex.Message);
                return View("ManageListingPhotos", viewModel);
            }
            catch(Exception ex)
            {
                if(!string.IsNullOrWhiteSpace(dbPath)){
                    _photoStorageService.DeletePhoto(dbPath);
                }

                _logger.LogError(
                    ex,
                    "Unexpected error while uploading photo for listing {listingId}.", 
                    listingId
                );

                ModelState.AddModelError("",
                    "Something went wrong while uploading your photo please try again."
                );

                return View("ManageListingPhotos", viewModel);
            }
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
                _logger.LogInformation(
                    "Deleting listing photo of {listingId} by Landlord {LandlordID}.", 
                    listingId, landlordUserId
                );

                await _listingRepository.DeleteListingPhoto(photoId, listingId, landlordUserId);
                
                _logger.LogInformation(
                    "Photo {PhotoID} Of listing {listingId} of Landlord {LandlordID} was deleted successfully.", 
                    photoId, listingId, landlordUserId
                );

                TempData["SuccessMessage"] = "Photo deleted successfully.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred while deleting photo {PhotoId} of Listing {listingId} of Landlord {LandlordUserId}.",
                    photoId, listingId, landlordUserId
                );

                TempData["ErrorMessage"] = "Unable to complete your request. Please try again later.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }
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
                await _listingRepository.SetPrimaryListingPhoto(listingId, photoId, landlordUserId);
                TempData["SuccessMessage"] = "Primary photo updated successfully.";

                _logger.LogInformation(
                    "Primary photo of Listing {ListingId} was updated successfully",
                    listingId
                );

                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An expected exception occurred while updating primary photo for listing {listingId}.",
                    listingId
                );
                TempData["ErrorMessage"] = "Unable to complete your request. Please try again later.";
                return RedirectToAction(nameof(ManageListingPhotos), new { listingId });
            }

            
        }
    }
}