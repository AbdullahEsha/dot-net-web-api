using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using dot_net_web_api.Models.Configuration;
using dot_net_web_api.Models.DTOs;
using dot_net_web_api.Models.Entities;
using dot_net_web_api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace dot_net_web_api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<RefreshToken> refreshTokenRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            // Check if user already exists
            var existingUsers = await _userRepository.GetAllAsync();
            var existingUser = existingUsers.FirstOrDefault(u => 
                u.Username.ToLower() == request.Username.ToLower() || 
                u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                if (existingUser.Username.ToLower() == request.Username.ToLower())
                    throw new InvalidOperationException("Username already exists.");
                if (existingUser.Email.ToLower() == request.Email.ToLower())
                    throw new InvalidOperationException("Email already exists.");
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Enums.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            // Generate tokens
            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
        {
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => 
                u.Username.ToLower() == request.UsernameOrEmail.ToLower() || 
                u.Email.ToLower() == request.UsernameOrEmail.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username/email or password.");
            }

            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var refreshTokens = await _refreshTokenRepository.GetAllAsync();
            var storedRefreshToken = refreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);

            if (storedRefreshToken == null || !storedRefreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var user = await _userRepository.GetByIdAsync(storedRefreshToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Revoke the old refresh token
            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.RevokedByIp = ipAddress;

            _refreshTokenRepository.Update(storedRefreshToken);
            await _refreshTokenRepository.SaveAsync();

            // Generate new tokens
            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            var refreshTokens = await _refreshTokenRepository.GetAllAsync();
            var storedRefreshToken = refreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);

            if (storedRefreshToken == null || !storedRefreshToken.IsActive)
            {
                return false;
            }

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.RevokedByIp = ipAddress;

            _refreshTokenRepository.Update(storedRefreshToken);
            await _refreshTokenRepository.SaveAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            return true;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string ipAddress)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

            return new AuthResponse
            {
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiry = accessToken.Expiry,
                RefreshTokenExpiry = refreshToken.ExpiryDate,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };
        }

        private (string Token, DateTime Expiry) GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRandomToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveAsync();

            return refreshToken;
        }

        private static string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}