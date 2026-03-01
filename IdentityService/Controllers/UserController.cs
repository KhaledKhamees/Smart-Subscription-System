using IdentityService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        [HttpGet("userEmail/{userId}")]
        public async Task<IActionResult> GetUserEmail(Guid userId)
        {
            var userSummary = await _userService.GetUserEmailAsync(userId);
            if (userSummary == null)
            {
                return NotFound();
            }
            return Ok(userSummary);
        }
    }
}
