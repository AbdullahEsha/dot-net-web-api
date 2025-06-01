namespace dot_net_web_api.Models.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; set; } = 15; // 15 minutes
        public int RefreshTokenExpirationDays { get; set; } = 7; // 7 days
    }
}