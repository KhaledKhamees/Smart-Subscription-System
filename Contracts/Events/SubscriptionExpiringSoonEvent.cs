using System;
namespace Contracts.Events
{
    public class SubscriptionExpiringSoonEvent
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime EndDate { get; set; }

    }
}
