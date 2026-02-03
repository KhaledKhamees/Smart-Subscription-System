using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.Data.Interfaces
{
    public interface IPlanRepository
    {
        Task AddAsync(SubscriptionPlanDTO plan);
        Task<SubscriptionPlan?> GetByIdAsync(Guid id);
        Task<List<SubscriptionPlan>> GetByProductIdAsync(Guid productId);
        Task DeleteAsync (Guid id);
        Task UpdateAsync(SubscriptionPlanDTO plan);
    }
}
