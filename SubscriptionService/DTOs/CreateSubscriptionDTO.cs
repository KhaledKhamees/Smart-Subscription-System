using SubscriptionService.Entities;

namespace SubscriptionService.DTOs
{
    public class CreateSubscriptionDTO
    {
        public Guid UserId { get;  set; }
        public Guid PlanId { get;  set; }
    }
}
