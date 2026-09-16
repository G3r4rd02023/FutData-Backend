namespace FutData.Application.DTOs
{
    public class GeneralRankingDto
    {
        public int Rank { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? TeamLogo { get; set; }
        public int TotalPoints { get; set; }
        public int TotalMatches { get; set; }
        public int TotalWon { get; set; }
        public decimal WinRate { get; set; }
        public decimal AvgGoals { get; set; }
        public int LeaguesCount { get; set; }
    }
}
