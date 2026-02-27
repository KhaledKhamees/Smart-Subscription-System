using BillingService.Models.Enum;

namespace BillingService.EventContracts
{
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
