using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KasiRoomNetwork.Data.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; }
    }
}
