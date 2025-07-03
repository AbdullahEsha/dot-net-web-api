using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using dot_net_web_api.Enums;

namespace dot_net_web_api.Models.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public UserRole Role { get; set; } = UserRole.User;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}