using Microsoft.EntityFrameworkCore;

namespace SubscriptionService.Data
{
    public class SubscriptionDbContext : DbContext
    {
        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options): base(options)
        { }
        public DbSet<Entities.Subscription> Subscriptions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Entities.Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.PlanId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.NextBillingDate).IsRequired();
            });
        }
    }
}
