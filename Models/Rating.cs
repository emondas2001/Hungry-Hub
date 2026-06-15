namespace HungryHub.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public int RestaurantId { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime RatedAt { get; set; } = DateTime.Now;
    }
}