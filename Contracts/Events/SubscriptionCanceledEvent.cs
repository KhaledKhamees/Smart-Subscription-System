using System;

namespace Contracts.Events
{
    public class SubscriptionCanceledEvent
    {
        public Guid SubscriptionId { get; set; }
        public DateTime CanceledAt { get; set; }
    }
}
