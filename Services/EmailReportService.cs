using HungryHub.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HungryHub.Services
{
    public class EmailReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailReportService>
            _logger;

        public EmailReportService(
            ApplicationDbContext db,
            IConfiguration config,
            ILogger<EmailReportService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task SendWeeklyReportAsync()
        {
            var weekStart = DateTime.Today.AddDays(-7);
            var weekEnd = DateTime.Today;

            int totalOrders = _db.Orders
                .Count(o => o.OrderDate >= weekStart);

            decimal totalRevenue = _db.Orders
                .Where(o => o.OrderDate >= weekStart)
                .Sum(o => (decimal?)o.GrandTotal)
                ?? 0;

            int newUsers = _db.Users
                .Count(u => u.CreatedAt >= weekStart);

            int cancelledOrders = _db.Orders
                .Count(o =>
                    o.OrderDate >= weekStart &&
                    o.Status == "Cancelled");

            var topRestaurant = _db.Orders
                .Where(o => o.OrderDate >= weekStart)
                .GroupBy(o => o.RestaurantName)
                .OrderByDescending(g => g.Count())
                .Select(g => new {
                    Name = g.Key,
                    Orders = g.Count(),
                    Revenue = g.Sum(x => x.GrandTotal)
                })
                .FirstOrDefault();

            var topItems = _db.OrderItems
                .Where(i => _db.Orders
                    .Where(o =>
                        o.OrderDate >= weekStart)
                    .Select(o => o.OrderId)
                    .Contains(i.OrderId))
                .GroupBy(i => i.ItemName)
                .OrderByDescending(g =>
                    g.Sum(x => x.Quantity))
                .Take(5)
                .Select(g => new {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            double cancelRate = totalOrders > 0
                ? Math.Round(
                    (double)cancelledOrders /
                    totalOrders * 100, 1)
                : 0;

            string html = BuildEmailHtml(
                weekStart, weekEnd,
                totalOrders, totalRevenue,
                newUsers, cancelledOrders,
                cancelRate,
                topRestaurant?.Name ?? "N/A",
                topRestaurant?.Orders ?? 0,
                topRestaurant?.Revenue ?? 0,
                topItems
                    .Select(x =>
                        x.Name + " (x" +
                        x.Quantity + ")")
                    .ToList());

            await SendEmailAsync(
                _config["EmailSettings:AdminEmail"]!,
                "HungryHub Weekly Report — Week of " +
                weekStart.ToString("dd MMM yyyy"),
                html);

            _logger.LogInformation(
                "Weekly report sent.");
        }

        private string BuildEmailHtml(
            DateTime weekStart,
            DateTime weekEnd,
            int totalOrders,
            decimal totalRevenue,
            int newUsers,
            int cancelledOrders,
            double cancelRate,
            string topRestaurant,
            int topOrders,
            decimal topRevenue,
            List<string> topItems)
        {
            string itemRows = string.Join("",
                topItems.Select(item =>
                    "<tr><td style='padding:8px 12px;" +
                    "border-bottom:1px solid #eee;'>" +
                    item + "</td></tr>"));

            return "<!DOCTYPE html><html>" +
                "<head><meta charset='utf-8'/>" +
                "<style>" +
                "body{font-family:'Segoe UI',sans-serif;" +
                "background:#f4f6fb;margin:0;padding:20px}" +
                ".container{max-width:600px;margin:0 auto;" +
                "background:#fff;border-radius:16px;" +
                "overflow:hidden;" +
                "box-shadow:0 4px 20px rgba(0,0,0,0.08)}" +
                ".header{background:linear-gradient(" +
                "135deg,#1a1a2e,#e84545);" +
                "padding:32px;text-align:center;color:#fff}" +
                ".header h1{margin:0;font-size:1.6rem}" +
                ".header p{margin:8px 0 0;opacity:0.8;" +
                "font-size:0.9rem}" +
                ".body{padding:28px}" +
                ".stat-grid{display:grid;" +
                "grid-template-columns:1fr 1fr;" +
                "gap:14px;margin-bottom:24px}" +
                ".stat-box{background:#f8f9ff;" +
                "border-radius:12px;padding:16px;" +
                "text-align:center;" +
                "border-left:4px solid #e84545}" +
                ".stat-val{font-size:1.8rem;" +
                "font-weight:700;color:#e84545}" +
                ".stat-lbl{font-size:0.78rem;" +
                "color:#888;margin-top:4px}" +
                ".section-title{font-size:1rem;" +
                "font-weight:700;color:#1a1a2e;" +
                "margin:20px 0 12px;" +
                "border-bottom:2px solid #f0f0f0;" +
                "padding-bottom:8px}" +
                "table{width:100%;border-collapse:collapse}" +
                "td{font-size:0.88rem;color:#444}" +
                ".footer{background:#f4f6fb;padding:20px;" +
                "text-align:center;font-size:0.78rem;" +
                "color:#aaa}" +
                ".highlight{background:#fff5f5;" +
                "border-radius:10px;padding:14px 18px;" +
                "margin-bottom:16px;" +
                "border:1.5px solid #ffc9c9}" +
                ".highlight strong{color:#e84545}" +
                "</style></head><body>" +
                "<div class='container'>" +
                "<div class='header'>" +
                "<div style='font-size:2.5rem;'>🍔</div>" +
                "<h1>HungryHub Weekly Report</h1>" +
                "<p>" + weekStart.ToString("dd MMM") +
                " — " +
                weekEnd.ToString("dd MMM yyyy") +
                "</p></div>" +
                "<div class='body'>" +
                "<div class='stat-grid'>" +
                "<div class='stat-box'>" +
                "<div class='stat-val'>" +
                totalOrders + "</div>" +
                "<div class='stat-lbl'>" +
                "Total Orders</div></div>" +
                "<div class='stat-box'>" +
                "<div class='stat-val'>&#2547;" +
                totalRevenue.ToString("N0") + "</div>" +
                "<div class='stat-lbl'>" +
                "Total Revenue</div></div>" +
                "<div class='stat-box'>" +
                "<div class='stat-val'>" +
                newUsers + "</div>" +
                "<div class='stat-lbl'>" +
                "New Users</div></div>" +
                "<div class='stat-box'>" +
                "<div class='stat-val' " +
                "style='color:#c62828;'>" +
                cancelRate + "%</div>" +
                "<div class='stat-lbl'>" +
                "Cancel Rate</div></div></div>" +
                "<div class='highlight'>" +
                "<strong>Top Restaurant:</strong><br/>" +
                topRestaurant + " — " +
                topOrders + " orders, &#2547;" +
                topRevenue.ToString("N0") +
                " revenue</div>" +
                "<div class='section-title'>" +
                "Top 5 Ordered Items</div>" +
                "<table>" + itemRows + "</table>" +
                "<div class='section-title'>" +
                "Summary</div>" +
                "<table>" +
                "<tr><td style='padding:6px 0;" +
                "color:#888;font-size:0.85rem;'>" +
                "Week period</td>" +
                "<td style='padding:6px 0;" +
                "font-weight:600;font-size:0.85rem;'>" +
                weekStart.ToString("dd MMM") +
                " — " +
                weekEnd.ToString("dd MMM yyyy") +
                "</td></tr>" +
                "<tr><td style='padding:6px 0;" +
                "color:#888;font-size:0.85rem;'>" +
                "Cancelled orders</td>" +
                "<td style='padding:6px 0;" +
                "font-weight:600;color:#c62828;" +
                "font-size:0.85rem;'>" +
                cancelledOrders + "</td></tr>" +
                "</table></div>" +
                "<div class='footer'>" +
                "Automated report from HungryHub.<br/>" +
                "Generated on " +
                DateTime.Now.ToString(
                    "dd MMM yyyy, HH:mm") +
                "</div></div></body></html>";
        }

        private async Task SendEmailAsync(
            string toEmail,
            string subject,
            string html)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["EmailSettings:SenderName"],
                _config["EmailSettings:SenderEmail"]));
            message.To.Add(
                MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = html
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["EmailSettings:SmtpHost"],
                int.Parse(
                    _config["EmailSettings:SmtpPort"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}