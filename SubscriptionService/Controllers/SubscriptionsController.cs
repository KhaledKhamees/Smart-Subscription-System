using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Data.Interfaces;
using SubscriptionService.Services.Interfaces;
using SubscriptionService.Sync_communication.Interfaces;

namespace SubscriptionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly ICatalogClient _catalogClient;
        private readonly ISubscriptionPublisherService _subscriptionPublisher;
        public SubscriptionsController(ISubscriptionRepository repository
                                    , ILogger<SubscriptionsController> logger
                                    , ICatalogClient catalogClient,
                                    ISubscriptionPublisherService subscriptionPublisher)
        {
            _repository = repository;
            _logger = logger;
            _catalogClient = catalogClient;
            _subscriptionPublisher = subscriptionPublisher;
        }
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetSubscriptionById(Guid Id)
        {
            var subscriptions = await _repository.GetByIdAsync(Id);
            return Ok(subscriptions);
        }
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] DTOs.CreateSubscriptionDTO subscription)
        {
            var plan = await _catalogClient.GetPlanSummaryAsync(subscription.PlanId);
            if (plan == null)
            {
                _logger.LogWarning("Plan not found: {PlanId}", subscription.PlanId);
                return NotFound($"Plan with ID {subscription.PlanId} not found.");
            }
            DateTime nextBillingDate = DateTime.UtcNow.AddMonths(1);
            if (plan.PlanType == 1)
            {
                nextBillingDate = DateTime.UtcNow.AddYears(1);
            }
            var Id = await _repository.AddAsync(subscription.UserId, subscription.PlanId, nextBillingDate);
            _logger.LogInformation("Subscription created for UserId: {UserId}", subscription.UserId);
            CancellationToken cancellationToken = HttpContext.RequestAborted;
            await _subscriptionPublisher.PublishSubscriptionCreatedEventAsync( new EventContracts.SubscriptionCreatedEvent
            {
                SubscriptionId = Id,
                UserId = subscription.UserId,
                PlanId = subscription.PlanId,
                StartDate = DateTime.UtcNow
            },cancellationToken);
            return CreatedAtAction(nameof(GetSubscriptionById), new { id = Id }, subscription);
        }
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetSubscriptionsByUserId(Guid userId)
        {
            var subscriptions = await _repository.GetByUserIdAsync(userId);
            return Ok(subscriptions);
        }
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateSubscription(Guid id)
        {
            await _repository.ActivateAsync(id);
            _logger.LogInformation("Subscription activated: {SubscriptionId}", id);
            return NoContent();
        }
        [HttpPost("{id}/suspend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SuspendSubscription(Guid id)
        {
            await _repository.SuspendAsync(id);
            _logger.LogInformation("Subscription suspended: {SubscriptionId}", id);
            return NoContent();
        }
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelSubscription(Guid id)
        {
            await _repository.CancelAsync(id);
            _logger.LogInformation("Subscription canceled: {SubscriptionId}", id);
            await _subscriptionPublisher.PublishSubscriptionCanceledEventAsync(new EventContracts.SubscriptionCanceledEvent
            {
                SubscriptionId = id,
                CanceledAt = DateTime.UtcNow
            }, HttpContext.RequestAborted);
            return NoContent();
        }
        [HttpPut("{id}/next-billing-date")]
        public async Task<IActionResult> UpdateNextBillingDate(Guid id, [FromBody] DateTime nextBillingDate)
        {
            await _repository.UpdateAsync(id, nextBillingDate);
            _logger.LogInformation("Subscription next billing date updated: {SubscriptionId}", id);
            return NoContent();
        }

    }
}
