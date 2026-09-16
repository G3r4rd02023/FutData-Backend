using FutData.Domain.Enums;

namespace FutData.Application.DTOs
{
    public class MatchFilterDto
    {
        public Guid? LeagueId { get; set; }
        public Guid? TeamId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public MatchStatus? Status { get; set; }
        public int? Round { get; set; }
    }
}
