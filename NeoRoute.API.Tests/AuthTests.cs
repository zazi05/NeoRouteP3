using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NeoRoute.API.DTOs;
using Xunit;

namespace NeoRoute.API.Tests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient()
        {
            return _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        }

        [Fact]
        public async Task Register_ValidOrganiser_Returns201()
        {
            var client = CreateClient();
            var dto = new RegisterDto
            {
                Email = $"org_{Guid.NewGuid():N}@test.com",
                Password = "Password123!",
                FirstName = "Alice",
                LastName = "Organiser",
                Role = "Organiser"
            };

            var response = await client.PostAsJsonAsync("/api/Auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_ValidParticipant_Returns201()
        {
            var client = CreateClient();
            var dto = new RegisterDto
            {
                Email = $"part_{Guid.NewGuid():N}@test.com",
                Password = "Password123!",
                FirstName = "Bob",
                LastName = "Runner",
                Role = "Participant",
                Age = 28,
                Gender = "Male"
            };

            var response = await client.PostAsJsonAsync("/api/Auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            var client = CreateClient();
            var dto = new RegisterDto
            {
                Email = $"bad_{Guid.NewGuid():N}@test.com",
                Password = "Password123!",
                FirstName = "Bad",
                LastName = "Role",
                Role = "Admin"
            };

            var response = await client.PostAsJsonAsync("/api/Auth/register", dto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200()
        {
            var client = CreateClient();
            var email = $"login_{Guid.NewGuid():N}@test.com";
            var password = "Password123!";

            var reg = await client.PostAsJsonAsync("/api/Auth/register", new RegisterDto
            {
                Email = email,
                Password = password,
                FirstName = "Test",
                LastName = "User",
                Role = "Organiser"
            });
            Assert.Equal(HttpStatusCode.Created, reg.StatusCode);

            var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
            {
                Email = email,
                Password = password
            });

            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Expected 200 but got {(int)response.StatusCode}. Body: {body}");
            Assert.Contains("Login successful", body);
        }

        [Fact]
        public async Task Login_InvalidPassword_Returns401()
        {
            var client = CreateClient();
            var email = $"fail_{Guid.NewGuid():N}@test.com";

            await client.PostAsJsonAsync("/api/Auth/register", new RegisterDto
            {
                Email = email,
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Organiser"
            });

            var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
            {
                Email = email,
                Password = "WrongPassword!"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_WhenNotLoggedIn_Returns401()
        {
            var client = CreateClient();
            var response = await client.GetAsync("/api/Auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
