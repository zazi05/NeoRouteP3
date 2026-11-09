using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public int? Age { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
