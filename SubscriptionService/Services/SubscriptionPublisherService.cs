using MassTransit;
using SubscriptionService.EventContracts;
using SubscriptionService.Services.Interfaces;

namespace SubscriptionService.Services
{
    public class SubscriptionPublisherService : ISubscriptionPublisherService
    {
        private readonly ILogger<SubscriptionPublisherService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public SubscriptionPublisherService(ILogger<SubscriptionPublisherService> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        public Task PublishSubscriptionCanceledEventAsync(SubscriptionCanceledEvent canceledEvent, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Publishing SubscriptionCanceledEvent for SubscriptionId: {SubscriptionId}", canceledEvent.SubscriptionId);
                return _publishEndpoint.Publish(canceledEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish SubscriptionCanceledEvent for SubscriptionId: {SubscriptionId}", canceledEvent.SubscriptionId);
                throw;

            }
        }

        public Task PublishSubscriptionCreatedEventAsync(SubscriptionCreatedEvent createdEvent, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Publishing SubscriptionCreatedEvent for SubscriptionId: {SubscriptionId}", createdEvent.SubscriptionId);
                return _publishEndpoint.Publish(createdEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish SubscriptionCreatedEvent for SubscriptionId: {SubscriptionId}", createdEvent.SubscriptionId);
                throw;

            }
        }
    }
}
