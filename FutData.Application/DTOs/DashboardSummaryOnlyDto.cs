namespace FutData.Application.DTOs
{
    public class DashboardSummaryOnlyDto
    {
        public int TotalTeams { get; set; }
        public int TotalLeagues { get; set; }
        public int TotalMatches { get; set; }
        public int MatchesPlayed { get; set; }
        public int MatchesPending { get; set; }
    }
}
