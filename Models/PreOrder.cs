using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class PreOrder
    {
        public int PreOrderId { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
            = string.Empty;

        [Required(ErrorMessage =
            "Event name is required")]
        public string EventName { get; set; }
            = string.Empty;

        [Required(ErrorMessage =
            "Event date is required")]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage =
            "Event address is required")]
        public string EventAddress { get; set; }
            = string.Empty;

        [Required(ErrorMessage =
            "Guest count is required")]
        [Range(10, 500,
            ErrorMessage =
                "Minimum 10, maximum 500 guests")]
        public int GuestCount { get; set; }

        public string? SpecialRequests { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string Status { get; set; }
            = "Pending";
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public List<PreOrderItem> Items
        { get; set; } = new();
    }

    public class PreOrderItem
    {
        public int PreOrderItemId { get; set; }
        public int PreOrderId { get; set; }
        public string ItemName { get; set; }
            = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}