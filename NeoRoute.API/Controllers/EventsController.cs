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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        /// <summary>
        /// List all events (public / both roles).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            var events = await _context.Events
                .OrderBy(e => e.EventDate)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    Distance = e.Distance,
                    EventType = e.EventType,
                    CreatedByUserId = e.CreatedByUserId,
                    ImageUrl = e.ImageUrl
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>
        /// Get a single event by id.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var e = await _context.Events.FindAsync(id);
            if (e == null) return NotFound();

            return Ok(new EventDto
            {
                EventId = e.EventId,
                EventName = e.EventName,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType,
                CreatedByUserId = e.CreatedByUserId,
                ImageUrl = e.ImageUrl
            });
        }

        /// <summary>
        /// Create a new event (Organiser only).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var newEvent = new Event
            {
                EventName = dto.EventName.Trim(),
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location.Trim(),
                Distance = dto.Distance,
                EventType = dto.EventType,
                ImageUrl = dto.ImageUrl,
                CreatedByUserId = userId.Value
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, new EventDto
            {
                EventId = newEvent.EventId,
                EventName = newEvent.EventName,
                Description = newEvent.Description,
                EventDate = newEvent.EventDate,
                Location = newEvent.Location,
                Distance = newEvent.Distance,
                EventType = newEvent.EventType,
                CreatedByUserId = newEvent.CreatedByUserId,
                ImageUrl = newEvent.ImageUrl
            });
        }

        /// <summary>
        /// Update an existing event (Organiser only).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();

            existing.EventName = dto.EventName.Trim();
            existing.Description = dto.Description;
            existing.EventDate = dto.EventDate;
            existing.Location = dto.Location.Trim();
            existing.Distance = dto.Distance;
            existing.EventType = dto.EventType;
            if (dto.ImageUrl != null)
                existing.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully." });
        }

        /// <summary>
        /// Delete an event (Organiser only).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();

            _context.Events.Remove(existing);
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
