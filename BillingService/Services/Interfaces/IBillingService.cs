using BillingService.Models.Enum;

namespace BillingService.Services.Interfaces
{
    public interface IBillingService
    {
        Task ProcessSubscriptionCreatedAsync(Guid subscriptionId, Guid userId, Guid planId, DateTime startDate, decimal Price ,int TrialDays, BillingPeriod BillingPeriod);
        Task ProcessSubscriptionCanceledAsync(Guid subscriptionId, DateTime canceledAt);
        Task ProcessPaymentAsync(Guid invoiceId);
        Task MarkOverdueInvoicesAsync();
    }
}
