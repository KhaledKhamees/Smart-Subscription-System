using Microsoft.EntityFrameworkCore;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }
        public DbSet<Models.SentExpiredSubscriptionNotification> SentExpiredSubscriptionNotifications { get; set; }
    }
}
