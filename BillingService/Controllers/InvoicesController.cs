using BillingService.Models;
using BillingService.Repositories.Interfaces;
using BillingService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(
            IBillingService billingService,
            IInvoiceRepository invoiceRepository,
            ILogger<InvoicesController> logger)
        {
            _billingService = billingService;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all invoices for a subscription
        /// </summary>
        [HttpGet("subscription/{subscriptionId}")]
        [ProducesResponseType(typeof(List<Invoice>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoicesBySubscription(Guid subscriptionId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetBySubscriptionIdAsync(subscriptionId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new { error = "An error occurred while retrieving invoices" });
            }
        }

        /// <summary>
        /// Get a specific invoice by ID
        /// </summary>
        [HttpGet("{invoiceId}")]
        [ProducesResponseType(typeof(Invoice), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoice(Guid invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { error = $"Invoice {invoiceId} not found" });
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { error = "An error occurred while retrieving the invoice" });
            }
        }

        /// <summary>
        /// Process payment for an invoice
        /// </summary>
        [HttpPost("{invoiceId}/pay")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PayInvoice(Guid invoiceId)
        {
            try
            {
                await _billingService.ProcessPaymentAsync(invoiceId);
                return Ok(new { message = "Payment processed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while processing payment for invoice {InvoiceId}", invoiceId);
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while processing payment for invoice {InvoiceId}", invoiceId);
                return Conflict(new { error = "The invoice was modified by another process. Please try again." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while processing payment for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { error = "A database error occurred while processing the payment" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing payment for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { error = "An unexpected error occurred while processing the payment" });
            }
        }

        /// <summary>
        /// Get all overdue invoices
        /// </summary>
        [HttpGet("overdue")]
        [ProducesResponseType(typeof(List<Invoice>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOverdueInvoices()
        {
            try
            {
                var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesAsync();
                return Ok(overdueInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue invoices");
                return StatusCode(500, new { error = "An error occurred while retrieving overdue invoices" });
            }
        }
    }
}
