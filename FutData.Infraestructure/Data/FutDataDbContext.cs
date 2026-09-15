using FutData.Domain.Entities;
using FutData.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FutData.Infraestructure.Data
{
    public class FutDataDbContext(DbContextOptions<FutDataDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000001"),
                    Username = "admin",
                    Email = "admin@futdata.com",
                    PasswordHash = "$2a$11$eXpw/b/G2nsh00gYf63bfeG7vU8Zg4E0fF2rA1Yh5qM5C3M9N8fFa",
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2026, 9, 1),
                    UpdatedAt = new DateTime(2026, 9, 1)
                }
            );
        }
    }
}
