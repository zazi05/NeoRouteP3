using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required, MaxLength(200)]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Distance { get; set; }

        [MaxLength(20)]
        public string? EventType { get; set; } // run, walk, cycle

        public int CreatedByUserId { get; set; }

        public string? ImageUrl { get; set; }

        // Navigation
        public User CreatedBy { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}
