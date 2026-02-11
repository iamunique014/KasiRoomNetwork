using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class TenantController : Controller
    {
        [HttpGet]
        public IActionResult TenantDashboard()
        {
            return View();
        }
    }
}
