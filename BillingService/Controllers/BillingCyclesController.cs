using BillingService.Models;
using BillingService.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingCyclesController : ControllerBase
    {
        private readonly IBillingCycleRepository _billingCycleRepository;
        private readonly ILogger<BillingCyclesController> _logger;

        public BillingCyclesController(
            IBillingCycleRepository billingCycleRepository,
            ILogger<BillingCyclesController> logger)
        {
            _billingCycleRepository = billingCycleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all billing cycles for a subscription
        /// </summary>
        [HttpGet("subscription/{subscriptionId}")]
        [ProducesResponseType(typeof(List<BillingCycle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBillingCycles(Guid subscriptionId)
        {
            try
            {
                var cycles = await _billingCycleRepository.GetBySubscriptionIdAsync(subscriptionId);
                return Ok(cycles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing cycles for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { error = "An error occurred while retrieving billing cycles" });
            }
        }

        /// <summary>
        /// Get the active billing cycle for a subscription
        /// </summary>
        [HttpGet("subscription/{subscriptionId}/active")]
        [ProducesResponseType(typeof(BillingCycle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveBillingCycle(Guid subscriptionId)
        {
            try
            {
                var cycle = await _billingCycleRepository.GetActiveBySubscriptionIdAsync(subscriptionId);
                if (cycle == null)
                {
                    return NotFound(new { error = $"No active billing cycle found for subscription {subscriptionId}" });
                }

                return Ok(cycle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active billing cycle for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { error = "An error occurred while retrieving the active billing cycle" });
            }
        }
    }
}

