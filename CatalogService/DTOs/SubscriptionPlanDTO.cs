using CatalogService.Entities;

namespace CatalogService.DTOs
{
    public class SubscriptionPlanDTO
    {
        public Guid ProductId { get;  set; }
        public SubscriptionPlanType PlanType { get;  set; }
        public decimal Price { get;  set; }
        public int TrailDays { get;  set; }
    }
}
