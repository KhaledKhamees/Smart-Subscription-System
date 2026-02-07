using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO);
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenDTO refreshToken);
        Task<bool> RevokeTokenAsync(string UserId);
    }
}
