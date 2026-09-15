using FutData.Domain.Enums;

namespace FutData.Domain.Entities
{
    public class Match
    {
        public Guid Id { get; set; }
        public Guid LeagueId { get; set; }
        public Guid HomeTeamId { get; set; }
        public Guid AwayTeamId { get; set; }
        public int? HomeGoals { get; set; }
        public int? AwayGoals { get; set; }
        public DateTime MatchDate { get; set; }
        public MatchStatus Status { get; set; }
        public int? Round { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Team HomeTeam { get; set; } = null!;
        public Team AwayTeam { get; set; } = null!;
    }
}
