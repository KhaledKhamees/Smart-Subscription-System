using BillingService.Entities;
using BillingService.Entities.Enums;

namespace BillingService.Services.Interfaces
{
    public interface IBillingService
    {
        Task<SubscriptionBilling> CreateSubscriptionBillingAsync(Guid subscriptionId, Guid userId, Guid planId, DateTime startDate);
        Task<Payment> CreatePaymentRecordAsync(Guid subscriptionId, decimal amount, string stripeInvoiceId);
        Task UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? failureReason = null);
        Task HandlePaymentSuccessAsync(string paymentIntentId);
        Task HandlePaymentFailureAsync(string paymentIntentId, string reason);
        Task ProcessRetryPaymentsAsync();
        Task ProcessGracePeriodExpirationsAsync();
        Task CancelSubscriptionAsync(Guid subscriptionId);
    }
}
