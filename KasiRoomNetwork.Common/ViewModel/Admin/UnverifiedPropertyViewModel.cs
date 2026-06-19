using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class UnverifiedPropertyViewModel
    {
        //Property
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int TotalRooms { get; set; }
        public DateTime CreatedAt { get; set; }
        //Address
        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }

        //Landlord
        public string LandlordUserId { get; set; }
        public string FullName { get; set; }
        public bool IsVerified { get; set; }
        public DateTime VerifiedAt { get; set; }
    }
}
