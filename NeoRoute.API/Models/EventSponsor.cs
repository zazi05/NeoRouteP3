namespace NeoRoute.API.Models
{
    public class EventSponsor
    {
        public int EventId { get; set; }
        public int SponsorId { get; set; }

        // Navigation
        public Event Event { get; set; } = null!;
        public Sponsor Sponsor { get; set; } = null!;
    }
}
