using KasiRoomNetwork.Common.ViewModel.Admin;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;
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
        // Unverified 
        // ===============================
        public async Task<IEnumerable<UnverifiedListingViewModel>> 
            GetUnverifiedListingsAsync()
        {
            return await _db.GetData<UnverifiedListingViewModel, dynamic>(
                "sp_Admin_Get_Unverified_Listings",
                new { }
            );
        }

        public async Task<IEnumerable<UnverifiedPropertyViewModel>> 
            GetUnverifiedPropertiesAsync()
        {
            return await _db.GetData<UnverifiedPropertyViewModel, dynamic>(
                "sp_Admin_Get_Unverified_Properties",
                new { }
            );
        }

        public async Task<IEnumerable<UnverifiedLandlordViewModel>>
            GetUnverifiedLandlordsAsync()
        {
            return await _db.GetData<UnverifiedLandlordViewModel, dynamic>(
                "sp_Admin_Get_Unverified_Landlords",
                new { }
            );
        }

        // ===============================
        // Details for Verification
        // ===============================
        public async Task<AdminListingReviewViewModel> 
            GetListingForVerificationAsync(int listingId)
        {
            var result = await _db
                .GetData<AdminListingReviewViewModel, dynamic>(
                    "sp_Admin_Get_Listing_For_Verification",
                    new { ListingId = listingId }
                );

            return result.FirstOrDefault();
        }

        public async Task<AdminLandlordReviewViewModel>
            GetLandlordForVerificationAsync(string landlordUserId)
        {
            var result =
                await _db.GetData<AdminLandlordReviewViewModel, dynamic>(
                    "sp_Admin_Get_Landlord_For_Verification",
                    new
                    {
                        LandlordUserId = landlordUserId
                    });

            return result.FirstOrDefault();
        }

        public async Task<AdminPropertyReviewViewModel>
            GetPropertyForVerificationAsync(int propertyId)
        {
            var result =
                await _db.GetData<AdminPropertyReviewViewModel, dynamic>(
                    "sp_Admin_Get_Property_For_Verification",
                    new
                    {
                        PropertyId = propertyId
                    });

            return result.FirstOrDefault();
        }

        

        // ===============================
        // Photos
        // ===============================
        public async Task<IEnumerable<ListingPhotoViewModel>> 
            GetListingPhotosAsync(int listingId)
        {
            return await _db.GetData<ListingPhotoViewModel, dynamic>(
                "sp_Admin_Get_Listing_Photos",
                new { ListingId = listingId }
            );
        }

        public async Task<IEnumerable<PropertyPhotoViewModel>>
            GetPropertyPhotosAsync(int propertyId)
        {
            return await _db.GetData<PropertyPhotoViewModel, dynamic>(
                "sp_Admin_Get_Property_Photos",
                new
                {
                    PropertyId = propertyId
                });
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

        public async Task VerifyLandlordAsync(
            string landlordUserId,
            string adminUserId,
            bool isApproved,
            string notes)
        {
            await _db.SaveData(
                "sp_Admin_Verify_Landlord",
                new
                {
                    LandlordUserId = landlordUserId,
                    AdminUserId = adminUserId,
                    IsApproved = isApproved,
                    Notes = notes
                });
        }

        public async Task VerifyPropertyAsync(
            int propertyId,
            string adminUserId,
            bool isApproved,
            string notes)
        {
            await _db.SaveData(
                "sp_Admin_Verify_Property",
                new
                {
                    PropertyId = propertyId,
                    AdminUserId = adminUserId,
                    IsApproved = isApproved,
                    Notes = notes
                });
        }

        // ===============================
        // Verification Logs
        // ===============================
        public async Task<IEnumerable<AdminVerificationLogViewModel>> 
            GetVerificationLogsAsync()
        {
            return await _db.GetData<AdminVerificationLogViewModel, dynamic>(
                "sp_Admin_Get_Verification_Logs",
                new
                { }
            );
        }

        // ===============================
        // User Management - Get All
        // ===============================

        public async Task<IEnumerable<ManageUsersViewModel>> 
            GetUsersAsync()
        {
            return await _db.GetData<ManageUsersViewModel, dynamic>(
                 "sp_Admin_Get_Users",
                 new
                 { }
            );
        }
    }
}
