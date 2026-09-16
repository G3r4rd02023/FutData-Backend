using System.Text.Json.Serialization;
using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class LeagueDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeagueFormat Format { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LeagueStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TeamsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
