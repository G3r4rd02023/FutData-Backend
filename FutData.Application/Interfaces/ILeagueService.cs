using FutData.Application.DTOs;
using FutData.Domain.Entities;

namespace FutData.Application.Interfaces
{
    public interface ILeagueService
    {
        Task<LeagueDetailDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<LeagueDto>> GetAllAsync();
        Task<IEnumerable<LeagueDto>> SearchAsync(string searchTerm);
        Task<LeagueDetailDto> CreateAsync(CreateLeagueDto dto);
        Task<LeagueDetailDto?> UpdateAsync(Guid id, UpdateLeagueDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<LeagueDetailDto?> AddTeamAsync(Guid leagueId, Guid teamId);
        Task<LeagueDetailDto?> RemoveTeamAsync(Guid leagueId, Guid teamId);
        Task<LeagueDetailDto?> UpdateStatusAsync(Guid leagueId, UpdateLeagueStatusDto dto);
        Task<bool> HasMatchesAsync(Guid leagueId);
    }
}
