using KasiRoomNetwork.Common.ViewModel.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Interfaces
{
    public interface IListingRepository
    {
        // sp_Create_Listing
        Task<int> CreateListing(CreateListingViewModel model,string landlordUserId);

        // sp_Delete_Listing
        Task DeleteListing(int listingId);

        // sp_Add_Listing_Photo
        Task<bool> AddListingPhoto(int listingId, string photoPath, bool isPrimary, string landlordUserId);

        // sp_Get_Listing_By_Id
        Task<ListingDetailsViewModel?> GetListingById(int listingId);
        Task<ListingDetailsViewModel?> GetListingDetailsById(int RoomId);

        // sp_Get_Listing_Photos
        Task<List<ListingPhotoViewModel>> GetListingPhotos(int listingId);

        //SP_Get_Listing_Photo_Count_By_Listing
        Task<int> GetListingPhotoCount(int listingId);

        // sp_Search_Listings
        Task<List<ListingSearchResultViewModel>> SearchListings(ListingSearchViewModel searchModel);
        Task<EditListingViewModel?> GetListingForEdit(int listingId, string landlordUserId);

        Task<bool> UpdateListing(EditListingViewModel model, string landlordUserId);
        Task<bool> DeleteListingPhoto(int photoId, int listingId, string landlordUserId);
        Task<bool> SetPrimaryListingPhoto(int listingId, int photoId, string landlordUserId);
    }
}
