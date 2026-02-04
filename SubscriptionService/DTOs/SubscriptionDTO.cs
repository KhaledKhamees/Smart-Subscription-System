using SubscriptionService.Entities;

namespace SubscriptionService.DTOs
{
    public class SubscriptionDTO
    {
        public Guid Id { get;  set; }
        public Guid UserId { get;  set; }
        public Guid PlanId { get;  set; }
        public SubscriptionStatus Status { get;  set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextBillingDate { get;  set; }
    }
}
