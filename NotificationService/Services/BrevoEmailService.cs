using NotificationService.Services.Interfaces;
using System.Runtime;

namespace NotificationService.Services
{
    public class BrevoEmailService : IEmailService
    {
        private readonly ILogger<BrevoEmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public BrevoEmailService(ILogger<BrevoEmailService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }
        public async Task SendSubscriptionExpiryAsync(string email, DateTime endDate)
        {
            var payload = new
            {
                sender = new
                {
                    email = _configuration["Brevo:SenderEmail"],
                    name = _configuration["Brevo:SenderName"]
                },
                to = new[]
            {
                new { email = email }
            },
                subject = "Your Subscription Is Expiring Soon",
                htmlContent = $"""
                <html>
                    <body>
                        <h2>Subscription Expiry Notice</h2>
                        <p>Your subscription will expire on <strong>{endDate:yyyy-MM-dd}</strong>.</p>
                        <p>Please renew to avoid service interruption.</p>
                    </body>
                </html>
                """
            };

            var response = await _httpClient.PostAsJsonAsync("/smtp/email", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo email failed: {Error}", error);
                throw new Exception("Brevo email sending failed.");
            }

            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
    }
}
