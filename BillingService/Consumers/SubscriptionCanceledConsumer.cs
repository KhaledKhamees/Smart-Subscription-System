using BillingService.EventContracts;
using MassTransit;

namespace BillingService.Consumers
{
    public class SubscriptionCanceledConsumer : IConsumer<SubscriptionCanceledEvent>
    {
        private readonly ILogger<SubscriptionCanceledConsumer> _logger;
        public Task Consume(ConsumeContext<SubscriptionCanceledEvent> context)
        {
            _logger.LogInformation("Received SubscriptionCanceledEvent for SubscriptionId: {SubscriptionId} at {CanceledAt}",
                context.Message.SubscriptionId, context.Message.CanceledAt);
            var message = context.Message;
            _logger.LogInformation("Processing cancellation for SubscriptionId: {SubscriptionId}", message.SubscriptionId);
            return Task.CompletedTask;
        }
    }
}
