using BillingService.EventContracts;
using MassTransit;

namespace BillingService.Consumers
{
    public class SubscriptionCreatedConsumer : IConsumer<SubscriptionCreatedEvent>
    {
        private readonly ILogger<SubscriptionCreatedConsumer> _logger;
        public SubscriptionCreatedConsumer(ILogger<SubscriptionCreatedConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<SubscriptionCreatedEvent> context)
        {
            _logger.LogInformation("Received SubscriptionCreatedEvent for SubscriptionId: {SubscriptionId}", context.Message.SubscriptionId);
            var message = context.Message;
            _logger.LogInformation("Processing subscription for UserId: {UserId}, PlanId: {PlanId}, StartDate: {StartDate}",
                message.UserId, message.PlanId, message.StartDate);
            return Task.CompletedTask;
        }
    }
}
