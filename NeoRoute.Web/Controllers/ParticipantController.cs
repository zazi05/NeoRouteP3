using Microsoft.AspNetCore.Mvc;
using NeoRoute.Web.Services;
using System.Text.Json;

namespace NeoRoute.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;

        public ParticipantController(ApiService api)
        {
            _api = api;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("IsLoggedIn") == "true"
                && HttpContext.Session.GetString("UserRole") == "Participant";
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("IsLoggedIn") == "true";
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var (success, content, _) = await _api.GetAsync("/api/enrolments/my");
            ViewBag.EnrolmentsJson = success ? content : "[]";
            return View();
        }

        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var (success, content, _) = await _api.GetAsync("/api/results/my");
            ViewBag.ResultsJson = success ? content : "[]";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var (success, content, _) = await _api.GetAsync("/api/profile");
            if (success && content != null)
            {
                ViewBag.ProfileJson = content;
            }
            else
            {
                ViewBag.ProfileJson = "{}";
                ViewBag.Error = "Could not load profile.";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string firstName, string lastName, int? age, string? gender, IFormFile? profilePicture)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            string? pictureUrl = null;
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var (upSuccess, upContent, _) = await _api.UploadFileAsync("/api/files/upload", profilePicture);
                if (upSuccess && upContent != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(upContent);
                        if (doc.RootElement.TryGetProperty("url", out var urlProp))
                            pictureUrl = urlProp.GetString();
                    }
                    catch { }
                }
            }

            var data = new
            {
                FirstName = firstName,
                LastName = lastName,
                Age = age,
                Gender = gender,
                ProfilePictureUrl = pictureUrl
            };

            var (success, content, _) = await _api.PutAsync("/api/profile", data);
            if (success)
            {
                TempData["Success"] = "Profile updated successfully.";
            }
            else
            {
                TempData["Error"] = content ?? "Update failed.";
            }
            return RedirectToAction("Profile");
        }
    }
}
