using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.Models
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }

        public int EventId { get; set; }
        public int ParticipantId { get; set; }
        public int CategoryId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow.Date;

        /// <summary>
        /// Pending, Confirmed, Cancelled, Waitlisted
        /// </summary>
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        // Navigation
        public Event Event { get; set; } = null!;
        public Participant Participant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Result? Result { get; set; }
    }
}
