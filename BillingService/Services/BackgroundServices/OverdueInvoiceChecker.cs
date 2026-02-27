using BillingService.Services.Interfaces;

namespace BillingService.Services.BackgroundServices
{
    public class OverdueInvoiceChecker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueInvoiceChecker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        public OverdueInvoiceChecker(
            IServiceProvider serviceProvider,
            ILogger<OverdueInvoiceChecker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Overdue Invoice Checker started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                        await billingService.MarkOverdueInvoicesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking for overdue invoices");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Overdue Invoice Checker stopped");
        }
    }
}
