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

        // sp_Add_Listing_Photo
        Task AddListingPhoto(int listingId, string photoPath, bool isPrimary);

        // sp_Get_Listing_By_Id
        Task<ListingDetailsViewModel?> GetListingById(int listingId);

        // sp_Get_Listing_Photos
        Task<List<ListingPhotoViewModel>> GetListingPhotos(int listingId);

        // sp_Search_Listings
        Task<List<ListingSearchResultViewModel>> SearchListings(ListingSearchViewModel searchModel);
    }
}
