using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data
{
    public class CatalogDbContext: DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(builder =>
            {
                builder.Property(p => p.BasePrice)
                       .HasPrecision(18, 2);
            });

            modelBuilder.Entity<SubscriptionPlan>(builder =>
            {
                builder.Property(p => p.Price)
                       .HasPrecision(18, 2);
            });
        }

        public DbSet<Entities.Product> Products { get; set; }
        public DbSet<Entities.SubscriptionPlan> SubscriptionPlans { get; set; }
    }
}
