using System.ComponentModel.DataAnnotations;

namespace NeoRoute.API.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;
        public string? Distance { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Distance { get; set; }
    }
}
