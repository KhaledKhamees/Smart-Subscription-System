using BillingService.Models;

namespace BillingService.Repositories.Interfaces
{
    public interface ISubscriptionPlanRepository
    {
        Task<SubscriptionPlan> GetByIdAsync(Guid planId);
    }
}
