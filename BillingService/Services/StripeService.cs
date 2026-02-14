using BillingService.Configuration;
using BillingService.Services.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace BillingService.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeOptions _options;
        private readonly ILogger<StripeService> _logger;
        public StripeService(IOptions<StripeOptions> options, ILogger<StripeService> logger)
        {
            _options = options.Value;
            _logger = logger;
            StripeConfiguration.ApiKey = _options.SecretKey;
        }
        public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                var subscription = await service.CancelAsync(subscriptionId);

                _logger.LogInformation("Canceled Stripe subscription {SubscriptionId}", subscriptionId);
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<Session> CreateCheckoutSessionAsync(string customerId, string priceId, Guid subscriptionId)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    Customer = customerId,
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    SuccessUrl = $"{_options.WebhookBaseUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{_options.WebhookBaseUrl}/payment/cancel",
                    Metadata = new Dictionary<string, string>
                    {
                        { "subscription_id", subscriptionId.ToString() }
                    }
                };
                var service = new SessionService();
                var session = await service.CreateAsync(options);
                _logger.LogInformation("Created Stripe checkout session for CustomerId: {CustomerId}, PriceId: {PriceId}, SessionId: {SessionId}", customerId, priceId, session.Id);
                return session;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout session for CustomerId: {CustomerId}, PriceId: {PriceId}", customerId, priceId);
                throw;
            }
        }

        public async Task<Customer> CreateOrGetCustomerAsync(Guid userId, string email)
        {
            try
            {
                var customerService = new CustomerService();

                var searchOptions = new CustomerSearchOptions
                {
                    Query = $"metadata['user_id']:'{userId}'"
                };
                var existingCustomers = await customerService.SearchAsync(searchOptions);
                if (existingCustomers.Data.Count > 0)
                {
                    _logger.LogInformation("Found existing Stripe customer for UserId: {UserId}", userId);
                    return existingCustomers.Data[0];
                }
                var CreateOptions = new CustomerCreateOptions
                {
                    Email = email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userId.ToString() }
                    }
                };
                var customer = await customerService.CreateAsync(CreateOptions);
                _logger.LogInformation("Created new Stripe customer for UserId: {UserId}, CustomerId: {CustomerId}", userId, customer.Id);

                return customer;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, int trialDays)
        {
            try
            {
                var options = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions { Price = priceId }
                    },
                    TrialPeriodDays = trialDays > 0 ? trialDays : null,
                    PaymentBehavior = "default_incomplete",
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        SaveDefaultPaymentMethod = "on_subscription"
                    }
                };

                var service = new SubscriptionService();
                var subscription = await service.CreateAsync(options);

                _logger.LogInformation("Created Stripe subscription {SubscriptionId} for customer {CustomerId}",
                    subscription.Id, customerId);

                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating subscription for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PaymentIntent> RetryPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.ConfirmAsync(paymentIntentId);

                _logger.LogInformation("Retried payment intent {PaymentIntentId}", paymentIntentId);
                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error retrying payment {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }
    }
}
