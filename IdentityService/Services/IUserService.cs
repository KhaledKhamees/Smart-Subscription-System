using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IUserService
    {
        Task<UserSummary?> GetUserEmailAsync(Guid userId);
    }
}
