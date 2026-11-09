using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.DTOs
{
    public class CreateEventDto
    {
        [Required]
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        [Required]
        public string Location { get; set; } = string.Empty;
        public string? Distance { get; set; }
        public string? EventType { get; set; } // run, walk, cycle
        public string? ImageUrl { get; set; }
    }

    public class EventDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Distance { get; set; }
        public string? EventType { get; set; }
        public int CreatedByUserId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
