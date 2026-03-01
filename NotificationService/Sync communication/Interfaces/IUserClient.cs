using NotificationService.Sync_communication.Summaries;

namespace NotificationService.Sync_communication.Interfaces
{
    public interface IUserClient
    {
        Task<UserSummary?> GetUserSummaryAsync(Guid userId);
    }
}
