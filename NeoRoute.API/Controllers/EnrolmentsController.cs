using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoRoute.API.Data;
using NeoRoute.API.DTOs;
using NeoRoute.API.Models;
using System.Security.Claims;

namespace NeoRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Organiser: view all enrolments (optionally filter by eventId).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetAll([FromQuery] int? eventId)
        {
            var query = _context.Enrollments
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(e => e.EventId == eventId.Value);

            var list = await query
                .Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    EventId = e.EventId,
                    EventName = e.Event.EventName,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.Participant.FirstName + " " + e.Participant.LastName,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.CategoryName,
                    RegistrationDate = e.RegistrationDate,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Participant: view own enrolments.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrolments()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return NotFound(new { message = "Participant profile not found." });

            var list = await _context.Enrollments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantId == participant.ParticipantId)
                .Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    EventId = e.EventId,
                    EventName = e.Event.EventName,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = participant.FirstName + " " + participant.LastName,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.CategoryName,
                    RegistrationDate = e.RegistrationDate,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Participant: enrol in an event by selecting a category.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrollmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return BadRequest(new { message = "Only Participants can enrol." });

            // Check event and category exist
            if (!await _context.Events.AnyAsync(e => e.EventId == dto.EventId))
                return NotFound(new { message = "Event not found." });

            if (!await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
                return NotFound(new { message = "Category not found." });

            // Prevent duplicate enrolment
            if (await _context.Enrollments.AnyAsync(e =>
                    e.EventId == dto.EventId && e.ParticipantId == participant.ParticipantId))
                return Conflict(new { message = "You are already enrolled in this event." });

            var enrollment = new Enrollment
            {
                EventId = dto.EventId,
                ParticipantId = participant.ParticipantId,
                CategoryId = dto.CategoryId,
                RegistrationDate = DateTime.UtcNow.Date,
                Status = "Pending"
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), new
            {
                enrollmentId = enrollment.EnrollmentId,
                message = "Successfully enrolled."
            });
        }

        /// <summary>
        /// Cancel an enrolment (Participant can cancel own; Organiser can cancel any).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null) return NotFound();

            var userId = GetCurrentUserId();
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Participant")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (participant == null || enrollment.ParticipantId != participant.ParticipantId)
                    return Forbid();
            }
            // Organiser can delete any

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Organiser: update enrolment status (Pending, Confirmed, Cancelled, Waitlisted).
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrollmentStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var allowed = new[] { "Pending", "Confirmed", "Cancelled", "Waitlisted" };
            if (!allowed.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = "Status must be one of: Pending, Confirmed, Cancelled, Waitlisted." });

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            enrollment.Status = dto.Status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Status updated.", status = enrollment.Status });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}
