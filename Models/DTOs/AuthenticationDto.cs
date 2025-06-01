using System.ComponentModel.DataAnnotations;

namespace dot_net_web_api.Models.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Username or email is required.")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one digit, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}