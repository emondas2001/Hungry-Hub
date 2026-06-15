namespace HungryHub.Models
{
    public class WeatherData
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Main { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string FoodSuggestion { get; set; } = string.Empty;
        public string SuggestionReason { get; set; } = string.Empty;
        public bool IsLoaded { get; set; } = false;
    }
}
