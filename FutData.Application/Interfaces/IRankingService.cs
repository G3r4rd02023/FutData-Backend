using FutData.Application.DTOs;

namespace FutData.Application.Interfaces
{
    public interface IRankingService
    {
        Task<List<StandingDto>> GetLeagueStandingsAsync(Guid leagueId);
        Task<List<GeneralRankingDto>> GetGeneralRankingAsync();
        Task<List<GeneralRankingDto>> GetTopTeamsAsync(int count = 10);
        Task<TeamStatsDto?> GetTeamStatsAsync(Guid teamId);
    }
}
