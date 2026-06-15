namespace HungryHub.Models
{
    public class SplitOrder
    {
        public int SplitOrderId { get; set; }
        public int OrderId { get; set; }
        public int CreatorUserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string SplitType { get; set; }
            = "Equal";
        public string Status { get; set; }
            = "Active";
        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public List<SplitParticipant>
            Participants
        { get; set; } = new();
    }

    public class SplitParticipant
    {
        public int ParticipantId { get; set; }
        public int SplitOrderId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
            = string.Empty;
        public string? Email { get; set; }
        public decimal AmountOwed { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
