namespace SubscriptionService.EventContracts
{
    public class SubscriptionCreatedEvent
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
