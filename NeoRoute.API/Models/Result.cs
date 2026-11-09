using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrollmentId { get; set; }

        [MaxLength(20)]
        public string? FinishTime { get; set; }

        public int? Position { get; set; }

        // Navigation
        public Enrollment Enrollment { get; set; } = null!;
    }
}
