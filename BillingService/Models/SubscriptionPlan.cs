using BillingService.Models.Enum;

namespace BillingService.Models
{
    public class SubscriptionPlan
    {
        public Guid PlanId { get; set; }
        public decimal Price { get; set; }
        public BillingPeriod BillingPeriod { get; set; }
        public int TrialDays { get; set; }
    }
}
