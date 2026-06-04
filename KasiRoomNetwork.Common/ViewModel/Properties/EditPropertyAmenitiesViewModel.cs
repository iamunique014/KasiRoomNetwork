using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class EditPropertyAmenitiesViewModel
    {
        public int PropertyId { get; set; }

        public List<AmenitySelectionViewModel> Amenities { get; set; } = new();
    }
}
