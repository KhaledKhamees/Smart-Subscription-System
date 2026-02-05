using SubscriptionService.Sync_communication.Interfaces;
using SubscriptionService.Sync_communication.Summaries;

public class CatalogClient : ICatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogClient> _logger;

    public CatalogClient(HttpClient httpClient, ILogger<CatalogClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PlanSummary?> GetPlanSummaryAsync(Guid planId)
    {
        try
        {
            _logger.LogInformation("Fetching plan summary for PlanId: {PlanId}", planId);

            using var response = await _httpClient.GetAsync($"api/Plan/{planId}");

            if (response.IsSuccessStatusCode)
            {
                var plan = await response.Content.ReadFromJsonAsync<PlanSummary>();
                _logger.LogInformation("Successfully retrieved plan {PlanId}", planId);
                return plan;
            }

            _logger.LogWarning("Plan {PlanId} not found. Status: {Status}",
                planId, response.StatusCode);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for PlanId: {PlanId}", planId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching plan {PlanId}", planId);
            throw;
        }
    }
}