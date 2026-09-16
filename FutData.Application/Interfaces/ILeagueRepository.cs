using FutData.Domain.Entities;

namespace FutData.Application.Interfaces
{
    public interface ILeagueRepository
    {
        Task<League?> GetByIdAsync(Guid id);
        Task<League?> GetByNameAsync(string name);
        Task<IEnumerable<League>> GetAllAsync();
        Task<IEnumerable<League>> SearchAsync(string searchTerm);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
        Task<int> GetTeamCountAsync(Guid leagueId);
        Task<List<Team>> GetTeamsByLeagueAsync(Guid leagueId);
        Task<bool> LeagueHasTeamAsync(Guid leagueId, Guid teamId);
        Task<Team?> GetTeamByIdAsync(Guid teamId);
        Task AddTeamToLeagueAsync(LeagueTeam leagueTeam);
        Task RemoveTeamFromLeagueAsync(LeagueTeam leagueTeam);
        Task<bool> HasMatchesAsync(Guid leagueId);
        Task AddAsync(League league);
        Task UpdateAsync(League league);
        Task DeleteAsync(League league);
    }
}
