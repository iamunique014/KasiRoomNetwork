using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        // sp_Listing_Delete
        public async Task DeleteListing(int listingId)
        {
            if (listingId <= 0)
            {
                return;
            }

            var listing = await GetListingById(listingId);
            if (listing == null)
            {
                return;
            }

            await _db.SaveData("sp_Listing_Delete", new
            {
                ListingId = listingId
            });
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

        public async Task<EditListingViewModel?> GetListingForEdit(int listingId, string landlordUserId)
        {
            var results = await _db.GetData<EditListingViewModel, dynamic>(
               "sp_Listing_Get_For_Edit",
               new
               {
                   ListingId = listingId,
                   LandlordUserId = landlordUserId
               });

            return results.FirstOrDefault();
        }


        public async Task UpdateListing(EditListingViewModel model, string landlordUserId)
        {
            await _db.SaveData(
                "sp_Listing_Update",
                new
                {
                    model.ListingId,
                    LandlordUserId = landlordUserId,
                    model.Title,
                    model.Price,
                    model.Description,
                    model.IsAvailable
                });
        }
        public async Task DeleteListingPhoto(int photoId, int listingId)
        {
            await _db.SaveData("sp_ListingPhoto_Delete", new
            {
                PhotoId = photoId,
                ListingId = listingId
            });
        }
        public async Task SetPrimaryListingPhoto(int listingId, int photoId)
        {
            await _db.SaveData("sp_ListingPhoto_Set_Primary", new
            {
                ListingId = listingId,
                PhotoId = photoId
            });
        }
    }
}
