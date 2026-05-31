using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class EditPropertyViewModel
    {
        public int PropertyId { get; set; }

        // Property
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int? TotalRooms { get; set; }

        // Address
        public string Province { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string Street { get; set; }
    }
}
