using System.Text.Json.Serialization;
using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class MatchDto
    {
        public Guid Id { get; set; }
        public Guid LeagueId { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public Guid HomeTeamId { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public string? HomeTeamLogo { get; set; }
        public Guid AwayTeamId { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
        public string? AwayTeamLogo { get; set; }
        public int? HomeGoals { get; set; }
        public int? AwayGoals { get; set; }
        public DateTime MatchDate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchStatus Status { get; set; }
        public int? Round { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
