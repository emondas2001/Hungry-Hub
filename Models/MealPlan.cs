namespace HungryHub.Models
{
    public class MealPlan
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
            = string.Empty;
        public string? Description { get; set; }
        public int MealsPerWeek { get; set; }
        public decimal PricePerWeek { get; set; }
        public bool IsActive { get; set; }
            = true;
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }

    public class UserSubscription
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
            = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DeliveryTime { get; set; }
            = "12:00 PM";
        public string DeliveryAddr { get; set; }
            = string.Empty;
        public string Status { get; set; }
            = "Active";
        public decimal TotalPaid { get; set; }
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public MealPlan? Plan { get; set; }
    }
}