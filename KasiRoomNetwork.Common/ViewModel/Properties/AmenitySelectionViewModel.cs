using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Properties
{
    public class AmenitySelectionViewModel
    {
        public int AmenityId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public string? Icon { get; set; }

        public bool IsSelected { get; set; }
    }
}
