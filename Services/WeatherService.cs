using HungryHub.Models;
using System.Text.Json;

namespace HungryHub.Services
{
    public class WeatherService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(
            IConfiguration config,
            IHttpClientFactory http,
            ILogger<WeatherService> logger)
        {
            _config = config;
            _http = http;
            _logger = logger;
        }

        public async Task<WeatherData>
            GetWeatherAsync()
        {
            var result = new WeatherData();

            try
            {
                string apiKey =
                    _config["WeatherApi:ApiKey"]!;
                string city =
                    _config["WeatherApi:City"]
                    ?? "Chattogram";
                string country =
                    _config["WeatherApi:Country"]
                    ?? "BD";

                string url =
                    "https://api.openweathermap.org" +
                    "/data/2.5/weather" +
                    "?q=" + city + "," + country +
                    "&appid=" + apiKey +
                    "&units=metric";

                var client = _http.CreateClient();
                var response =
                    await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return GetFallbackWeather();

                var json = await response.Content
                    .ReadAsStringAsync();

                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                double temp = root
                    .GetProperty("main")
                    .GetProperty("temp")
                    .GetDouble();

                double feelsLike = root
                    .GetProperty("main")
                    .GetProperty("feels_like")
                    .GetDouble();

                int humidity = root
                    .GetProperty("main")
                    .GetProperty("humidity")
                    .GetInt32();

                double wind = root
                    .GetProperty("wind")
                    .GetProperty("speed")
                    .GetDouble();

                string desc = root
                    .GetProperty("weather")[0]
                    .GetProperty("description")
                    .GetString() ?? "";

                string main = root
                    .GetProperty("weather")[0]
                    .GetProperty("main")
                    .GetString() ?? "";

                string icon = root
                    .GetProperty("weather")[0]
                    .GetProperty("icon")
                    .GetString() ?? "";

                string cityName = root
                    .GetProperty("name")
                    .GetString() ?? city;

                result.City = cityName;
                result.Country = country;
                result.Temperature = Math.Round(temp, 1);
                result.FeelsLike = Math.Round(
                    feelsLike, 1);
                result.Humidity = humidity;
                result.WindSpeed = Math.Round(
                    wind * 3.6, 1); // m/s to km/h
                result.Description =
                    CapFirst(desc);
                result.Main = main;
                result.Icon = icon;
                result.Emoji = GetEmoji(main);
                result.IsLoaded = true;

                var suggestion =
                    GetFoodSuggestion(main, temp);
                result.FoodSuggestion =
                    suggestion.Item1;
                result.SuggestionReason =
                    suggestion.Item2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Weather API error");
                return GetFallbackWeather();
            }

            return result;
        }

        // ── Weather emoji mapper ─────────────────────
        private string GetEmoji(string main)
        {
            return main switch
            {
                "Thunderstorm" => "⛈️",
                "Drizzle" => "🌦️",
                "Rain" => "🌧️",
                "Snow" => "❄️",
                "Mist" => "🌫️",
                "Fog" => "🌫️",
                "Haze" => "🌫️",
                "Clear" => "☀️",
                "Clouds" => "☁️",
                "Smoke" => "💨",
                "Dust" => "💨",
                "Sand" => "💨",
                "Tornado" => "🌪️",
                _ => "🌤️"
            };
        }

        // ── Food suggestion based on weather ─────────
        private (string, string) GetFoodSuggestion(
            string main, double temp)
        {
            if (main == "Rain" ||
                main == "Thunderstorm" ||
                main == "Drizzle")
                return (
                    "Hot Soup or Khichuri",
                    "Rainy weather calls for " +
                    "warm comfort food!");

            if (main == "Snow")
                return (
                    "Hot Chocolate & Snacks",
                    "Stay warm with hot drinks!");

            if (temp >= 35)
                return (
                    "Cold Drinks & Light Salads",
                    "Beat the heat with " +
                    "something cool and fresh!");

            if (temp >= 28)
                return (
                    "Refreshing Drinks & Snacks",
                    "Warm day — stay hydrated " +
                    "with light meals!");

            if (temp >= 20 && temp < 28)
                return (
                    "Biryani or Grilled Food",
                    "Perfect weather for a " +
                    "hearty meal!");

            if (temp < 20)
                return (
                    "Hot Kacchi or Beef Curry",
                    "Cool weather — time for " +
                    "rich and hearty food!");

            return (
                "Anything you crave!",
                "Great weather to enjoy " +
                "any meal today!");
        }

        // ── Fallback when API fails ──────────────────
        private WeatherData GetFallbackWeather()
        {
            return new WeatherData
            {
                City = "Chattogram",
                Country = "BD",
                Temperature = 28.0,
                FeelsLike = 30.0,
                Humidity = 75,
                WindSpeed = 12.0,
                Description = "Partly cloudy",
                Main = "Clouds",
                Emoji = "⛅",
                FoodSuggestion = "Biryani or Grilled Food",
                SuggestionReason = "Enjoy a great meal today!",
                IsLoaded = false
            };
        }

        private string CapFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) +
                   s.Substring(1);
        }
    }
}
