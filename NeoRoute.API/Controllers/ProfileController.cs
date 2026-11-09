using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoRoute.API.Data;
using NeoRoute.API.DTOs;
using System.Security.Claims;

namespace NeoRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ProfileController(RaceDayContext context)
        {
            _context = context;
        }

        /// <summary>
        /// View own profile (works for both Organiser and Participant).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            return Ok(new ProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Age = user.Participant?.Age,
                Gender = user.Participant?.Gender,
                ParticipantId = user.Participant?.ParticipantId,
                ProfilePictureUrl = user.ProfilePictureUrl
            });
        }

        /// <summary>
        /// Update own profile information.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Participant)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            if (dto.ProfilePictureUrl != null)
                user.ProfilePictureUrl = dto.ProfilePictureUrl;

            if (user.Participant != null)
            {
                user.Participant.FirstName = user.FirstName;
                user.Participant.LastName = user.LastName;
                user.Participant.Age = dto.Age;
                user.Participant.Gender = dto.Gender;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}
