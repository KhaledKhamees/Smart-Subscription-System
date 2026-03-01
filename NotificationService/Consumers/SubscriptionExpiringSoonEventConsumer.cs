using MassTransit;
using NotificationService.Data;
using NotificationService.EventContracts;
using NotificationService.Models;
using NotificationService.Services.Interfaces;
using NotificationService.Sync_communication.Interfaces;

namespace NotificationService.Consumers
{
    public class SubscriptionExpiringSoonEventConsumer : IConsumer<SubscriptionExpiringSoonEvent>
    {
        private readonly ILogger<SubscriptionExpiringSoonEventConsumer> _logger;
        private readonly IEmailService _emailService;
        private readonly NotificationDbContext _dbContext;
        private readonly IUserClient _userClient;
        public SubscriptionExpiringSoonEventConsumer(ILogger<SubscriptionExpiringSoonEventConsumer> logger,IEmailService emailService,NotificationDbContext dbContext,IUserClient userClient)
        {
            _logger = logger;
            _emailService = emailService;
            _dbContext = dbContext;
            _userClient = userClient;
        }
        public Task Consume(ConsumeContext<SubscriptionExpiringSoonEvent> context)
        {
            _logger.LogInformation("Received SubscriptionExpiringSoonEvent for SubscriptionId: {SubscriptionId}, UserId: {UserId}, EndDate: {EndDate}",
                context.Message.SubscriptionId, context.Message.UserId, context.Message.EndDate);
            var message = context.Message;

            var exsitingNotification = _dbContext.SentExpiredSubscriptionNotifications.FirstOrDefault(n => n.SubscriptionId == message.SubscriptionId && n.UserId == message.UserId);
            if(exsitingNotification != null && exsitingNotification.SentDate < DateTime.UtcNow.AddDays(30))
            {
                _logger.LogInformation("Notification for SubscriptionId: {SubscriptionId}, UserId: {UserId} was already sent on {SentDate}. Skipping email.",
                    message.SubscriptionId, message.UserId, exsitingNotification.SentDate);
                return Task.CompletedTask;
            }
            var user = _userClient.GetUserSummaryAsync(message.UserId);
            if(user == null)
            {
                _logger.LogWarning("User with Id: {UserId} not found. Cannot send notification for SubscriptionId: {SubscriptionId}.",
                    message.UserId, message.SubscriptionId);
                return  Task.CompletedTask;
            }
            var email = user.Result.Email;
            var subject = "Your subscription is expiring soon";
            var body = $"Dear user, your subscription with ID {message.SubscriptionId} is expiring on {message.EndDate:MMMM dd, yyyy}. Please renew it to continue enjoying our services.";
            _emailService.SendSubscriptionExpiryAsync(email, message.EndDate);
            _dbContext.SentExpiredSubscriptionNotifications.Add(new SentExpiredSubscriptionNotification
            {
                SubscriptionId = message.SubscriptionId,
                UserId = message.UserId,
                SentDate = DateTime.UtcNow
            });


            return Task.CompletedTask;
        }
    }
}
