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

        public AdminController(
            IAdminRepository 
            adminRepository, 
            UserManager<ApplicationUser> 
            userManager)
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
        public async Task<IActionResult> UnverifiedProperties()
        {
            var properties = await _adminRepository.GetUnverifiedPropertiesAsync();
            return View(properties);
        }
        public async Task<IActionResult> UnverifiedLandlords()
        {
            var landlords = await _adminRepository.GetUnverifiedLandlordsAsync();
            return View(landlords);
        }
        // ===============================
        // Review
        // ===============================
        public async Task<IActionResult> ReviewListing(int id)
        {
            var listing = await _adminRepository.GetListingForVerificationAsync(id);

            if (listing == null)
                return NotFound();

            listing.Photos = (await _adminRepository
                .GetListingPhotosAsync(id)).ToList();

            return View(listing);
        }
        public async Task<IActionResult> ReviewLandlord(string id, int landlordId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var landlord =
                await _adminRepository
                .GetLandlordForVerificationAsync(id, landlordId);

            if (landlord == null)
            {
                return NotFound();
            }

            return View(landlord);
        }

        public async Task<IActionResult> ReviewProperty(int id)
        {
            var property =
                await _adminRepository.GetPropertyForVerificationAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            property.Photos =
                (await _adminRepository
                    .GetPropertyPhotosAsync(id))
                    .ToList();

            return View(property);
        }

        // ===============================
        // Approve / Reject 
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyListing(VerifyListingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(ReviewListing), new { id = model.ListingId });
            }

            if (!model.IsApproved &&
                string.IsNullOrWhiteSpace(model.Notes))
            {
                TempData["Error"] =
                    "Rejection notes are required.";

                return RedirectToAction(
                    nameof(ReviewListing),
                    new { id = model.ListingId });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyLandlord(
            VerifyLandlordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(ReviewLandlord),
                    new { id = model.LandlordUserId });
            }

            if (!model.IsApproved &&
                string.IsNullOrWhiteSpace(model.Notes))
            {
                TempData["Error"] =
                    "Rejection notes are required.";

                return RedirectToAction(
                    nameof(ReviewLandlord),
                    new { id = model.LandlordUserId });
            }

            var adminUserId = _userManager.GetUserId(User);

            await _adminRepository.VerifyLandlordAsync(
                model.LandlordUserId,
                model.LandlordProfileId,
                adminUserId,
                model.IsApproved,
                model.Notes);

            TempData["Success"] = model.IsApproved
                ? "Landlord verified successfully."
                : "Landlord rejected successfully.";

            return RedirectToAction(nameof(UnverifiedLandlords));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyProperty(
            VerifyPropertyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(ReviewProperty),
                    new { id = model.PropertyId });
            }

            var adminUserId = _userManager.GetUserId(User);

            await _adminRepository.VerifyPropertyAsync(
                model.PropertyId,
                adminUserId,
                model.IsApproved,
                model.Notes
            );

            TempData["Success"] = model.IsApproved
                ? "Property approved successfully."
                : "Property rejected successfully.";

            return RedirectToAction(nameof(UnverifiedProperties));
        }


        // ===============================
        // Verification Logs 
        // ===============================
        public async Task<IActionResult> VerificationLogs(int listingId)
        {
            var logs = await _adminRepository.GetVerificationLogsAsync();
            return View(logs);
        }

        // ===============================
        // User Management - GET ALL USER
        // ===============================

        public async Task<IActionResult> UserManagement()
        {
            var users = await _adminRepository.GetUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var user =
                await _adminRepository
                    .GetUserDetailsAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var currentAdminId = _userManager.GetUserId(User);

            if (currentAdminId == id)
            {
                TempData["Error"] =
                    "You cannot block your own account.";

                return RedirectToAction(
                    nameof(UserDetails),
                    new { id });
            }

            var user =
                await _userManager.FindByIdAsync(id);



            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEnabledAsync(
                user,
                true);

            await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.MaxValue);

            TempData["Success"] =
                "User blocked successfully.";

            return RedirectToAction(
                nameof(UserDetails),
                new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.SetLockoutEndDateAsync(
                user,
                null);

            TempData["Success"] =
                "User unblocked successfully.";

            return RedirectToAction(
                nameof(UserDetails),
                new { id });
        }
    }
}
