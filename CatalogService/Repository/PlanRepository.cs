using CatalogService.Data;
using CatalogService.Data.Interfaces;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repository
{
    public class PlanRepository : IPlanRepository
    {
        private readonly CatalogDbContext _context;
        public PlanRepository(CatalogDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(SubscriptionPlanDTO plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }
            var planEntity = new SubscriptionPlan(
                            plan.ProductId,
                            plan.PlanType,
                            plan.Price,
                            plan.TrailDays
                        );

            await _context.SubscriptionPlans.AddAsync(planEntity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
            {
                throw new KeyNotFoundException($"Subscription plan with id {id} not found.");
            }
            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
        {
            var plan = await _context.SubscriptionPlans
                .Where(p => p.Id == id)
                .Where(p => _context.Products.Any(prod => prod.Id == p.ProductId && prod.IsActive))
                .FirstOrDefaultAsync();

            if (plan == null)
            {
                throw new KeyNotFoundException($"Subscription plan with id {id} not found or is associated with an inactive product.");
            }

            return plan;
        }

        public async Task<List<SubscriptionPlan>> GetByProductIdAsync(Guid productId)
        {
            return await _context.SubscriptionPlans
                    .Where(p => p.ProductId == productId)
                    .Where(p => _context.Products.Any(prod => prod.Id == p.ProductId && prod.IsActive))
                    .ToListAsync();
        }

        public Task UpdateAsync(SubscriptionPlanDTO plan)
        {
            var existingPlan = _context.SubscriptionPlans.FirstOrDefault(p => p.Id == plan.ProductId);
            if (existingPlan == null)
            {
                throw new KeyNotFoundException($"Subscription plan with id {plan.ProductId} not found.");
            }
            existingPlan = new SubscriptionPlan(
                plan.ProductId,
                plan.PlanType,
                plan.Price,
                plan.TrailDays
            );
            _context.SubscriptionPlans.Update(existingPlan);
            _context.SaveChangesAsync(); 
            return Task.CompletedTask;
        }
    }
}
