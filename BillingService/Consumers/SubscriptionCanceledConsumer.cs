using BillingService.EventContracts;
using BillingService.Services.Interfaces;
using MassTransit;

namespace BillingService.Consumers
{
    public class SubscriptionCanceledConsumer : IConsumer<SubscriptionCanceledEvent>
    {
        private readonly ILogger<SubscriptionCanceledConsumer> _logger;
        private readonly IBillingService _billingService;
        public SubscriptionCanceledConsumer(ILogger<SubscriptionCanceledConsumer> logger, IBillingService billingService)
        {
            _logger = logger;
            _billingService = billingService;
        }
        public async Task Consume(ConsumeContext<SubscriptionCanceledEvent> context)
        {
            _logger.LogInformation("Received SubscriptionCanceledEvent for SubscriptionId: {SubscriptionId} at {CanceledAt}",
                context.Message.SubscriptionId, context.Message.CanceledAt);

            var message = context.Message;

            try
            {
                await _billingService.ProcessSubscriptionCanceledAsync(
                    message.SubscriptionId,
                    message.CanceledAt);

                _logger.LogInformation("Successfully processed subscription cancellation for {SubscriptionId}",
                    message.SubscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription cancellation for {SubscriptionId}",
                    message.SubscriptionId);
                throw;
            }
        }
    }
}
