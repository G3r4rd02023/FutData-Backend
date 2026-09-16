using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;

namespace FutData.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;

        public TeamService(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<TeamDto?> GetByIdAsync(Guid id)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            return team == null ? null : MapToDto(team);
        }

        public async Task<List<TeamDto>> GetAllAsync()
        {
            var teams = await _teamRepository.GetAllAsync();
            return teams.Select(MapToDto).ToList();
        }

        public async Task<List<TeamDto>> SearchAsync(string? searchTerm)
        {
            var teams = await _teamRepository.SearchAsync(searchTerm);
            return teams.Select(MapToDto).ToList();
        }

        public async Task<PaginatedResult<TeamDto>> GetAllPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            var (teams, totalCount) = await _teamRepository.GetAllPagedAsync(page, pageSize, searchTerm);
            return new PaginatedResult<TeamDto>
            {
                Items = teams.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TeamDto> CreateAsync(CreateTeamDto dto)
        {
            if (await _teamRepository.ExistsByNameAsync(dto.Name))
                throw new InvalidOperationException("Ya existe un equipo con ese nombre");

            var team = new Team
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                City = dto.City,
                Stadium = dto.Stadium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _teamRepository.AddAsync(team);
            return MapToDto(team);
        }

        public async Task<TeamDto> UpdateAsync(Guid id, UpdateTeamDto dto)
        {
            var team = await _teamRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Equipo no encontrado");

            if (await _teamRepository.ExistsByNameAsync(dto.Name, id))
                throw new InvalidOperationException("Ya existe un equipo con ese nombre");

            team.Name = dto.Name;
            team.City = dto.City;
            team.Stadium = dto.Stadium;
            team.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.UpdateAsync(team);
            return MapToDto(team);
        }

        public async Task DeleteAsync(Guid id)
        {
            var team = await _teamRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Equipo no encontrado");

            var matchCount = await _teamRepository.GetMatchCountAsync(id);
            if (matchCount > 0)
                throw new InvalidOperationException($"No se puede eliminar: el equipo tiene {matchCount} partido(s) registrado(s)");

            await _teamRepository.DeleteAsync(team);
        }

        public async Task<TeamDto> UpdateLogoAsync(Guid id, string logoUrl)
        {
            var team = await _teamRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Equipo no encontrado");

            team.LogoUrl = logoUrl;
            team.UpdatedAt = DateTime.UtcNow;

            await _teamRepository.UpdateAsync(team);
            return MapToDto(team);
        }

        private static TeamDto MapToDto(Team team)
        {
            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                City = team.City,
                Stadium = team.Stadium,
                LogoUrl = team.LogoUrl,
                CreatedAt = team.CreatedAt
            };
        }
    }
}
