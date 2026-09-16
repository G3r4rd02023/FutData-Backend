namespace FutData.Application.DTOs
{
    public class TeamStatsDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? TeamLogo { get; set; }
        public int TotalMatches { get; set; }
        public int TotalWon { get; set; }
        public int TotalDrawn { get; set; }
        public int TotalLost { get; set; }
        public int TotalGoalsFor { get; set; }
        public int TotalGoalsAgainst { get; set; }
        public int TotalGoalDifference => TotalGoalsFor - TotalGoalsAgainst;
        public int TotalPoints { get; set; }
        public decimal WinRate { get; set; }
        public decimal AvgGoalsFor { get; set; }
        public decimal AvgGoalsAgainst { get; set; }
        public List<LeagueParticipationDto> Leagues { get; set; } = new();
    }

    public class LeagueParticipationDto
    {
        public Guid LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
        public string Form { get; set; } = string.Empty;
    }
}
