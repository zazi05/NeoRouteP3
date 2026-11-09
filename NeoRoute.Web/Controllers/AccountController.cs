using Microsoft.AspNetCore.Mvc;
using NeoRoute.Web.Models;
using NeoRoute.Web.Services;
using System.Text.Json;

namespace NeoRoute.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, content, status) = await _api.PostAsync("/api/auth/login", model);

            if (success)
            {
                // Store basic user info in session
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("UserEmail", model.Email);

                // Try to get role from response if available
                try
                {
                    using var doc = JsonDocument.Parse(content ?? "{}");
                    if (doc.RootElement.TryGetProperty("role", out var roleProp))
                    {
                        HttpContext.Session.SetString("UserRole", roleProp.GetString() ?? "Participant");
                    }
                    else
                    {
                        HttpContext.Session.SetString("UserRole", "Participant");
                    }
                }
                catch
                {
                    HttpContext.Session.SetString("UserRole", "Participant");
                }

                var role = HttpContext.Session.GetString("UserRole");
                if (role == "Organiser")
                    return RedirectToAction("Dashboard", "Organiser");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, content, status) = await _api.PostAsync("/api/auth/register", model);

            if (success)
            {
                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", content ?? "Registration failed");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}