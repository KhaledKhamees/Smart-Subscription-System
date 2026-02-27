using BillingService.Models;

namespace BillingService.Repositories.Interfaces
{
    public interface IBillingCycleRepository
    {
        Task<BillingCycle> CreateAsync(BillingCycle cycle);
        Task<BillingCycle> GetActiveBySubscriptionIdAsync(Guid subscriptionId);
        Task<List<BillingCycle>> GetBySubscriptionIdAsync(Guid subscriptionId);
    }
}
