using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public IActionResult AddListingPhotos(int listingId)
        {
            ViewBag.ListingId = listingId;
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
                return View();
            }

            // NOTE: actual file saving logic will be added in UI layer
            var filePath = "/uploads/listings/" + Guid.NewGuid() + Path.GetExtension(photo.FileName);

            await _listingRepository.AddListingPhoto(listingId, filePath, isPrimary);

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
                model.Results = await _listingRepository.SearchListings(model);
            }

            return View(model);
        }
    }
}
