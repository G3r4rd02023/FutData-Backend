using FutData.Application.DTOs;

namespace FutData.Application.Interfaces
{
    public interface IMatchService
    {
        Task<MatchDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<MatchDto>> GetAllAsync(MatchFilterDto? filters);
        Task<PaginatedResult<MatchDto>> GetAllPagedAsync(int page, int pageSize, MatchFilterDto? filters = null);
        Task<MatchDto> CreateAsync(CreateMatchDto dto);
        Task<MatchDto?> UpdateAsync(Guid id, UpdateMatchDto dto);
        Task<MatchDto?> RegisterResultAsync(Guid id, ResultDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<MatchDto?> UpdateStatusAsync(Guid id, string status);
        Task<List<TeamLeagueStatsDto>> GetLeagueStatsAsync(Guid leagueId);
    }
}
