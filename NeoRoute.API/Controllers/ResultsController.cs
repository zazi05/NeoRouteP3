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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List results (optionally filter by eventId). Public / both roles.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResults([FromQuery] int? eventId)
        {
            var query = _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Event)
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(r => r.Enrollment.EventId == eventId.Value);

            var list = await query
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrollmentId = r.EnrollmentId,
                    FinishTime = r.FinishTime,
                    Position = r.Position,
                    ParticipantName = r.Enrollment.Participant.FirstName + " " + r.Enrollment.Participant.LastName,
                    EventName = r.Enrollment.Event.EventName
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Participant: view own results.
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetMyResults()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (participant == null)
                return NotFound(new { message = "Participant profile not found." });

            var list = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e.Event)
                .Where(r => r.Enrollment.ParticipantId == participant.ParticipantId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrollmentId = r.EnrollmentId,
                    FinishTime = r.FinishTime,
                    Position = r.Position,
                    ParticipantName = participant.FirstName + " " + participant.LastName,
                    EventName = r.Enrollment.Event.EventName
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Organiser: capture finish time and position for an enrolment.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var enrollment = await _context.Enrollments.FindAsync(dto.EnrollmentId);
            if (enrollment == null)
                return NotFound(new { message = "Enrollment not found." });

            if (await _context.Results.AnyAsync(r => r.EnrollmentId == dto.EnrollmentId))
                return Conflict(new { message = "Result already exists for this enrolment." });

            var result = new Result
            {
                EnrollmentId = dto.EnrollmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResults), new { resultId = result.ResultId }, result);
        }

        /// <summary>
        /// Organiser: update a result.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] CreateResultDto dto)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Result updated." });
        }

        /// <summary>
        /// Organiser: delete a result.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out int id) ? id : null;
        }
    }
}
