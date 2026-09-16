using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Domain.Enums;

namespace FutData.Application.Services
{
    public class LeagueService : ILeagueService
    {
        private readonly ILeagueRepository _leagueRepository;

        public LeagueService(ILeagueRepository leagueRepository)
        {
            _leagueRepository = leagueRepository;
        }

        public async Task<LeagueDetailDto?> GetByIdAsync(Guid id)
        {
            var league = await _leagueRepository.GetByIdAsync(id);
            if (league == null) return null;

            var teams = await _leagueRepository.GetTeamsByLeagueAsync(id);
            var teamDtos = teams.Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                City = t.City,
                Stadium = t.Stadium,
                LogoUrl = t.LogoUrl,
                CreatedAt = t.CreatedAt
            }).ToList();

            return new LeagueDetailDto
            {
                Id = league.Id,
                Name = league.Name,
                Description = league.Description,
                Format = league.Format,
                Status = league.Status,
                StartDate = league.StartDate,
                EndDate = league.EndDate,
                Teams = teamDtos,
                CreatedAt = league.CreatedAt
            };
        }

        public async Task<IEnumerable<LeagueDto>> GetAllAsync()
        {
            var leagues = await _leagueRepository.GetAllAsync();
            var result = new List<LeagueDto>();

            foreach (var league in leagues)
            {
                var teamsCount = await _leagueRepository.GetTeamCountAsync(league.Id);

                result.Add(new LeagueDto
                {
                    Id = league.Id,
                    Name = league.Name,
                    Description = league.Description,
                    Format = league.Format,
                    Status = league.Status,
                    StartDate = league.StartDate,
                    EndDate = league.EndDate,
                    TeamsCount = teamsCount,
                    CreatedAt = league.CreatedAt
                });
            }

            return result;
        }

        public async Task<IEnumerable<LeagueDto>> SearchAsync(string searchTerm)
        {
            var leagues = await _leagueRepository.SearchAsync(searchTerm);
            var result = new List<LeagueDto>();

            foreach (var league in leagues)
            {
                var teamsCount = await _leagueRepository.GetTeamCountAsync(league.Id);

                result.Add(new LeagueDto
                {
                    Id = league.Id,
                    Name = league.Name,
                    Description = league.Description,
                    Format = league.Format,
                    Status = league.Status,
                    StartDate = league.StartDate,
                    EndDate = league.EndDate,
                    TeamsCount = teamsCount,
                    CreatedAt = league.CreatedAt
                });
            }

            return result;
        }

        public async Task<LeagueDetailDto> CreateAsync(CreateLeagueDto dto)
        {
            if (await _leagueRepository.ExistsByNameAsync(dto.Name))
                throw new InvalidOperationException("Ya existe una liga con ese nombre");

            var league = new League
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Format = dto.Format,
                Status = LeagueStatus.Programada,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _leagueRepository.AddAsync(league);

            return new LeagueDetailDto
            {
                Id = league.Id,
                Name = league.Name,
                Description = league.Description,
                Format = league.Format,
                Status = league.Status,
                StartDate = league.StartDate,
                EndDate = league.EndDate,
                Teams = new List<TeamDto>(),
                CreatedAt = league.CreatedAt
            };
        }

        public async Task<LeagueDetailDto?> UpdateAsync(Guid id, UpdateLeagueDto dto)
        {
            var league = await _leagueRepository.GetByIdAsync(id);
            if (league == null) return null;

            if (await _leagueRepository.ExistsByNameAsync(dto.Name, id))
                throw new InvalidOperationException("Ya existe una liga con ese nombre");

            league.Name = dto.Name;
            league.Description = dto.Description;
            league.Format = dto.Format;
            league.StartDate = dto.StartDate;
            league.EndDate = dto.EndDate;
            league.UpdatedAt = DateTime.UtcNow;

            await _leagueRepository.UpdateAsync(league);
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var league = await _leagueRepository.GetByIdAsync(id);
            if (league == null) return false;

            if (league.Status == LeagueStatus.Activa)
                throw new InvalidOperationException("No se puede eliminar una liga activa");

            if (await HasMatchesAsync(id))
                throw new InvalidOperationException("No se puede eliminar una liga con partidos registrados");

            await _leagueRepository.DeleteAsync(league);
            return true;
        }

        public async Task<LeagueDetailDto?> AddTeamAsync(Guid leagueId, Guid teamId)
        {
            var league = await _leagueRepository.GetByIdAsync(leagueId);
            if (league == null) return null;

            var team = await _leagueRepository.GetTeamByIdAsync(teamId);
            if (team == null) return null;

            if (await _leagueRepository.LeagueHasTeamAsync(leagueId, teamId))
                throw new InvalidOperationException("El equipo ya está registrado en esta liga");

            var leagueTeam = new LeagueTeam
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                TeamId = teamId,
                JoinedAt = DateTime.UtcNow
            };

            await _leagueRepository.AddTeamToLeagueAsync(leagueTeam);
            return await GetByIdAsync(leagueId);
        }

        public async Task<LeagueDetailDto?> RemoveTeamAsync(Guid leagueId, Guid teamId)
        {
            var league = await _leagueRepository.GetByIdAsync(leagueId);
            if (league == null) return null;

            var leagueTeams = await _leagueRepository.GetTeamsByLeagueAsync(leagueId);
            if (!leagueTeams.Any(t => t.Id == teamId)) return null;

            var leagueTeam = new LeagueTeam { LeagueId = leagueId, TeamId = teamId };
            await _leagueRepository.RemoveTeamFromLeagueAsync(leagueTeam);

            return await GetByIdAsync(leagueId);
        }

        public async Task<LeagueDetailDto?> UpdateStatusAsync(Guid leagueId, UpdateLeagueStatusDto dto)
        {
            var league = await _leagueRepository.GetByIdAsync(leagueId);
            if (league == null) return null;

            if (!Enum.TryParse<LeagueStatus>(dto.Status, ignoreCase: true, out var newStatus))
                throw new InvalidOperationException("Estado no válido");

            if (newStatus == LeagueStatus.Programada && league.Status != LeagueStatus.Programada)
                throw new InvalidOperationException("No se puede volver a estado Programada");

            if (newStatus == LeagueStatus.Finalizada && league.Status != LeagueStatus.Activa)
                throw new InvalidOperationException("Solo se puede finalizar una liga activa");

            league.Status = newStatus;
            league.UpdatedAt = DateTime.UtcNow;

            await _leagueRepository.UpdateAsync(league);
            return await GetByIdAsync(leagueId);
        }

        public async Task<bool> HasMatchesAsync(Guid leagueId)
        {
            return await _leagueRepository.HasMatchesAsync(leagueId);
        }
    }
}
