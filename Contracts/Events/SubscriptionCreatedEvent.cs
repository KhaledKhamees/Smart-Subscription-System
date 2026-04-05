using System;

namespace Contracts.Events
{
    public enum BillingPeriod
    {
        Monthly = 0,
        Yearly = 1
    }
    public class SubscriptionCreatedEvent
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Price { get; set; }
        public int TrialDays { get; set; }
        public BillingPeriod BillingPeriod { get; set; } // 0= Monthly, 1 = Yearly
    }
}
