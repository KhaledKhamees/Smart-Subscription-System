
using BillingService.Repositories.Interfaces;
using BillingService.Services.Interfaces;

namespace BillingService.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ILogger<IdempotencyService> _logger;
        private readonly IIdempotencyRepository _idempotencyRepository;
        public IdempotencyService(
            IIdempotencyRepository idempotencyRepository,
            ILogger<IdempotencyService> logger)
        {
            _idempotencyRepository = idempotencyRepository;
            _logger = logger;
        }

        public Task AddRequestAsync(Guid SubscriptionId)
        {
            return _idempotencyRepository.AddRequestAsync(SubscriptionId);
        }

        public Task<bool> IsDuplicateRequestAsync(Guid SubscriptionId)
        {
            return _idempotencyRepository.IsDuplicateRequestAsync(SubscriptionId);
        }
    }
}
