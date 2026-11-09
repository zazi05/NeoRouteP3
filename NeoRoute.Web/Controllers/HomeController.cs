using Microsoft.AspNetCore.Mvc;
using NeoRoute.Web.Services;

namespace NeoRoute.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var (success, content, _) = await _api.GetAsync("/api/events");
            ViewBag.EventsJson = success ? content : "[]";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}