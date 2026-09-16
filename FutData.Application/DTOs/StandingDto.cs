using System.Text.Json.Serialization;
using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class StandingDto
    {
        public int Position { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? TeamLogo { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points { get; set; }
        public string Form { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PositionChange PositionChange { get; set; }
    }

    public enum PositionChange
    {
        Sube,
        Baja,
        Igual
    }
}
