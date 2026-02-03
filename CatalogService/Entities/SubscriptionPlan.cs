namespace CatalogService.Entities
{
    public enum SubscriptionPlanType
    {
        Monthly,
        Yearly
    }
    public class SubscriptionPlan
    {
        public Guid Id { get; private set;}
        public Guid ProductId { get; private set; }
        public SubscriptionPlanType PlanType { get; private set; }
        public decimal Price { get; private set; }
        public int TrialDays { get; private set; }
        private SubscriptionPlan() { }
        public SubscriptionPlan(Guid productId, SubscriptionPlanType planType, decimal price, int trailDays)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            PlanType = planType;
            Price = price;
            TrialDays = trailDays;
        }
    }
}
