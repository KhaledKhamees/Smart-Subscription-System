namespace CatalogService.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal BasePrice { get; private set; }
        public string Currency { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<SubscriptionPlan> _plans = new();
        public IReadOnlyCollection<SubscriptionPlan> Plans => _plans.AsReadOnly();
        private Product() { }
        public Product(string name, string description, decimal basePrice, string currency)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            BasePrice = basePrice;
            Currency = currency;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }
        public void AddSubscriptionPlan(SubscriptionPlan plan)
        {
            _plans.Add(plan);
        }
        public void Deactivate()
        {
            IsActive = false;
        }
        public void Activate()
        {
            IsActive = true;
        }
    }
}
