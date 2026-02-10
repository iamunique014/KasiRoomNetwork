using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Profiles;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ISqlDataAccess _db;

        public ProfileRepository(ISqlDataAccess db)
        {
            _db = db;
        }
        public async Task<ProfileViewModel?> GetByUserId(string userId)
        {
            var result = await _db.GetData<ProfileViewModel, dynamic>(
                "sp_Profile_Get_By_UserId",
                new { UserId = userId});

            return result.FirstOrDefault();
        }
        public async Task<bool> Exists(string userId)
        {
            var result = await _db.GetData<int, dynamic>(
                "sp_Profile_Get_By_UserId",
                new { UserId = userId });

            return result.Any();
        }

        public async Task SaveProfile(ProfileViewModel profile, string userId)
        {
            await _db.SaveData("sp_Profile_Upsert", new
            {
                userId,
                profile.FullName,
                profile.PhoneNumber,
                profile.City,
                profile.Province
            });
        }

        public async Task<LandlordPublicProfileViewModel?> GetLandlordPublic(string userId)
        {
            var result = await _db.GetData<LandlordPublicProfileViewModel, dynamic>(
               "sp_Profile_LandlordProfile_Public",
               new { UserId = userId });

            return result.FirstOrDefault();
        }
    }
}
