using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class RestaurantDb
    {
        [Key]
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Cuisine { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; } = 20;
        public int DeliveryTime { get; set; } = 30;
        public decimal MinOrder { get; set; } = 0;
        public decimal Rating { get; set; } = 0;
        public string ImageUrl { get; set; } = string.Empty;

        // New: stores uploaded image path
        public string? ImagePath { get; set; }

        public string Tag { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<RestaurantHours> Hours
        { get; set; } = new();
        public List<MenuItem> MenuItems
        { get; set; } = new();
    }
}