// dot_net_web_api/Services/IAuthService.cs
using System.Threading.Tasks;
using dot_net_web_api.Models.DTOs;

namespace dot_net_web_api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress);
        Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<UserDto?> GetUserByIdAsync(int userId);
    }
}