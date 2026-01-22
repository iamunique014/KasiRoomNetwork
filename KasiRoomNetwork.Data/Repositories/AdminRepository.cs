using KasiRoomNetwork.Common.ViewModel.Admin;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ISqlDataAccess _db;

        public AdminRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        // ===============================
        // Unverified Listings
        // ===============================
        public async Task<IEnumerable<UnverifiedListingViewModel>> GetUnverifiedListingsAsync()
        {
            return await _db.GetData<UnverifiedListingViewModel, dynamic>(
                "sp_Admin_Get_Unverified_Listings",
                new { }
            );
        }

        // ===============================
        // Listing Details for Verification
        // ===============================
        public async Task<AdminListingReviewViewModel> GetListingForVerificationAsync(int listingId)
        {
            var result = await _db.GetData<AdminListingReviewViewModel, dynamic>(
                "sp_Admin_Get_Listing_For_Verification",
                new { ListingId = listingId }
            );

            return result.FirstOrDefault();
        }

        // ===============================
        // Listing Photos
        // ===============================
        public async Task<IEnumerable<ListingPhotoViewModel>> GetListingPhotosAsync(int listingId)
        {
            return await _db.GetData<ListingPhotoViewModel, dynamic>(
                "sp_Admin_Get_Listing_Photos",
                new { ListingId = listingId }
            );
        }

        // ===============================
        // Approve / Reject Listing
        // ===============================
        public async Task VerifyListingAsync(
            int listingId,
            string adminUserId,
            bool isApproved,
            string notes
        )
        {
            await _db.SaveData(
                "sp_Admin_Verify_Listing",
                new
                {
                    ListingId = listingId,
                    AdminUserId = adminUserId,
                    IsApproved = isApproved,
                    Notes = notes
                }
            );
        }

        // ===============================
        // Verification Logs
        // ===============================
        public async Task<IEnumerable<AdminVerificationLogViewModel>> GetVerificationLogsAsync()
        {
            return await _db.GetData<AdminVerificationLogViewModel, dynamic>(
                "sp_Admin_Get_Verification_Logs",
                new
                { }
            );
        }
    }
}
