namespace SubscriptionService.EventContracts
{
    public class SubscriptionCanceledEvent
    {
        public Guid  SubscriptionId { get; set; }
        public DateTime CanceledAt { get; set; }
    }
}
