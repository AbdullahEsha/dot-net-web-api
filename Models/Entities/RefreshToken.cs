using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dot_net_web_api.Models.Entities
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public bool IsRevoked { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        // Foreign Key
        [Required]
        public int UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // Additional properties for security
        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsActive => !IsRevoked && DateTime.UtcNow <= ExpiryDate;
    }
}