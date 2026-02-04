using SubscriptionService.Data;
using SubscriptionService.Data.Interfaces;
using SubscriptionService.DTOs;
using SubscriptionService.Entities;
using Microsoft.EntityFrameworkCore;


namespace SubscriptionService.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly SubscriptionDbContext _context;
        public SubscriptionRepository(SubscriptionDbContext context)
        {
            _context = context;
        }
        public async Task ActivateAsync(Guid id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                subscription.Activate();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Guid> AddAsync(CreateSubscriptionDTO subscription)
        {
            var entity = new Subscription(subscription.UserId, subscription.PlanId);
            _context.Subscriptions.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task CancelAsync(Guid id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                subscription.Cancel();
                await _context.SaveChangesAsync();
            }
        }

        public async Task<SubscriptionDTO?> GetByIdAsync(Guid id)
        {
            return await _context.Subscriptions
                .Where(s => s.Id == id)
                .Select(s => new SubscriptionDTO
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    PlanId = s.PlanId,
                    Status = s.Status,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    NextBillingDate = s.NextBillingDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<SubscriptionDTO>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .Select(s => new SubscriptionDTO
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    PlanId = s.PlanId,
                    Status = s.Status,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    NextBillingDate = s.NextBillingDate
                })
                .ToListAsync();
        }

        public async Task SuspendAsync(Guid id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription != null)
            {
                subscription.Suspend();
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Guid Id, DateTime nextBillingDate)
        {
            var subscription = await _context.Subscriptions.FindAsync(Id);
            if(subscription != null)
            {
                subscription.Renew(nextBillingDate);
                await _context.SaveChangesAsync();
            }
        }
    }
}
