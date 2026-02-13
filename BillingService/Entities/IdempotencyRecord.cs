namespace BillingService.Entities
{
    public class IdempotencyRecord
    {
        public string Key { get; set; }  // Primary key
        public string EventType { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string? Response { get; set; }
    }
}
