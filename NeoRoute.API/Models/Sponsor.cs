using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.Models
{
    public class Sponsor
    {
        public int SponsorId { get; set; }

        [Required, MaxLength(200)]
        public string SponsorName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? ContactInfo { get; set; }

        // Navigation
        public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
    }
}
