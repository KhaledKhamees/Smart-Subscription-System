using SubscriptionService.Sync_communication.Summaries;

namespace SubscriptionService.Sync_communication.Interfaces
{
    public interface ICatalogClient
    {
        Task<PlanSummary?> GetPlanSummaryAsync(Guid planId);
    }
}
