using BillingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data
{
    public class BillingDbContext : DbContext
    {
        public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
        {
        }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SubscriptionBilling> SubscriptionBillings { get; set; }
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasPrecision(18, 2);
                entity.Property(p => p.Currency).HasMaxLength(3).IsRequired();
                entity.Property(p => p.StripePaymentIntentId).HasMaxLength(255);
                entity.Property(p => p.StripeInvoiceId).HasMaxLength(255);

                // Indexes for common queries
                entity.HasIndex(p => p.SubscriptionId);
                entity.HasIndex(p => p.StripePaymentIntentId);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.NextRetryAt);
            });

            modelBuilder.Entity<SubscriptionBilling>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.StripeCustomerId).HasMaxLength(255);
                entity.Property(s => s.StripeSubscriptionId).HasMaxLength(255);

                // Indexes
                entity.HasIndex(s => s.UserId);
                entity.HasIndex(s => s.Status);
                entity.HasIndex(s => s.StripeCustomerId);
                entity.HasIndex(s => s.GracePeriodEndDate);
            });

            modelBuilder.Entity<IdempotencyRecord>(entity =>
            {
                entity.HasKey(i => i.Key);
                entity.Property(i => i.EventType).HasMaxLength(100).IsRequired();
                entity.HasIndex(i => i.ProcessedAt);
            });
        }

    }
}
