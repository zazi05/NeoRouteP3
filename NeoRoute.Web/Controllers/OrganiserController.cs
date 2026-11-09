using Microsoft.AspNetCore.Mvc;
using NeoRoute.Web.Services;
using System.Text.Json;

namespace NeoRoute.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api)
        {
            _api = api;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("IsLoggedIn") == "true"
                && HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var (success, content, _) = await _api.GetAsync("/api/events");
            ViewBag.EventsJson = success ? content : "[]";
            return View();
        }

        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var (success, content, _) = await _api.GetAsync("/api/events");
            ViewBag.EventsJson = success ? content : "[]";
            return View();
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string eventName, string description, DateTime eventDate, string location, string distance, string eventType, IFormFile? banner)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            string? imageUrl = null;
            if (banner != null && banner.Length > 0)
            {
                var (upSuccess, upContent, _) = await _api.UploadFileAsync("/api/files/upload", banner);
                if (upSuccess && upContent != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(upContent);
                        if (doc.RootElement.TryGetProperty("url", out var urlProp))
                            imageUrl = urlProp.GetString();
                    }
                    catch { /* ignore parse errors */ }
                }
                else
                {
                    TempData["Error"] = "Banner upload failed. Event will be created without image. " + (upContent ?? "");
                }
            }

            var data = new
            {
                EventName = eventName,
                Description = description,
                EventDate = eventDate,
                Location = location,
                Distance = distance,
                EventType = eventType,
                ImageUrl = imageUrl
            };

            var (success, content, _) = await _api.PostAsync("/api/events", data);

            if (success)
            {
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Events");
            }

            TempData["Error"] = content ?? "Failed to create event";
            return View();
        }

        public async Task<IActionResult> Enrolments()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var (success, content, _) = await _api.GetAsync("/api/enrolments");
            ViewBag.EnrolmentsJson = success ? content : "[]";
            return View();
        }

        public async Task<IActionResult> Results()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            return View();
        }
    }
}
