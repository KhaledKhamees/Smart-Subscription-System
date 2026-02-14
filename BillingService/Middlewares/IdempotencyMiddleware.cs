using BillingService.Data;
using BillingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Middlewares
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Logger<IdempotencyMiddleware> _logger;
        public IdempotencyMiddleware(RequestDelegate next, Logger<IdempotencyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, BillingDbContext dbContext)
        {
            // Only apply to webhook endpoints
            if (!context.Request.Path.StartsWithSegments("/api/webhooks/stripe"))
            {
                await _next(context);
                return;
            }

            // Read the request body
            context.Request.EnableBuffering();
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Generate idempotency key from Stripe event ID
            var idempotencyKey = ExtractStripeEventId(body);

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                _logger.LogWarning("Unable to extract Stripe event ID for idempotency");
                await _next(context);
                return;
            }

            // Check if already processed
            var existingRecord = await dbContext.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Key == idempotencyKey);

            if (existingRecord != null)
            {
                _logger.LogInformation("Duplicate webhook event detected: {EventId}. Returning cached response.", idempotencyKey);
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(existingRecord.Response ?? "OK");
                return;
            }

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Store idempotency record if successful
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                var record = new IdempotencyRecord
                {
                    Key = idempotencyKey,
                    EventType = ExtractEventType(body),
                    ProcessedAt = DateTime.UtcNow,
                    Response = responseText
                };

                dbContext.IdempotencyRecords.Add(record);
                await dbContext.SaveChangesAsync();
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }

        private string ExtractStripeEventId(string body)
        {
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(body);
                return json.RootElement.GetProperty("id").GetString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ExtractEventType(string body)
        {
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(body);
                return json.RootElement.GetProperty("type").GetString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
