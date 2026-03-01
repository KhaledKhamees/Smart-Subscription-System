using IdentityService.DTOs;
using IdentityService.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(ILogger<UserService> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }
         public async Task<UserSummary?> GetUserEmailAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                return null;
            }
            var userSummary = new UserSummary
            {
                UserId = userId,
                Email = user.Email
            };
            return userSummary;
        }
    }
}
