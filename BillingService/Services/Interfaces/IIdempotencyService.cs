namespace BillingService.Services.Interfaces
{
    public interface IIdempotencyService
    {
        Task<bool> IsDuplicateRequestAsync(Guid SubscriptionId);
        Task AddRequestAsync(Guid SubscriptionId);
    }
}
