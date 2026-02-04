using SubscriptionService.DTOs;
using SubscriptionService.Entities;

namespace SubscriptionService.Data.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<Guid> AddAsync(CreateSubscriptionDTO subscription);
        Task<SubscriptionDTO?> GetByIdAsync(Guid id);
        Task<List<SubscriptionDTO>> GetByUserIdAsync(Guid userId);
        Task ActivateAsync(Guid id);
        Task SuspendAsync(Guid id);
        Task CancelAsync(Guid id);
        Task UpdateAsync(Guid Id , DateTime nextBillingDate);
    }
}
