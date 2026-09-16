namespace FutData.Application.DTOs
{
    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public List<ActiveLeagueDto> ActiveLeagues { get; set; } = new();
        public List<RecentMatchDto> RecentMatches { get; set; } = new();
        public List<UpcomingMatchDto> UpcomingMatches { get; set; } = new();
        public List<TopTeamDto> TopTeams { get; set; } = new();
        public DashboardChartsDto Charts { get; set; } = new();
    }

    public class DashboardSummaryDto
    {
        public int TotalTeams { get; set; }
        public int TotalLeagues { get; set; }
        public int TotalMatches { get; set; }
        public int MatchesPlayed { get; set; }
        public int MatchesPending { get; set; }
    }

    public class ActiveLeagueDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TeamsCount { get; set; }
        public int MatchesCount { get; set; }
    }

    public class RecentMatchDto
    {
        public Guid Id { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string? HomeTeamLogo { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public string? AwayTeamLogo { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public DateTime MatchDate { get; set; }
        public string LeagueName { get; set; } = string.Empty;
    }

    public class UpcomingMatchDto
    {
        public Guid Id { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string? HomeTeamLogo { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public string? AwayTeamLogo { get; set; }
        public DateTime MatchDate { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public int? Round { get; set; }
    }

    public class TopTeamDto
    {
        public int Rank { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? TeamLogo { get; set; }
        public int Points { get; set; }
        public decimal WinRate { get; set; }
    }

    public class DashboardChartsDto
    {
        public List<MonthlyDataDto> MatchesByMonth { get; set; } = new();
        public ResultDistributionDto ResultDistribution { get; set; } = new();
    }

    public class MonthlyDataDto
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ResultDistributionDto
    {
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
    }
}
