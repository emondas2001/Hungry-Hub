namespace HungryHub.Services
{
    public class WeeklyReportHostedService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _config;
        private readonly ILogger<WeeklyReportHostedService>
            _logger;

        public WeeklyReportHostedService(
            IServiceScopeFactory factory,
            IConfiguration config,
            ILogger<WeeklyReportHostedService> logger)
        {
            _factory = factory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var next = GetNextMonday8AM(now);
                    var delay = next - now;

                    _logger.LogInformation(
                        "Next weekly report: " +
                        next.ToString(
                            "dd MMM yyyy HH:mm"));

                    await Task.Delay(
                        delay, stoppingToken);

                    if (!stoppingToken
                            .IsCancellationRequested)
                    {
                        await RunReportAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Weekly report error");
                    await Task.Delay(
                        TimeSpan.FromHours(1),
                        stoppingToken);
                }
            }
        }

        private async Task RunReportAsync()
        {
            try
            {
                using var scope =
                    _factory.CreateScope();

                var provider =
                    scope.ServiceProvider;

                var svc =
                    (EmailReportService)provider
                    .GetService(
                        typeof(EmailReportService))!;

                await svc.SendWeeklyReportAsync();

                _logger.LogInformation(
                    "Weekly report sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send report");
            }
        }

        private DateTime GetNextMonday8AM(
            DateTime from)
        {
            int days =
                ((int)DayOfWeek.Monday -
                 (int)from.DayOfWeek + 7) % 7;
            if (days == 0 && from.Hour >= 8)
                days = 7;
            return from.Date
                .AddDays(days)
                .AddHours(8);
        }
    }
}