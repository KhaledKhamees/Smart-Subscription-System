using BillingService.EventContracts.Publishing_Contracts;
using BillingService.Services.Interfaces;
using MassTransit;

namespace BillingService.Services
{
    public class SubscriptionExpiryNotificationPublisherService : ISubscriptionExpiryNotificationPublisherService
    {
        private readonly ILogger<SubscriptionExpiryNotificationPublisherService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public SubscriptionExpiryNotificationPublisherService(ILogger<SubscriptionExpiryNotificationPublisherService> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        public async Task PublishSubscriptionExpiringSoonEventAsync(SubscriptionExpiringSoonEvent subscription, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Publishing SubscriptionExpiringSoonEvent for SubscriptionId: {SubscriptionId}", subscription.SubscriptionId);
                await _publishEndpoint.Publish(subscription, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish SubscriptionExpiringSoonEvent for SubscriptionId: {SubscriptionId}", subscription.SubscriptionId);
                throw;
            }
             
        }
    }
}
