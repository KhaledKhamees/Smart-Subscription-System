using BillingService.Data;
using BillingService.Models;
using BillingService.Models.Enum;
using BillingService.Repositories.Interfaces;
using BillingService.Services.Interfaces;

namespace BillingService.Services
{
    public class BillingServiceImpl : IBillingService
    {
        private readonly BillingDbContext _context;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IBillingCycleRepository _billingCycleRepository;
        private readonly ILogger<BillingServiceImpl> _logger;

        public BillingServiceImpl(
            BillingDbContext context,
            IInvoiceRepository invoiceRepository,
            IBillingCycleRepository billingCycleRepository,
            ILogger<BillingServiceImpl> logger)
        {
            _context = context;
            _invoiceRepository = invoiceRepository;
            _billingCycleRepository = billingCycleRepository;
            _logger = logger;
        }

        public async Task ProcessSubscriptionCreatedAsync(Guid subscriptionId, Guid userId, Guid planId, DateTime startDate ,decimal Price, int TrialDays , BillingPeriod BillingPeriod)
        {
            _logger.LogInformation("Processing billing for new subscription {SubscriptionId}", subscriptionId);

            // Use a transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Calculate trial end date
                var trialEndDate = startDate.AddDays(TrialDays);

                // Create the first billing cycle
                var firstBillingCycle = new BillingCycle
                {
                    SubscriptionId = subscriptionId,
                    StartDate = trialEndDate,
                    UserId = userId,
                    EndDate = CalculateEndDate(trialEndDate, BillingPeriod),
                    Period = BillingPeriod
                };
                await _billingCycleRepository.CreateAsync(firstBillingCycle);

                _logger.LogInformation("Created billing cycle for subscription {SubscriptionId}. Trial ends: {TrialEnd}, First billing: {BillingStart}",
                    subscriptionId, trialEndDate, firstBillingCycle.StartDate);

                // Create the first invoice (due after trial)
                var firstInvoice = new Invoice
                {
                    SubscriptionId = subscriptionId,
                    UserId = userId,
                    Amount = Price,
                    BillingDate = trialEndDate,
                    DueDate = trialEndDate.AddDays(7), // 7 days to pay
                    Status = InvoiceStatus.Pending
                };
                await _invoiceRepository.CreateAsync(firstInvoice);

                _logger.LogInformation("Created first invoice {InvoiceId} for subscription {SubscriptionId}. Amount: {Amount}, Due: {DueDate}",
                    firstInvoice.InvoiceId, subscriptionId, firstInvoice.Amount, firstInvoice.DueDate);

                // Commit the transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed for subscription {SubscriptionId}", subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription creation for {SubscriptionId}, rolling back transaction", subscriptionId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ProcessSubscriptionCanceledAsync(Guid subscriptionId, DateTime canceledAt)
        {
            _logger.LogInformation("Processing cancellation for subscription {SubscriptionId}", subscriptionId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get all pending invoices for this subscription
                var invoices = await _invoiceRepository.GetBySubscriptionIdAsync(subscriptionId);
                var pendingInvoices = invoices.Where(i => i.Status == InvoiceStatus.Pending).ToList();

                foreach (var invoice in pendingInvoices)
                {
                    // Cancel future invoices (those with billing date after cancellation)
                    if (invoice.BillingDate > canceledAt)
                    {
                        invoice.Status = InvoiceStatus.Canceled;
                        await _invoiceRepository.UpdateAsync(invoice);
                        _logger.LogInformation("Canceled invoice {InvoiceId} for subscription {SubscriptionId}",
                            invoice.InvoiceId, subscriptionId);
                    }
                    else
                    {
                        // Keep invoices that were already due
                        _logger.LogInformation("Invoice {InvoiceId} remains pending (already due before cancellation)",
                            invoice.InvoiceId);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed for cancellation of subscription {SubscriptionId}", subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription cancellation for {SubscriptionId}, rolling back transaction", subscriptionId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ProcessPaymentAsync(Guid invoiceId)
        {
            _logger.LogInformation("Processing payment for invoice {InvoiceId}", invoiceId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogError("Invoice {InvoiceId} not found", invoiceId);
                    throw new InvalidOperationException($"Invoice {invoiceId} not found");
                }

                if (invoice.Status == InvoiceStatus.Paid)
                {
                    _logger.LogWarning("Invoice {InvoiceId} is already paid", invoiceId);
                    return;
                }

                // Mark invoice as paid
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidAt = DateTime.UtcNow;
                await _invoiceRepository.UpdateAsync(invoice);

                _logger.LogInformation("Invoice {InvoiceId} marked as paid. Amount: {Amount}", invoiceId, invoice.Amount);

                // Get the current billing cycle
                var currentCycle = await _billingCycleRepository.GetActiveBySubscriptionIdAsync(invoice.SubscriptionId);
                if (currentCycle != null)
                {
                    // Get the plan to determine the price for next invoice
                    var allCycles = await _billingCycleRepository.GetBySubscriptionIdAsync(invoice.SubscriptionId);
                    var billingPeriod = currentCycle.Period;

                    // Create the next billing cycle
                    var nextCycle = new BillingCycle
                    {
                        SubscriptionId = invoice.SubscriptionId,
                        StartDate = currentCycle.EndDate.AddDays(1),
                        EndDate = CalculateEndDate(currentCycle.EndDate.AddDays(1), billingPeriod),
                        Period = billingPeriod
                    };
                    await _billingCycleRepository.CreateAsync(nextCycle);

                    _logger.LogInformation("Created next billing cycle for subscription {SubscriptionId}. Period: {Start} to {End}",
                        invoice.SubscriptionId, nextCycle.StartDate, nextCycle.EndDate);

                    // Create the next invoice
                    var nextInvoice = new Invoice
                    {
                        SubscriptionId = invoice.SubscriptionId,
                        UserId = invoice.UserId,
                        Amount = invoice.Amount, // Same amount as previous
                        BillingDate = nextCycle.StartDate,
                        DueDate = nextCycle.StartDate.AddDays(7),
                        Status = InvoiceStatus.Pending
                    };
                    await _invoiceRepository.CreateAsync(nextInvoice);

                    _logger.LogInformation("Created next invoice {InvoiceId} for subscription {SubscriptionId}",
                        nextInvoice.InvoiceId, invoice.SubscriptionId);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed for payment of invoice {InvoiceId}", invoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}, rolling back transaction", invoiceId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task MarkOverdueInvoicesAsync()
        {
            _logger.LogInformation("Checking for overdue invoices");

            var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesAsync();

            if (!overdueInvoices.Any())
            {
                _logger.LogInformation("No overdue invoices found");
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var invoice in overdueInvoices)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                    await _invoiceRepository.UpdateAsync(invoice);

                    _logger.LogWarning("Invoice {InvoiceId} marked as overdue. Subscription: {SubscriptionId}, Amount: {Amount}",
                        invoice.InvoiceId, invoice.SubscriptionId, invoice.Amount);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Marked {Count} invoices as overdue", overdueInvoices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking overdue invoices, rolling back transaction");
                await transaction.RollbackAsync();
                throw;
            }
        }

        private DateTime CalculateEndDate(DateTime startDate, BillingPeriod period)
        {
            return period switch
            {
                BillingPeriod.Monthly => startDate.AddMonths(1).AddDays(-1),
                BillingPeriod.Yearly => startDate.AddYears(1).AddDays(-1),
                _ => throw new ArgumentException($"Unknown billing period: {period}")
            };
        }
    }
}


