using SubscriptionService.Entities;

namespace SubscriptionService.DTOs
{
    public class CreateSubscriptionDTO
    {
        public Guid UserId { get;  set; }
        public Guid PlanId { get;  set; }
        public DateTime StartDate { get;  set; }
        public DateTime? EndDate { get;  set; }
        public DateTime NextBillingDate { get;  set; }
    }
}
