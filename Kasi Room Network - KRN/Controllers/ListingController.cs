using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class ListingController : Controller
    {
        private readonly IListingRepository _listingRepository;

        public ListingController(IListingRepository listingRepository)
        {
            _listingRepository = listingRepository;
        }

        // =========================
        // CREATE LISTING
        // =========================

        [Authorize(Roles = "Landlord")]
        [HttpGet]
        public IActionResult CreateListing()
        {
            return View();
        }

        [Authorize(Roles = "Landlord")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListing(CreateListingViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var landlordUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
        public async Task<IActionResult> ListingDetails(int id)
        {
            var listing = await _listingRepository.GetListingById(id);
            if (listing == null)
                return NotFound();

            listing.Photos = await _listingRepository.GetListingPhotos(id);

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
    }
}
