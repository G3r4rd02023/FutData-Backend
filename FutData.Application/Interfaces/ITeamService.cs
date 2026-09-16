using FutData.Application.DTOs;

namespace FutData.Application.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDto?> GetByIdAsync(Guid id);
        Task<List<TeamDto>> GetAllAsync();
        Task<List<TeamDto>> SearchAsync(string? searchTerm);
        Task<PaginatedResult<TeamDto>> GetAllPagedAsync(int page, int pageSize, string? searchTerm = null);
        Task<TeamDto> CreateAsync(CreateTeamDto dto);
        Task<TeamDto> UpdateAsync(Guid id, UpdateTeamDto dto);
        Task DeleteAsync(Guid id);
        Task<TeamDto> UpdateLogoAsync(Guid id, string logoUrl);
    }
}
