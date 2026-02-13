namespace BillingService.Entities.Enums
{
    public enum SubscriptionLifecycleStatus
    {
        Trial = 0,
        Active = 1,
        GracePeriod = 2,
        PastDue = 3,
        Canceled = 4,
        Expired = 5
    }
}
