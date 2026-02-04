using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Data.Interfaces;

namespace SubscriptionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionRepository _repository;
        private readonly ILogger<SubscriptionsController> _logger;
        public SubscriptionsController(ISubscriptionRepository repository, ILogger<SubscriptionsController> logger)
        {
            _repository = repository;
            _logger = logger;
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
            var Id = await _repository.AddAsync(subscription);
            _logger.LogInformation("Subscription created for UserId: {UserId}", subscription.UserId);
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
        public async Task<IActionResult> SuspendSubscription(Guid id)
        {
            await _repository.SuspendAsync(id);
            _logger.LogInformation("Subscription suspended: {SubscriptionId}", id);
            return NoContent();
        }
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(Guid id)
        {
            await _repository.CancelAsync(id);
            _logger.LogInformation("Subscription canceled: {SubscriptionId}", id);
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
