using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public int? ParticipantId { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
