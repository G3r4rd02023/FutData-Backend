using System.Text.Json.Serialization;

namespace FutData.Application.DTOs
{
    public class SimulationResult
    {
        public Guid MatchId { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string? HomeTeamLogo { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public string? AwayTeamLogo { get; set; }
        public int SimulatedHomeGoals { get; set; }
        public int SimulatedAwayGoals { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SimulatedResult Result { get; set; }
        public SimulationProbabilities Probabilities { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public DateTime SimulatedAt { get; set; }
    }

    public enum SimulatedResult
    {
        Local,
        Empate,
        Visitante
    }

    public class SimulationProbabilities
    {
        public decimal HomeWin { get; set; }
        public decimal Draw { get; set; }
        public decimal AwayWin { get; set; }
    }

    public class BatchSimulationRequest
    {
        public Guid LeagueId { get; set; }
        public int Round { get; set; }
        public List<BatchSimulationItem> Matches { get; set; } = new();
    }

    public class BatchSimulationItem
    {
        public Guid HomeTeamId { get; set; }
        public Guid AwayTeamId { get; set; }
        public decimal HomeWinProbability { get; set; }
        public decimal DrawProbability { get; set; }
        public decimal AwayWinProbability { get; set; }
    }

    public class BatchSimulationResult
    {
        public List<SimulationResult> Results { get; set; } = new();
        public int TotalSimulated { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SimulationHistoryDto
    {
        public Guid Id { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public string Result { get; set; } = string.Empty;
        public string LeagueName { get; set; } = string.Empty;
        public int? Round { get; set; }
        public DateTime SimulatedAt { get; set; }
    }

    public class SimulationStatsDto
    {
        public int TotalSimulations { get; set; }
        public int HomeWins { get; set; }
        public int Draws { get; set; }
        public int AwayWins { get; set; }
        public decimal HomeWinRate { get; set; }
        public decimal DrawRate { get; set; }
        public decimal AwayWinRate { get; set; }
        public decimal AvgHomeGoals { get; set; }
        public decimal AvgAwayGoals { get; set; }
    }
}
