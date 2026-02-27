using BillingService.EventContracts.Publishing_Contracts;

namespace BillingService.Services.Interfaces
{
    public interface ISubscriptionExpiryNotificationPublisherService
    {
        Task PublishSubscriptionExpiringSoonEventAsync(SubscriptionExpiringSoonEvent subscription , CancellationToken cancellationToken);
    }
}
