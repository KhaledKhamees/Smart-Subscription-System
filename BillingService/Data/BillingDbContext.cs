using BillingService.Models;
using BillingService.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data
{
    public class BillingDbContext : DbContext
    {
        public BillingDbContext(DbContextOptions<BillingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<BillingCycle> BillingCycles { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Idempotency> IdempotencyKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Invoice entity
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.SubscriptionId)
                    .IsRequired();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.HasIndex(e => e.SubscriptionId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.Status, e.DueDate });
            });

            // Configure BillingCycle entity
            modelBuilder.Entity<BillingCycle>(entity =>
            {
                entity.HasKey(e => e.BillingCycleId);

                entity.Property(e => e.Period)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.SubscriptionId)
                    .IsRequired();

                entity.HasIndex(e => e.SubscriptionId);
                entity.HasIndex(e => new { e.SubscriptionId, e.StartDate, e.EndDate });
            });

            // Configure SubscriptionPlan entity
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(e => e.PlanId);

                

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.BillingPeriod)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                
            });

        }
    }
}
