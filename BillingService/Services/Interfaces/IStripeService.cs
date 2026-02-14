using Stripe;
using Stripe.Checkout;

namespace BillingService.Services.Interfaces
{
    public interface IStripeService
    {
        Task<Customer> CreateOrGetCustomerAsync(Guid userId, string email);
        Task<Session> CreateCheckoutSessionAsync(string customerId, string priceId, Guid subscriptionId);
        Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, int trialDays);
        Task<Subscription> CancelSubscriptionAsync(string subscriptionId);
        Task<PaymentIntent> RetryPaymentAsync(string paymentIntentId);
    }
}
