using KasiRoomNetwork.Common.ViewModel.Admin;
using KasiRoomNetwork.Common.ViewModel.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IAdminRepository
    {
        // ===== Listings Verification =====

        Task<IEnumerable<UnverifiedListingViewModel>>
            GetUnverifiedListingsAsync();

        Task<AdminListingReviewViewModel>
            GetListingForVerificationAsync(int listingId);

        Task<IEnumerable<ListingPhotoViewModel>>
            GetListingPhotosAsync(int listingId);

        Task VerifyListingAsync(
            int listingId, 
            string adminUserId,
            bool isApproved,
            string notes );

        // ===== Property Verification =====

        Task<IEnumerable<UnverifiedPropertyViewModel>> 
            GetUnverifiedPropertiesAsync();

        // ===== Landlord Verification =====
        Task<IEnumerable<UnverifiedLandlordViewModel>>
            GetUnverifiedLandlordsAsync();

        // ===== Audit Logs =====
        Task<IEnumerable<AdminVerificationLogViewModel>> 
            GetVerificationLogsAsync();

        // ===== User Management =====
        Task<IEnumerable<ManageUsersViewModel>> 
            GetUsersAsync();
    }
}
