using BillingService.Models.Enum;

namespace BillingService.Models
{
    public class Invoice
    {
        public Guid InvoiceId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
