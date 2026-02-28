namespace NotificationService.Models
{
    public class SentExpiredSubscriptionNotification
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime SentDate { get; set; }
    }
}
