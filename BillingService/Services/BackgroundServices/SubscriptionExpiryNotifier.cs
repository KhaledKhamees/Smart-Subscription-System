using BillingService.Data;
using BillingService.EventContracts.Publishing_Contracts;
using BillingService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services.BackgroundServices
{
    public class SubscriptionExpiryNotifier : BackgroundService
    {
        private readonly ILogger<SubscriptionExpiryNotifier> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SubscriptionExpiryNotifier(
            ILogger<SubscriptionExpiryNotifier> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Expiry Notifier started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // FIX: Create a fresh scope per iteration — resolves both scoped services safely
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
                    var publisherService = scope.ServiceProvider
                        .GetRequiredService<ISubscriptionExpiryNotificationPublisherService>();

                    var expiringSubscriptions = await CheckForExpiringSubscriptionsAsync(dbContext, stoppingToken);

                    foreach (var subscription in expiringSubscriptions)
                    {
                        await publisherService.PublishSubscriptionExpiringSoonEventAsync(subscription, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for expiring subscriptions.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Subscription Expiry Notifier stopped at: {time}", DateTimeOffset.Now);
        }

        private async Task<List<SubscriptionExpiringSoonEvent>> CheckForExpiringSubscriptionsAsync(
            BillingDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(3);

            return await dbContext.BillingCycles
                .Where(b => b.EndDate.Date == targetDate)
                .Select(b => new SubscriptionExpiringSoonEvent
                {
                    SubscriptionId = b.SubscriptionId,
                    UserId = b.UserId,
                    EndDate = b.EndDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}
