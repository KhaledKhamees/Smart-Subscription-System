namespace BillingService.Configuration
{
    public class StripeOptions
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string WebhookBaseUrl { get; set; } = string.Empty;
    }
}
