namespace SubscriptionService.Sync_communication.Summaries
{
    public class PlanSummary
    {
        public Guid PlanId { get; set; }
        public Guid ProductId { get; set; }
        public int PlanType { get; set; }
        public decimal Price { get; set; }
        public int TrialDays { get; set; }
    }
}
