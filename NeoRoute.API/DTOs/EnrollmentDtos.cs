using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.DTOs
{
    public class CreateEnrollmentDto
    {
        [Required]
        public int EventId { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }

    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public int ParticipantId { get; set; }
        public string? ParticipantName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class UpdateEnrollmentStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
