using KasiRoomNetwork.Common.ViewModel.Admin;
using KasiRoomNetwork.Data.Domain.Models;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IAdminRepository adminRepository, UserManager<ApplicationUser> userManager)
        {
            _adminRepository = adminRepository;
            _userManager = userManager;
        }

        // ===============================
        // Admin Dashboard
        // ===============================

        public IActionResult AdminDashboard()
        {
            return View();
        }

        // ===============================
        // Verification Queue
        // ===============================
        public async Task<IActionResult> UnverifiedListings()
        {
            var listings = await _adminRepository.GetUnverifiedListingsAsync();
            return View(listings);
        }

        // ===============================
        // Review Listing
        // ===============================
        public async Task<IActionResult> ReviewListing(int id)
        {
            var listing = await _adminRepository.GetListingForVerificationAsync(id);

            if (listing == null)
                return NotFound();

            listing.Photos = (await _adminRepository.GetListingPhotosAsync(id)).ToList();

            return View(listing);
        }

        // ===============================
        // Approve / Reject Listing
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyListing(VerifyListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ReviewListing), new { id = model.ListingId });
            }

            var adminUserId = _userManager.GetUserId(User);

            await _adminRepository.VerifyListingAsync(
                model.ListingId,
                adminUserId,
                model.IsApproved,
                model.Notes
            );

            TempData["Success"] = model.IsApproved
                ? "Listing approved successfully."
                : "Listing rejected successfully.";

            return RedirectToAction(nameof(UnverifiedListings));
        }

        // ===============================
        // Verification Logs (Optional View)
        // ===============================
        public async Task<IActionResult> VerificationLogs(int listingId)
        {
            var logs = await _adminRepository.GetVerificationLogsAsync();
            return View(logs);
        }
    }
}
