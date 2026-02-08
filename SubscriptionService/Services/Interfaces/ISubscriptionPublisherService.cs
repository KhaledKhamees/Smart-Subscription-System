using SubscriptionService.EventContracts;

namespace SubscriptionService.Services.Interfaces
{
    public interface ISubscriptionPublisherService
    {
        Task PublishSubscriptionCreatedEventAsync(SubscriptionCreatedEvent createdEvent, CancellationToken cancellationToken);
        Task PublishSubscriptionCanceledEventAsync(SubscriptionCanceledEvent canceledEvent, CancellationToken cancellationToken);
        // Cancelation Token is used to allow the caller to cancel the operation if needed,
        // for example, if the application is shutting down or if the user cancels an action.
    }
}
