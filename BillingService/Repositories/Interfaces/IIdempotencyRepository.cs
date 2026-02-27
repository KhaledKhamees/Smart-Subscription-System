namespace BillingService.Repositories.Interfaces
{
    public interface IIdempotencyRepository
    {
        Task<bool> IsDuplicateRequestAsync(Guid subscriptionId);
        Task AddRequestAsync(Guid subscriptionId);
    }
}
