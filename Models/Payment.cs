namespace HungryHub.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
            = string.Empty;
        public string? TransactionId { get; set; }
        public string? AccountNumber { get; set; }
        public string Status { get; set; }
            = "Pending";
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;
    }
}