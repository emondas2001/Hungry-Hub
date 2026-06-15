namespace HungryHub.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
            = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; }
            = "Pending";
        public string DeliveryAddress { get; set; }
            = string.Empty;
        public string OrderNote { get; set; }
            = string.Empty;

        // Nullable — these may be NULL in DB
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TransactionId { get; set; }

        public DateTime OrderDate { get; set; }
            = DateTime.Now;

        public List<OrderItem> OrderItems
        { get; set; } = new();
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}