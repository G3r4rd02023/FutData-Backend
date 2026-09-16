using FutData.Domain.Entities;
using FutData.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FutData.Infraestructure.Data
{
    public class FutDataDbContext : DbContext
    {
        public FutDataDbContext(DbContextOptions<FutDataDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<League> Leagues => Set<League>();
        public DbSet<LeagueTeam> LeagueTeams => Set<LeagueTeam>();
        public DbSet<TeamLeagueStats> TeamLeagueStats => Set<TeamLeagueStats>();

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

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Stadium).HasMaxLength(100);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(e => e.HomeTeam)
                    .WithMany()
                    .HasForeignKey(e => e.HomeTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AwayTeam)
                    .WithMany()
                    .HasForeignKey(e => e.AwayTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.League)
                    .WithMany()
                    .HasForeignKey(e => e.LeagueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<League>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Format).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<LeagueTeam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.LeagueId, e.TeamId }).IsUnique();

                entity.HasOne(e => e.League)
                    .WithMany()
                    .HasForeignKey(e => e.LeagueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TeamLeagueStats>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TeamId, e.LeagueId }).IsUnique();

                entity.HasOne(e => e.Team)
                    .WithMany()
                    .HasForeignKey(e => e.TeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.League)
                    .WithMany()
                    .HasForeignKey(e => e.LeagueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
