namespace HungryHub.Models
{
    public class DashboardViewModel
    {
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<Restaurant> Restaurants { get; set; } = new();
        public List<Restaurant> FeaturedRestaurants { get; set; } = new();
        public List<FoodCategory> Categories { get; set; } = new();
        public string? SearchQuery { get; set; }
    }
}
