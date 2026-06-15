using HungryHub.Data;
using HungryHub.Models;

namespace HungryHub.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PaymentService>
            _logger;

        public PaymentService(
            ApplicationDbContext db,
            ILogger<PaymentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── Process payment ──────────────────────
        public async Task<PaymentResult>
            ProcessPaymentAsync(
            PaymentViewModel model)
        {
            // Simulate processing delay
            await Task.Delay(1500);

            try
            {
                string txnId = GenerateTransactionId(
                    model.PaymentMethod);

                bool success = ValidatePayment(model);

                if (!success)
                {
                    return new PaymentResult
                    {
                        Success = false,
                        Message =
                            "Payment failed. " +
                            "Please check your " +
                            "details and try again.",
                        TransactionId = null
                    };
                }

                // Save payment record
                _db.Payments.Add(new Payment
                {
                    OrderId = model.OrderId,
                    UserId = GetUserIdFromOrder(
                        model.OrderId),
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    TransactionId = txnId,
                    AccountNumber =
                        model.MobileNumber
                        ?? MaskCard(model.CardNumber),
                    Status = "Success",
                    PaidAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                });

                // Update order payment status
                var order = _db.Orders.FirstOrDefault(
                o => o.OrderId == model.OrderId);
                if (order != null)
                {
                    order.PaymentMethod =
                        model.PaymentMethod ?? "Cash";
                    order.PaymentStatus = "Paid";
                    order.TransactionId = txnId ?? "";
                    order.Status = "Confirmed";
                }

                _db.SaveChanges();

                return new PaymentResult
                {
                    Success = true,
                    Message =
                        "Payment successful! " +
                        "Your order is confirmed.",
                    TransactionId = txnId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Payment processing error");
                return new PaymentResult
                {
                    Success = false,
                    Message =
                        "Payment error. Please try again."
                };
            }
        }

        // ── Validate based on method ─────────────
        private bool ValidatePayment(
            PaymentViewModel model)
        {
            switch (model.PaymentMethod)
            {
                case "bKash":
                case "Nagad":
                case "Rocket":
                    // Validate mobile number format
                    if (string.IsNullOrEmpty(
                            model.MobileNumber) ||
                        model.MobileNumber.Length < 11)
                        return false;

                    if (string.IsNullOrEmpty(
                            model.MobilePin) ||
                        model.MobilePin.Length < 4)
                        return false;

                    return true;

                case "Visa":
                case "Mastercard":
                    // Validate card details
                    if (string.IsNullOrEmpty(
                            model.CardNumber) ||
                        model.CardNumber.Replace(
                            " ", "").Length < 16)
                        return false;

                    if (string.IsNullOrEmpty(
                            model.CardHolder))
                        return false;

                    if (string.IsNullOrEmpty(
                            model.CardExpiry))
                        return false;

                    if (string.IsNullOrEmpty(
                            model.CardCVV) ||
                        model.CardCVV.Length < 3)
                        return false;

                    return true;

                case "Cash":
                    return true;

                default:
                    return false;
            }
        }

        // ── Generate unique transaction ID ───────
        private string GenerateTransactionId(
            string method)
        {
            string prefix = method switch
            {
                "bKash" => "BK",
                "Nagad" => "NG",
                "Rocket" => "RK",
                "Visa" => "VS",
                "Mastercard" => "MC",
                _ => "TX"
            };

            return prefix +
                DateTime.Now.ToString("yyyyMMddHHmmss") +
                new Random()
                    .Next(1000, 9999).ToString();
        }

        // ── Mask card number for storage ─────────
        private string MaskCard(string? card)
        {
            if (string.IsNullOrEmpty(card))
                return string.Empty;
            var clean = card.Replace(" ", "");
            if (clean.Length < 4)
                return card;
            return "****" +
                clean.Substring(clean.Length - 4);
        }

        private int GetUserIdFromOrder(int orderId)
        {
            return _db.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => o.UserId)
                .FirstOrDefault();
        }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
            = string.Empty;
        public string? TransactionId { get; set; }
    }
}