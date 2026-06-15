namespace HungryHub.Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Cuisine { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int DeliveryTime { get; set; }
        public decimal DeliveryFee { get; set; }
        public bool IsOpen { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
    }
}
