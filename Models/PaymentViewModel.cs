using System.ComponentModel.DataAnnotations;

namespace HungryHub.Models
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string RestaurantName { get; set; }
            = string.Empty;
        public string UserFullName { get; set; }
            = string.Empty;
        public string UserEmail { get; set; }
            = string.Empty;

        [Required(ErrorMessage =
            "Please select a payment method")]
        public string PaymentMethod { get; set; }
            = string.Empty;

        // Mobile banking fields
        public string? MobileNumber { get; set; }
        public string? MobilePin { get; set; }

        // Card fields
        public string? CardNumber { get; set; }
        public string? CardHolder { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCVV { get; set; }
    }
}
