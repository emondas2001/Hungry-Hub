using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class RestaurantHours
    {
        [Key]
        public int HoursId { get; set; }
        public int RestaurantId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeOnly? OpenTime { get; set; }
        public TimeOnly? CloseTime { get; set; }
        public bool IsClosed { get; set; } = false;

        public string DayName => DayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday",
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }
}