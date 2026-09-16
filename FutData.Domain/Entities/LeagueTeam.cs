namespace FutData.Domain.Entities
{
    public class LeagueTeam
    {
        public Guid Id { get; set; }
        public Guid LeagueId { get; set; }
        public Guid TeamId { get; set; }
        public DateTime JoinedAt { get; set; }

        public League League { get; set; } = null!;
        public Team Team { get; set; } = null!;
    }
}
