using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace NeoRoute.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        private void AttachCookies()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            // Forward the API auth cookie if present
            if (context.Request.Cookies.TryGetValue("NeoRoute.Auth", out var authCookie))
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", $"NeoRoute.Auth={authCookie}");
            }
            else if (context.Session.GetString("AuthCookie") is string stored)
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", stored);
            }
        }

        public async Task<(bool Success, string? Content, HttpStatusCode StatusCode)> GetAsync(string endpoint)
        {
            AttachCookies();
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, content, response.StatusCode);
        }

        public async Task<(bool Success, string? Content, HttpStatusCode StatusCode)> PostAsync(string endpoint, object data)
        {
            AttachCookies();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Capture Set-Cookie from login responses
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx != null)
                {
                    foreach (var c in cookies)
                    {
                        if (c.StartsWith("NeoRoute.Auth="))
                        {
                            ctx.Session.SetString("AuthCookie", c.Split(';')[0]);
                            // Also set on the response so browser keeps it if needed
                            var value = c.Split('=')[1].Split(';')[0];
                            ctx.Response.Cookies.Append("NeoRoute.Auth", value, new CookieOptions
                            {
                                HttpOnly = true,
                                SameSite = SameSiteMode.Lax,
                                Expires = DateTimeOffset.UtcNow.AddHours(8)
                            });
                        }
                    }
                }
            }

            return (response.IsSuccessStatusCode, responseContent, response.StatusCode);
        }

        public async Task<(bool Success, string? Content, HttpStatusCode StatusCode)> PutAsync(string endpoint, object data)
        {
            AttachCookies();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, responseContent, response.StatusCode);
        }

        public async Task<(bool Success, string? Content, HttpStatusCode StatusCode)> DeleteAsync(string endpoint)
        {
            AttachCookies();
            var response = await _httpClient.DeleteAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, content, response.StatusCode);
        }

        public async Task<(bool Success, string? Content, HttpStatusCode StatusCode)> UploadFileAsync(string endpoint, IFormFile file)
        {
            AttachCookies();
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, responseContent, response.StatusCode);
        }
    }
}
