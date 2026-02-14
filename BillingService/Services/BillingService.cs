using BillingService.Data;
using BillingService.Entities;
using BillingService.Entities.Enums;
using BillingService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace BillingService.Services
{
    public class BillingService : IBillingService
    {
        private readonly BillingDbContext _dbContext;
        private readonly IStripeService _stripeService;
        private readonly ILogger<BillingService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        private const int MaxConsecutiveFailures = 3;
        private const int GracePeriodDays = 7;
        private static readonly TimeSpan[] RetryIntervals = new[]
        {
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(3),
            TimeSpan.FromDays(5)
        };

        public BillingService(
            BillingDbContext dbContext,
            IStripeService stripeService,
            ILogger<BillingService> logger)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _logger = logger;

            // Configure Polly retry policy for transient failures
            _retryPolicy = Policy
                .Handle<DbUpdateException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Retry {RetryCount} after {Delay}s due to {ExceptionType}",
                            retryCount, timeSpan.TotalSeconds, exception.GetType().Name);
                    });
        }

        public async Task<SubscriptionBilling> CreateSubscriptionBillingAsync(
            Guid subscriptionId, Guid userId, Guid planId, DateTime startDate)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var subscription = new SubscriptionBilling
                {
                    Id = subscriptionId,
                    UserId = userId,
                    PlanId = planId,
                    Status = SubscriptionLifecycleStatus.Trial,
                    StartDate = startDate,
                    TrialEndDate = startDate.AddDays(14), // 14-day trial
                    CurrentPeriodStart = startDate,
                    CurrentPeriodEnd = startDate.AddDays(14),
                    ConsecutiveFailures = 0
                };

                _dbContext.SubscriptionBillings.Add(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Created billing subscription {SubscriptionId} for user {UserId} with trial until {TrialEnd}",
                    subscriptionId, userId, subscription.TrialEndDate);

                return subscription;
            });
        }

        public async Task<Payment> CreatePaymentRecordAsync(Guid subscriptionId, decimal amount, string stripeInvoiceId)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscriptionId,
                    Amount = amount,
                    Status = PaymentStatus.Pending,
                    StripeInvoiceId = stripeInvoiceId,
                    CreatedAt = DateTime.UtcNow,
                    RetryCount = 0
                };

                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Created payment record {PaymentId} for subscription {SubscriptionId}, amount: {Amount}",
                    payment.Id, subscriptionId, amount);

                return payment;
            });
        }

        public async Task UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? failureReason = null)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var payment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for PaymentIntent {PaymentIntentId}", paymentIntentId);
                    return;
                }

                payment.Status = status;

                if (status == PaymentStatus.Succeeded)
                {
                    payment.CompletedAt = DateTime.UtcNow;
                }
                else if (status == PaymentStatus.Failed)
                {
                    payment.FailedAt = DateTime.UtcNow;
                    payment.FailureReason = failureReason;
                }

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated payment {PaymentId} status to {Status}",
                    payment.Id, status);
            });
        }

        public async Task HandlePaymentSuccessAsync(string paymentIntentId)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var payment = await _dbContext.Payments
                    .Include(p => p.Subscription)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for successful PaymentIntent {PaymentIntentId}", paymentIntentId);
                    return;
                }

                // Update payment status
                payment.Status = PaymentStatus.Succeeded;
                payment.CompletedAt = DateTime.UtcNow;

                // Update subscription status
                var subscription = payment.Subscription;
                subscription.ConsecutiveFailures = 0;
                subscription.GracePeriodEndDate = null;
                subscription.LastPaymentAttempt = DateTime.UtcNow;

                // Transition from trial to active if applicable
                if (subscription.Status == SubscriptionLifecycleStatus.Trial &&
                    DateTime.UtcNow >= subscription.TrialEndDate)
                {
                    subscription.Status = SubscriptionLifecycleStatus.Active;
                    _logger.LogInformation("Subscription {SubscriptionId} transitioned from Trial to Active",
                        subscription.Id);
                }
                else if (subscription.Status == SubscriptionLifecycleStatus.GracePeriod ||
                         subscription.Status == SubscriptionLifecycleStatus.PastDue)
                {
                    subscription.Status = SubscriptionLifecycleStatus.Active;
                    _logger.LogInformation("Subscription {SubscriptionId} recovered from {OldStatus} to Active",
                        subscription.Id, subscription.Status);
                }

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully processed payment {PaymentId} for subscription {SubscriptionId}",
                    payment.Id, subscription.Id);
            });
        }

        public async Task HandlePaymentFailureAsync(string paymentIntentId, string reason)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var payment = await _dbContext.Payments
                    .Include(p => p.Subscription)
                    .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for failed PaymentIntent {PaymentIntentId}", paymentIntentId);
                    return;
                }

                // Update payment
                payment.Status = PaymentStatus.Failed;
                payment.FailedAt = DateTime.UtcNow;
                payment.FailureReason = reason;
                payment.RetryCount++;

                var subscription = payment.Subscription;
                subscription.ConsecutiveFailures++;
                subscription.LastPaymentAttempt = DateTime.UtcNow;

                // Determine next action based on failure count
                if (payment.RetryCount < RetryIntervals.Length)
                {
                    // Schedule retry
                    payment.NextRetryAt = DateTime.UtcNow.Add(RetryIntervals[payment.RetryCount]);
                    subscription.Status = SubscriptionLifecycleStatus.PastDue;

                    _logger.LogWarning(
                        "Payment {PaymentId} failed (attempt {Attempt}). Next retry at {NextRetry}",
                        payment.Id, payment.RetryCount, payment.NextRetryAt);
                }
                else if (subscription.ConsecutiveFailures < MaxConsecutiveFailures)
                {
                    // Enter grace period
                    subscription.Status = SubscriptionLifecycleStatus.GracePeriod;
                    subscription.GracePeriodEndDate = DateTime.UtcNow.AddDays(GracePeriodDays);

                    _logger.LogWarning(
                        "Subscription {SubscriptionId} entered grace period until {GraceEnd}",
                        subscription.Id, subscription.GracePeriodEndDate);
                }
                else
                {
                    // Auto-cancel after max failures
                    subscription.Status = SubscriptionLifecycleStatus.Canceled;
                    subscription.CanceledAt = DateTime.UtcNow;

                    // Cancel in Stripe
                    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                    {
                        try
                        {
                            await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to cancel Stripe subscription {StripeSubscriptionId}",
                                subscription.StripeSubscriptionId);
                        }
                    }

                    _logger.LogError(
                        "Subscription {SubscriptionId} auto-canceled after {Failures} consecutive failures",
                        subscription.Id, subscription.ConsecutiveFailures);
                }

                await _dbContext.SaveChangesAsync();
            });
        }

        public async Task ProcessRetryPaymentsAsync()
        {
            var paymentsToRetry = await _dbContext.Payments
                .Include(p => p.Subscription)
                .Where(p => p.Status == PaymentStatus.Failed &&
                           p.NextRetryAt.HasValue &&
                           p.NextRetryAt.Value <= DateTime.UtcNow &&
                           p.RetryCount < RetryIntervals.Length)
                .ToListAsync();

            _logger.LogInformation("Found {Count} payments to retry", paymentsToRetry.Count);

            foreach (var payment in paymentsToRetry)
            {
                try
                {
                    if (string.IsNullOrEmpty(payment.StripePaymentIntentId))
                    {
                        _logger.LogWarning("Payment {PaymentId} has no Stripe PaymentIntent ID", payment.Id);
                        continue;
                    }

                    await _stripeService.RetryPaymentAsync(payment.StripePaymentIntentId);

                    payment.Status = PaymentStatus.Processing;
                    payment.NextRetryAt = null;

                    _logger.LogInformation("Initiated retry for payment {PaymentId}", payment.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrying payment {PaymentId}", payment.Id);
                }
            }

            if (paymentsToRetry.Any())
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task ProcessGracePeriodExpirationsAsync()
        {
            var expiredSubscriptions = await _dbContext.SubscriptionBillings
                .Where(s => s.Status == SubscriptionLifecycleStatus.GracePeriod &&
                           s.GracePeriodEndDate.HasValue &&
                           s.GracePeriodEndDate.Value <= DateTime.UtcNow)
                .ToListAsync();

            _logger.LogInformation("Found {Count} subscriptions with expired grace periods", expiredSubscriptions.Count);

            foreach (var subscription in expiredSubscriptions)
            {
                try
                {
                    subscription.Status = SubscriptionLifecycleStatus.Canceled;
                    subscription.CanceledAt = DateTime.UtcNow;

                    // Cancel in Stripe
                    if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                    {
                        await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
                    }

                    _logger.LogInformation(
                        "Auto-canceled subscription {SubscriptionId} after grace period expiration",
                        subscription.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscription.Id);
                }
            }

            if (expiredSubscriptions.Any())
            {
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task CancelSubscriptionAsync(Guid subscriptionId)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var subscription = await _dbContext.SubscriptionBillings
                    .FirstOrDefaultAsync(s => s.Id == subscriptionId);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription {SubscriptionId} not found for cancellation", subscriptionId);
                    return;
                }

                subscription.Status = SubscriptionLifecycleStatus.Canceled;
                subscription.CanceledAt = DateTime.UtcNow;

                // Cancel in Stripe if exists
                if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
                {
                    try
                    {
                        await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to cancel Stripe subscription {StripeSubscriptionId}",
                            subscription.StripeSubscriptionId);
                        throw;
                    }
                }

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Canceled subscription {SubscriptionId}", subscriptionId);
            });
        }
    }
}

