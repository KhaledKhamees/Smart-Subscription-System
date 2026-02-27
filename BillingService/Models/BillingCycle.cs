using BillingService.Models.Enum;

namespace BillingService.Models
{
    public class BillingCycle
    {
        public Guid BillingCycleId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BillingPeriod Period { get; set; }
    }
}
