using NotificationService.Sync_communication.Interfaces;
using NotificationService.Sync_communication.Summaries;

namespace NotificationService.Sync_communication.HttpClients
{
    public class UserClient : IUserClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserClient> _logger;
        public UserClient(HttpClient httpClient, ILogger<UserClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<UserSummary?> GetUserSummaryAsync(Guid userId)
        {
            
            try
            {
                using var response = await _httpClient.GetAsync($"/api/User/userEmail/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var userSummary = await response.Content.ReadFromJsonAsync<UserSummary>();
                    return userSummary;
                }
                else
                {
                    _logger.LogError("Failed to retrieve user summary for userId {UserId}. Status Code: {StatusCode}", userId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while retrieving user summary for userId {UserId}", userId);
                return null;
            }
        }
    }
}
