namespace NotificationService.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendSubscriptionExpiryAsync(string email, DateTime endDate);
    }
}
