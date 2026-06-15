using KasiRoomNetwork.Common.ViewModel.Listings;
using KasiRoomNetwork.Common.ViewModel.Profiles;
using KasiRoomNetwork.Data.DataAccess;
using KasiRoomNetwork.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KasiRoomNetwork.Data.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ISqlDataAccess _db;

        public ProfileRepository(ISqlDataAccess db)
        {
            _db = db;
        }
        public async Task<ProfilePageViewModel?> GetByUserId(string userId)
        {
            var result = await _db.GetMultiData<ProfileViewModel, LandlordPublicProfileViewModel, ProfilePageViewModel>(
                "sp_Profile_Get_By_UserId",
                (user, landlord) =>
                {
                    return new ProfilePageViewModel
                    {
                        UserProfile = user,
                        LandlordProfile = landlord
                    };
                },
                new { UserId = userId },
                splitOn: "LandlordSplit");

            return result.FirstOrDefault();
        }
        //public async Task<bool> Exists(string userId)
        //{
        //    var result = await _db.GetData<int, dynamic>(
        //        "sp_Profile_Get_By_UserId",
        //        new { UserId = userId });

        //    return result.Any();
        //}

        public async Task SaveProfile(ProfilePageViewModel profile, string userId)
        {
            await _db.SaveData("sp_Profile_Upsert", new
            {
                userId,
                profile.UserProfile.FullName,
                profile.UserProfile.PhoneNumber,
                profile.UserProfile.City,
                profile.UserProfile.Province
            });
        }

        public async Task SaveLandlordProfile(ProfilePageViewModel profile, string userId)
        {
            await _db.SaveData("sp_LandlordProfile_Create", new
            {
                userId,
                profile.UserProfile.FullName,
                profile.UserProfile.PhoneNumber,
                profile.LandlordProfile.WhatsAppNumber,
                profile.UserProfile.City,
                profile.UserProfile.Province,
                profile.LandlordProfile.Bio
            });
        }

        public async Task<bool> IsComplete(string userId)
        {
            var profile = await GetByUserId(userId);
            if (profile == null)
                return false;

            return !string.IsNullOrWhiteSpace(profile.UserProfile.FullName)
                && !string.IsNullOrWhiteSpace(profile.UserProfile.PhoneNumber)
                && !string.IsNullOrWhiteSpace(profile.UserProfile.City)
                && !string.IsNullOrWhiteSpace(profile.UserProfile.Province);
        }

        //public async Task<LandlordPublicProfileViewModel?> GetLandlordPublic(string userId)
        //{
        //    var result = await _db.GetData<LandlordPublicProfileViewModel, dynamic>(
        //       "sp_Profile_LandlordProfile_Public",
        //       new { UserId = userId });

        //    return result.FirstOrDefault();
        //}
    }
}
