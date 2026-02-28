using MassTransit;
using NotificationService.EventContracts;

namespace NotificationService.Consumers
{
    public class SubscriptionExpiringSoonEventConsumer : IConsumer<SubscriptionExpiringSoonEvent>
    {
        private readonly ILogger<SubscriptionExpiringSoonEventConsumer> _logger;
        public SubscriptionExpiringSoonEventConsumer(ILogger<SubscriptionExpiringSoonEventConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<SubscriptionExpiringSoonEvent> context)
        {
            _logger.LogInformation("Received SubscriptionExpiringSoonEvent for SubscriptionId: {SubscriptionId}, UserId: {UserId}, EndDate: {EndDate}",
                context.Message.SubscriptionId, context.Message.UserId, context.Message.EndDate);
            var message = context.Message;
            return Task.CompletedTask;
        }
    }
}
