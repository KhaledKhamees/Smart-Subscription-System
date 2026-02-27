using BillingService.Data;
using BillingService.Models;
using BillingService.Models.Enum;
using BillingService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<InvoiceRepository> _logger;

        public InvoiceRepository(
            BillingDbContext context,
            ILogger<InvoiceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            try
            {
                invoice.InvoiceId = Guid.NewGuid();
                invoice.CreatedAt = DateTime.UtcNow;

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created invoice {InvoiceId} in database", invoice.InvoiceId);
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice in database");
                throw;
            }
        }

        public async Task<Invoice> GetByIdAsync(Guid invoiceId)
        {
            try
            {
                return await _context.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", invoiceId);
                throw;
            }
        }

        public async Task<List<Invoice>> GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            try
            {
                return await _context.Invoices
                    .AsNoTracking()
                    .Where(i => i.SubscriptionId == subscriptionId)
                    .OrderByDescending(i => i.BillingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            try
            {
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated invoice {InvoiceId}", invoice.InvoiceId);
                return invoice;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating invoice {InvoiceId}", invoice.InvoiceId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId}", invoice.InvoiceId);
                throw;
            }
        }

        public async Task<List<Invoice>> GetOverdueInvoicesAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.Invoices
                    .AsNoTracking()
                    .Where(i => i.Status == InvoiceStatus.Pending && i.DueDate < now)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue invoices");
                throw;
            }
        }
    }
}
