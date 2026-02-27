using BillingService.Data;
using BillingService.Models;
using BillingService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories
{
    public class BillingCycleRepository : IBillingCycleRepository
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<BillingCycleRepository> _logger;

        public BillingCycleRepository(
            BillingDbContext context,
            ILogger<BillingCycleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BillingCycle> CreateAsync(BillingCycle cycle)
        {
            try
            {
                cycle.BillingCycleId = Guid.NewGuid();

                _context.BillingCycles.Add(cycle);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created billing cycle {BillingCycleId} in database", cycle.BillingCycleId);
                return cycle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billing cycle in database");
                throw;
            }
        }

        public async Task<BillingCycle> GetActiveBySubscriptionIdAsync(Guid subscriptionId)
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.BillingCycles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.SubscriptionId == subscriptionId
                        && c.StartDate <= now
                        && c.EndDate >= now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active billing cycle for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<List<BillingCycle>> GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            try
            {
                return await _context.BillingCycles
                    .AsNoTracking()
                    .Where(c => c.SubscriptionId == subscriptionId)
                    .OrderByDescending(c => c.StartDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing cycles for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }
    }
}
