using IdentityService.Configuration;
using IdentityService.DTOs;
using IdentityService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IdentityService.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        public AuthenticationService(UserManager<ApplicationUser> userManager, ITokenService tokenService, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
        }
        
        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingUser != null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "User with this email already exists."
                };
            }
            var user = new ApplicationUser
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName
            };
            var result = _userManager.CreateAsync(user, registerDTO.Password).Result;
            if (!result.Succeeded)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "User registration failed. " + string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }
            await _userManager.AddToRoleAsync(user, "Customer");
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateTokensAsync(user, roles);
            return new AuthResponseDTO
            {
                Success = true,
                Message = "User registered successfully.",
                Tokens = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                }
            };
        }
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null || !_userManager.CheckPasswordAsync(user, loginDTO.Password).Result)
            {
                return  new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }
            if(!user.IsActive)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Account is disabled"
                };
            }
            user.LastLogAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateTokensAsync(user, roles);
            return new AuthResponseDTO
            {
                Success = true,
                Message = "Login successful.",
                Tokens = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshTokenDTO refreshToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(refreshToken.AccessToken);
            var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid access token."
                };
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken!=refreshToken.RefreshToken|| user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid refresh token."
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateTokensAsync(user, roles);
            return new AuthResponseDTO
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Tokens = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                }
            };
        }

        public async Task<bool> RevokeTokenAsync(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return false;
            }
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
            return true;
        }
        private async Task<TokenDTO> GenerateTokensAsync(ApplicationUser user, IList<string> roles)
        {
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _userManager.UpdateAsync(user);

            return new TokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            };
        }
    }
}
