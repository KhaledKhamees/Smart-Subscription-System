using BillingService.Data;
using BillingService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly ILogger<IdempotencyRepository> _logger;
        private readonly BillingDbContext billingDbContext;
        public IdempotencyRepository(
            BillingDbContext billingDbContext,
            ILogger<IdempotencyRepository> logger)
        {
            this.billingDbContext = billingDbContext;
            _logger = logger;
        }
        public async Task AddRequestAsync(Guid subscriptionId)
        {
            billingDbContext.IdempotencyKeys.Add(new Models.Idempotency
            {
                SubscriptionId = subscriptionId
            });
            await billingDbContext.SaveChangesAsync();
        }

        public async Task<bool> IsDuplicateRequestAsync(Guid subscriptionId)
        {
            return await billingDbContext.IdempotencyKeys.AnyAsync(i => i.SubscriptionId == subscriptionId);
        }
    }
}
