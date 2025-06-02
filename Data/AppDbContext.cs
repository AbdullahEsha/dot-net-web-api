using Microsoft.EntityFrameworkCore;
using dot_net_web_api.Models.Entities;

namespace dot_net_web_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Description)
                    .IsRequired();
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.StockQuantity)
                    .HasDefaultValue(0);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.PasswordHash)
                    .IsRequired();
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasConversion<string>() // Store enum as string
                    .HasMaxLength(20);
                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                // Create unique indexes
                entity.HasIndex(e => e.Username)
                    .IsUnique();
                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.ExpiryDate)
                    .IsRequired();
                entity.Property(e => e.IsRevoked)
                    .IsRequired();
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .IsRequired();

                // Configure relationship
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create indexes
                entity.HasIndex(e => e.Token)
                    .IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.IsRevoked });
            });
        }
    }
}