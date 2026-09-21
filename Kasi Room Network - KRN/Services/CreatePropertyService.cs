using KasiRoomNetwork.Common.DTOs;
using Kasi_Room_Network___KRN.Services;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Common.ViewModel.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Kasi_Room_Network___KRN.Services
{
    public class CreatePropertyService : ICreatePropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAmenityRepository _amenityRepository;
        private readonly IPhotoStorageService _photoStorageService;
        private readonly IConfiguration _configuration;

        public CreatePropertyService(
            IPropertyRepository propertyRepository,
            IAmenityRepository amenityRepository,
            IPhotoStorageService photoStorageService,
            IConfiguration configuration)
        {
            _propertyRepository = propertyRepository;
            _amenityRepository = amenityRepository;
            _photoStorageService = photoStorageService;
            _configuration = configuration;
        }

        public async Task<int> CreatePropertyAsync(CreatePropertyWizardDto dto)
        {
            int propertyId = 0;
            IDbTransaction transaction = null;
            IDbConnection connection = null;

            var PermanentPropertyPhotoPaths = new List<string>();

            try
            {
                // Establish a single connection for the transaction
                connection = new SqlConnection(_configuration.GetConnectionString("conn"));
                connection.Open();
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
                //(Only the Last photo is set to primary by stored procedure)
                foreach (var tempPhotoPath in dto.TemporaryPhotoPaths)
                {
                    string permanentPath = await _photoStorageService.CopyTemporaryPhotoToPermanentAsync(tempPhotoPath, "properties");
                    PermanentPropertyPhotoPaths.Add(permanentPath);
                    bool isPrimary = true; //Every photo is set primary, this is changed in db by Sp.
                    await _propertyRepository.AddPropertyPhoto(propertyId, permanentPath, isPrimary, dto.LandlordUserId, transaction);
                }

                // Commit the transaction if all database operations are successful
                transaction.Commit();

                // Clean up temporary wizard photos after successful commit
                _photoStorageService.DeleteTemporaryWizardFolder(dto.LandlordUserId);
            }
            catch (Exception ex)
            {
                // Rollback transaction on any database-related error
                transaction?.Rollback();

                // Compensation: Delete any permanent blobs that were uploaded if the transaction failed
                foreach (var path in PermanentPropertyPhotoPaths)
                {
                    _photoStorageService.DeletePhoto(path);
                }

                // Re-throw the exception for upstream error handling
                throw;
            }
            finally
            {
                // Ensure the connection is closed
                connection?.Dispose();
            }

            return propertyId;
        }
    }
}