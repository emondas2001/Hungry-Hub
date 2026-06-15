using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class RestaurantOwner
    {
        public int OwnerId { get; set; }
        public int RestaurantId { get; set; }

        [Required]
        public string FullName { get; set; }
            = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;

        public string PasswordHash { get; set; }
            = string.Empty;

        public string? Phone { get; set; }
        public bool IsActive { get; set; }
            = true;
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        // Navigation
        public RestaurantDb? Restaurant
        { get; set; }
    }

    public class RestaurantActivityLog
    {
        public int LogId { get; set; }
        public int RestaurantId { get; set; }
        public int OwnerId { get; set; }
        public string Action { get; set; }
            = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }

    public class OwnerLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;

        [Required]
        public string Password { get; set; }
            = string.Empty;
    }
}