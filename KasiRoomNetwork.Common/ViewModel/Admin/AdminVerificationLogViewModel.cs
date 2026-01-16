using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Common.ViewModel.Admin
{
    public class AdminVerificationLogViewModel
    {
        public int LogId { get; set; }
        public string AdminUser { get; set; }
        public string Action { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
