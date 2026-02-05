namespace SubscriptionService.Entities
{
    public enum SubscriptionStatus
    {
        Pending = 0,
        Active = 1,
        Suspended =2,
        Canceled = 3
    }
    public class Subscription
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid PlanId { get; private set; }
        public SubscriptionStatus Status { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public DateTime NextBillingDate { get; private set; }

        private Subscription() { }
        
        public Subscription(Guid userId, Guid planId,DateTime nextBillingDate)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            PlanId = planId;
            Status = SubscriptionStatus.Pending;
            StartDate = DateTime.UtcNow;
            NextBillingDate = nextBillingDate;
        }
        public void Activate()
        {
            if (Status != SubscriptionStatus.Pending)
                throw new InvalidOperationException("Only pending subscriptions can be activated.");
            Status = SubscriptionStatus.Active;
        }
        public void Suspend()
        {
            if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Only active subscriptions can be suspended.");
            Status = SubscriptionStatus.Suspended;
        }
        public void Cancel()
        {
            if (Status == SubscriptionStatus.Canceled)
                throw new InvalidOperationException("Subscription is already canceled.");
            Status = SubscriptionStatus.Canceled;
            EndDate = DateTime.UtcNow;
        }
        public void Renew(DateTime nextBillingDate)
        {
            if (Status != SubscriptionStatus.Active)
                throw new InvalidOperationException("Only active subscriptions can be renewed.");
            NextBillingDate = nextBillingDate;
        }
    }
}
