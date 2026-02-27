namespace BillingService.Models
{
    public class Idempotency
    {
        /// <summary>
        /// this is used to ensure that if the same request is sent multiple times (e.g., due to network retries), it will only be processed once.
        /// it impelmented as simple as possible, we can use a database table to store the idempotency keys and their associated responses. When a request comes in, we check if the idempotency key already exists in the database. If it does, we return the stored response instead of processing the request again. If it doesn't, we process the request and store the response along with the idempotency key for future reference.
        [System.ComponentModel.DataAnnotations.Key]
        public Guid SubscriptionId { get; set; }
    }
}
