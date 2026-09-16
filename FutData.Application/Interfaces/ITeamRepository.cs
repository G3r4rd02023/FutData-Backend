using FutData.Domain.Entities;

namespace FutData.Application.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(Guid id);
        Task<List<Team>> GetAllAsync();
        Task<List<Team>> SearchAsync(string? searchTerm);
        Task<(List<Team> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, string? searchTerm = null);
        Task AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(Team team);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
        Task<int> GetMatchCountAsync(Guid teamId);
    }
}
