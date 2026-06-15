using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class MenuItem
    {
        [Key]
        public int MenuItemId { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        // New: stores uploaded food image path
        public string? ImagePath { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}