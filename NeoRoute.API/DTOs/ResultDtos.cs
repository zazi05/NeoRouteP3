using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.DTOs
{
    public class CreateResultDto
    {
        [Required]
        public int EnrollmentId { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
    }

    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrollmentId { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public string? ParticipantName { get; set; }
        public string? EventName { get; set; }
    }
}
