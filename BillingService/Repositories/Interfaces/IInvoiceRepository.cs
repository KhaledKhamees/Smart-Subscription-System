using BillingService.Models;

namespace BillingService.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice> CreateAsync(Invoice invoice);
        Task<Invoice> GetByIdAsync(Guid invoiceId);
        Task<List<Invoice>> GetBySubscriptionIdAsync(Guid subscriptionId);
        Task<Invoice> UpdateAsync(Invoice invoice);
        Task<List<Invoice>> GetOverdueInvoicesAsync();
    }
}
