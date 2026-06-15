using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class Coupon
    {
        public int CouponId { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }
            = string.Empty;

        [Required]
        public string Title { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        // "Percent" or "Flat"
        [Required]
        public string DiscountType { get; set; }
            = "Flat";

        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal MaxDiscount { get; set; }
        public int UsageLimit { get; set; }
            = 100;
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
            = true;
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }

    public class CouponUsage
    {
        public int UsageId { get; set; }
        public int CouponId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public decimal Discount { get; set; }
        public DateTime UsedAt { get; set; }
            = DateTime.Now;
    }
}