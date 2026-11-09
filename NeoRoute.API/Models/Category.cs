using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Distance { get; set; }

        // Navigation
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
