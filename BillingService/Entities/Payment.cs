using BillingService.Entities.Enums;

namespace BillingService.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public PaymentStatus Status { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? StripeInvoiceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? FailureReason { get; set; }
        public int RetryCount { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public virtual SubscriptionBilling Subscription { get; set; }
    }
}
