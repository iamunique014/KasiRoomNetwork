using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.Models
{
    public class PropertyAmenityModel
    {
        public int PropertyAmenityId { get; set; }

        public int PropertyId { get; set; }

        public int AmenityId { get; set; }
    }
}
