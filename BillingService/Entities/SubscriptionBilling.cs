using BillingService.Entities.Enums;

namespace BillingService.Entities
{
    public class SubscriptionBilling
    {
        public Guid Id { get; set; }  // = SubscriptionId 
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public SubscriptionLifecycleStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime? GracePeriodEndDate { get; set; }

        // Stripe references
        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }

        // Failure tracking
        public int ConsecutiveFailures { get; set; }
        public DateTime? LastPaymentAttempt { get; set; }

        // Navigation
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }
}
