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
    public class ListingRepository : IListingRepository
    {
        private readonly ISqlDataAccess _db;

        public ListingRepository(ISqlDataAccess db)
        {
            _db = db;
        }

        // sp_Create_Listing
        public async Task<int> CreateListing(CreateListingViewModel model,string landlordUserId)
        {
            var result = await _db.GetData<int, dynamic>("sp_Listing_Create_Listing", new
            {
                LandlordUserId = landlordUserId,
                model.PropertyId,
                model.Title,
                model.Description,
                model.Price
            });

            return result.First();
        }

        // sp_Add_Listing_Photo
        public async Task AddListingPhoto(int listingId, string photoPath, bool isPrimary)
        {
            await _db.SaveData("sp_Listing_Add_Listing_Photo", new
            {
                ListingId = listingId,
                PhotoPath = photoPath,
                IsPrimary = isPrimary
            });
        }

        // sp_Get_Listing_By_Id
        public async Task<ListingDetailsViewModel?> GetListingById(int listingId)
        {
            var result = await _db.GetData<ListingDetailsViewModel, dynamic>(
                "sp_Listing_Get_Listing_By_Id",
                new { ListingId = listingId });

            return result.FirstOrDefault();
        }

        // sp_Get_Listing_Photos
        public async Task<List<ListingPhotoViewModel>> GetListingPhotos(int listingId)
        {
            var result = await _db.GetData<ListingPhotoViewModel, dynamic>(
                "sp_Listing_Get_Listing_Photos",
                new { ListingId = listingId });

            return result.ToList();
        }

        // sp_Search_Listings
        public async Task<List<ListingSearchResultViewModel>> SearchListings(ListingSearchViewModel searchModel)
        {
            var result = await _db.GetData<ListingSearchResultViewModel, dynamic>("sp_Listing_Search_Listings", new
            {
                searchModel.Province,
                searchModel.City,
                searchModel.Suburb,
                searchModel.MinPrice,
                searchModel.MaxPrice
            });

            return result.ToList();
        }

        public async Task<int> GetListingPhotoCount(int listingId)
        {
            var result = await _db.GetData<int, dynamic>("sp_Listing_Get_Photo_Count_By_Listing", new 
            { 
                ListingId = listingId 
            });

            return result.FirstOrDefault();
        }
    }
}
