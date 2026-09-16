using FutData.Application.DTOs;
using FutData.Domain.Entities;

namespace FutData.Application.Interfaces
{
    public interface IMatchRepository
    {
        Task<Match?> GetByIdAsync(Guid id);
        Task<IEnumerable<Match>> GetAllAsync(MatchFilterDto? filters);
        Task<bool> ExistsDuplicateAsync(Guid leagueId, Guid homeTeamId, Guid awayTeamId, DateTime matchDate, Guid? excludeId = null);
        Task<bool> TeamsAreInLeagueAsync(Guid leagueId, Guid homeTeamId, Guid awayTeamId);
        Task<TeamLeagueStats?> GetStatsAsync(Guid teamId, Guid leagueId);
        Task AddAsync(Match match);
        Task UpdateAsync(Match match);
        Task DeleteAsync(Match match);
        Task AddStatsAsync(TeamLeagueStats stats);
        Task UpdateStatsAsync(TeamLeagueStats stats);
    }
}
