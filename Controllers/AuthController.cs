using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using dot_net_web_api.Models.DTOs;
using dot_net_web_api.Services;

namespace dot_net_web_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = GetIpAddress();
                var response = await _authService.RegisterAsync(request, ipAddress);

                return Ok(new
                {
                    message = "User registered successfully.",
                    data = response
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during registration.", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = GetIpAddress();
                var response = await _authService.LoginAsync(request, ipAddress);

                return Ok(new
                {
                    message = "Login successful.",
                    data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during login.", error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = GetIpAddress();
                var response = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                return Ok(new
                {
                    message = "Token refreshed successfully.",
                    data = response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while refreshing token.", error = ex.Message });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = GetIpAddress();
                var result = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

                if (!result)
                    return BadRequest(new { message = "Invalid or expired refresh token." });

                return Ok(new { message = "Token revoked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while revoking token.", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetCurrentUserId();
                await _authService.ChangePasswordAsync(userId, request);

                return Ok(new { message = "Password changed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing password.", error = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                    return NotFound(new { message = "User not found." });

                return Ok(new
                {
                    message = "Profile retrieved successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving profile.", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during logout.", error = ex.Message });
            }
        }

        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            var userId = GetCurrentUserId();
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "Token is valid.",
                data = new
                {
                    userId,
                    username,
                    email,
                    role
                }
            });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }
            return userId;
        }

        private string GetIpAddress()
        {
            // Try to get IP from X-Forwarded-For header (for reverse proxy scenarios)
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            // Try to get IP from X-Real-IP header
            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // Fall back to RemoteIpAddress
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}