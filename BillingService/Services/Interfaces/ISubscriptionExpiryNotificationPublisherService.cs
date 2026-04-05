using Contracts.Events;

namespace BillingService.Services.Interfaces
{
    public interface ISubscriptionExpiryNotificationPublisherService
    {
        Task PublishSubscriptionExpiringSoonEventAsync(SubscriptionExpiringSoonEvent subscription , CancellationToken cancellationToken);
    }
}
