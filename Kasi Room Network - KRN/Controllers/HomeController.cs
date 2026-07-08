using System.Diagnostics;
using Kasi_Room_Network___KRN.Models;
using KasiRoomNetwork.Data.Interfaces;
using KasiRoomNetwork.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Kasi_Room_Network___KRN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _logger = logger;
            _homeRepository = homeRepository;

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FeaturedProperties()
        {
            var properties =
                await _homeRepository.GetFeaturedPropertiesAsync();

            return PartialView(
                "_FeaturedProperties",
                properties);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
