using BillingService.EventContracts;
using BillingService.Services.Interfaces;
using MassTransit;

namespace BillingService.Consumers
{
    public class SubscriptionCreatedConsumer : IConsumer<SubscriptionCreatedEvent>
    {
        private readonly ILogger<SubscriptionCreatedConsumer> _logger;
        private readonly IBillingService _billingService;
        private readonly IIdempotencyService _idempotencyService;
        public SubscriptionCreatedConsumer(ILogger<SubscriptionCreatedConsumer> logger , IBillingService billingService, IIdempotencyService idempotencyService)
        {
            _logger = logger;
            _billingService = billingService;
            _idempotencyService = idempotencyService;
        }
        public async Task Consume(ConsumeContext<SubscriptionCreatedEvent> context)
        {
            _logger.LogInformation("Received SubscriptionCreatedEvent for SubscriptionId: {SubscriptionId}",
                context.Message.SubscriptionId);

            var message = context.Message;
            // Check for idempotency to avoid processing duplicate messages
            if (await _idempotencyService.IsDuplicateRequestAsync(message.SubscriptionId))
            {
                _logger.LogWarning($"Duplicate SubscriptionCreatedEvent received for SubscriptionId: {message.SubscriptionId}. Ignoring.");
                return; 
            }

            try
            {
                await _idempotencyService.AddRequestAsync(message.SubscriptionId);

                await _billingService.ProcessSubscriptionCreatedAsync(
                    message.SubscriptionId,
                    message.UserId,
                    message.PlanId,
                    message.StartDate,
                    message.Price,
                    message.TrialDays,
                    message.BillingPeriod);

                _logger.LogInformation("Successfully processed subscription creation for {SubscriptionId}",
                    message.SubscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription creation for {SubscriptionId}",
                    message.SubscriptionId);
                throw; // Let MassTransit handle retry logic
            }
        }
    }
}
