using BillingService.Data;
using BillingService.Models;
using BillingService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<SubscriptionPlanRepository> _logger;

        public SubscriptionPlanRepository(
            BillingDbContext context,
            ILogger<SubscriptionPlanRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SubscriptionPlan> GetByIdAsync(Guid planId)
        {
            try
            {
                return await _context.SubscriptionPlans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PlanId == planId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription plan {PlanId}", planId);
                throw;
            }
        }
    }
}
