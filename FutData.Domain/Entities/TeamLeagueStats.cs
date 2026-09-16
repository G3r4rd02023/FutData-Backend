namespace FutData.Domain.Entities
{
    public class TeamLeagueStats
    {
        public Guid Id { get; set; }
        public Guid TeamId { get; set; }
        public Guid LeagueId { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }

        public Team Team { get; set; } = null!;
        public League League { get; set; } = null!;
    }
}
