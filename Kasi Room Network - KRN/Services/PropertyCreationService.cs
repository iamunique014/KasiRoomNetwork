using KasiRoomNetwork.Common.DTOs;
using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Properties;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace KasiRoomNetwork.KRN.Services
{
    public class PropertyCreationService : IPropertyCreationService
    {
        private readonly ISqlDataAccess _db;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IPhotoStorageService _photoStorageService;
        private readonly IConfiguration _configuration;

        public PropertyCreationService(
            ISqlDataAccess db,
            IPropertyRepository propertyRepository,
            IAmenityRepository amenityRepository,
            IListingRepository listingRepository,
            IPhotoStorageService photoStorageService,
            IConfiguration configuration)
        {
            _db = db;
            _propertyRepository = propertyRepository;
            _amenityRepository = amenityRepository;
            _listingRepository = listingRepository;
            _photoStorageService = photoStorageService;
            _configuration = configuration;
        }

        public async Task<(int propertyId, int? listingId)> CreatePropertyAndListingAsync(PropertyListingCreationDto dto)
        {
            int propertyId = 0;
            int? listingId = null;
            IDbTransaction transaction = null;
            IDbConnection connection = null;

            List<string> permanentPhotoPaths = new List<string>();

            try
            {
                // Establish a single connection for the transaction
                connection = new SqlConnection(_configuration.GetConnectionString("conn"));
                await connection.OpenAsync();
                transaction = connection.BeginTransaction();

                // 1. Create Property
                var createPropertyViewModel = new CreatePropertyViewModel
                {
                    PropertyType = dto.PropertyType,
                    TotalRooms = dto.TotalRooms,
                    PropertyName = dto.PropertyName,
                    Street = dto.Street,
                    Province = dto.Province,
                    City = dto.City,
                    Suburb = dto.Suburb
                };
                propertyId = await _propertyRepository.CreateProperty(createPropertyViewModel, dto.LandlordUserId, transaction);

                // 2. Add Amenities
                foreach (var amenityId in dto.AmenityIds)
                {
                    await _amenityRepository.AddPropertyAmenity(propertyId, amenityId, dto.LandlordUserId, transaction);
                }

                // 3. Handle Photos (Move from temp to permanent storage and add metadata)
                foreach (var tempPhotoPath in dto.TemporaryPhotoPaths)
                {
                    string permanentPath = await _photoStorageService.CopyTemporaryPhotoToPermanentAsync(tempPhotoPath, propertyId);
                    permanentPhotoPaths.Add(permanentPath);
                    bool isPrimary = (tempPhotoPath == dto.PrimaryPhotoPath);
                    await _propertyRepository.AddPropertyPhoto(propertyId, permanentPath, isPrimary, dto.LandlordUserId, transaction);
                }

                // 4. Create Listing (if applicable)
                if (!string.IsNullOrEmpty(dto.ListingTitle))
                {
                    var createListingViewModel = new CreateListingViewModel
                    {
                        PropertyId = propertyId,
                        Title = dto.ListingTitle,
                        Description = dto.ListingDescription,
                        AvailableUnits = dto.AvailableUnits,
                        Price = dto.Price
                    };
                    listingId = await _listingRepository.CreateListing(createListingViewModel, dto.LandlordUserId, transaction);

                    // 5. Add Listing Photos (if applicable)
                    foreach (var selectedListingPhotoPath in dto.SelectedListingPhotoPaths)
                    {
                        // The photo is already in permanent storage (copied in step 3), just add metadata
                        bool isPrimaryListingPhoto = (selectedListingPhotoPath == dto.PrimaryPhotoPath);
                        await _listingRepository.AddListingPhoto(listingId.Value, selectedListingPhotoPath, isPrimaryListingPhoto, dto.LandlordUserId, transaction);
                    }
                }

                // Commit the transaction if all database operations are successful
                transaction.Commit();

                // Clean up temporary wizard photos after successful commit
                await _photoStorageService.DeleteTemporaryWizardFolder(dto.LandlordUserId);
            }
            catch (Exception ex)
            {
                // Rollback transaction on any database-related error
                transaction?.Rollback();

                // Compensation: Delete any permanent blobs that were uploaded if the transaction failed
                foreach (var path in permanentPhotoPaths)
                {
                    await _photoStorageService.DeletePhoto(path);
                }

                // Re-throw the exception for upstream error handling
                throw;
            }
            finally
            {
                // Ensure the connection is closed
                connection?.Dispose();
            }

            return (propertyId, listingId);
        }
    }
}
