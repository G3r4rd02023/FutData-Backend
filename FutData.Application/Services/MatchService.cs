using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Domain.Enums;

namespace FutData.Application.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;

        public MatchService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public async Task<MatchDto?> GetByIdAsync(Guid id)
        {
            var match = await _matchRepository.GetByIdAsync(id);
            return match == null ? null : MapToDto(match);
        }

        public async Task<IEnumerable<MatchDto>> GetAllAsync(MatchFilterDto? filters)
        {
            var matches = await _matchRepository.GetAllAsync(filters);
            return matches.Select(MapToDto);
        }

        public async Task<PaginatedResult<MatchDto>> GetAllPagedAsync(int page, int pageSize, MatchFilterDto? filters = null)
        {
            var (matches, totalCount) = await _matchRepository.GetAllPagedAsync(page, pageSize, filters);
            return new PaginatedResult<MatchDto>
            {
                Items = matches.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MatchDto> CreateAsync(CreateMatchDto dto)
        {
            if (dto.HomeTeamId == dto.AwayTeamId)
                throw new InvalidOperationException("Los equipos local y visitante deben ser distintos");

            if (!await _matchRepository.TeamsAreInLeagueAsync(dto.LeagueId, dto.HomeTeamId, dto.AwayTeamId))
                throw new InvalidOperationException("Ambos equipos deben pertenecer a la liga");

            if (await _matchRepository.ExistsDuplicateAsync(dto.LeagueId, dto.HomeTeamId, dto.AwayTeamId, dto.MatchDate))
                throw new InvalidOperationException("Ya existe un partido entre estos equipos en la misma fecha");

            var match = new Match
            {
                Id = Guid.NewGuid(),
                LeagueId = dto.LeagueId,
                HomeTeamId = dto.HomeTeamId,
                AwayTeamId = dto.AwayTeamId,
                MatchDate = dto.MatchDate,
                Status = MatchStatus.Programado,
                Round = dto.Round,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _matchRepository.AddAsync(match);
            var created = await _matchRepository.GetByIdAsync(match.Id);
            return MapToDto(created!);
        }

        public async Task<MatchDto?> UpdateAsync(Guid id, UpdateMatchDto dto)
        {
            var match = await _matchRepository.GetByIdAsync(id);
            if (match == null) return null;

            if (match.Status == MatchStatus.Finalizado)
                throw new InvalidOperationException("No se puede modificar un partido finalizado");

            if (dto.HomeTeamId == dto.AwayTeamId)
                throw new InvalidOperationException("Los equipos local y visitante deben ser distintos");

            if (!await _matchRepository.TeamsAreInLeagueAsync(dto.LeagueId, dto.HomeTeamId, dto.AwayTeamId))
                throw new InvalidOperationException("Ambos equipos deben pertenecer a la liga");

            if (await _matchRepository.ExistsDuplicateAsync(dto.LeagueId, dto.HomeTeamId, dto.AwayTeamId, dto.MatchDate, id))
                throw new InvalidOperationException("Ya existe un partido entre estos equipos en la misma fecha");

            match.LeagueId = dto.LeagueId;
            match.HomeTeamId = dto.HomeTeamId;
            match.AwayTeamId = dto.AwayTeamId;
            match.MatchDate = dto.MatchDate;
            match.Round = dto.Round;
            match.Notes = dto.Notes;
            match.UpdatedAt = DateTime.UtcNow;

            await _matchRepository.UpdateAsync(match);
            var updated = await _matchRepository.GetByIdAsync(id);
            return updated != null ? MapToDto(updated) : null;
        }

        public async Task<MatchDto?> RegisterResultAsync(Guid id, ResultDto dto)
        {
            var match = await _matchRepository.GetByIdAsync(id);
            if (match == null) return null;

            if (match.Status == MatchStatus.Finalizado)
                throw new InvalidOperationException("El partido ya fue finalizado");

            bool wasFinalized = match.Status == MatchStatus.Finalizado;

            match.HomeGoals = dto.HomeGoals;
            match.AwayGoals = dto.AwayGoals;
            match.Status = MatchStatus.Finalizado;
            match.UpdatedAt = DateTime.UtcNow;

            await _matchRepository.UpdateAsync(match);

            if (!wasFinalized)
            {
                await UpdateStatsForMatch(match);
            }

            var result = await _matchRepository.GetByIdAsync(id);
            return result != null ? MapToDto(result) : null;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var match = await _matchRepository.GetByIdAsync(id);
            if (match == null) return false;

            if (match.Status == MatchStatus.Finalizado)
                throw new InvalidOperationException("No se puede eliminar un partido finalizado");

            if (match.Status == MatchStatus.EnJuego)
                throw new InvalidOperationException("No se puede eliminar un partido en juego");

            await _matchRepository.DeleteAsync(match);
            return true;
        }

        public async Task<MatchDto?> UpdateStatusAsync(Guid id, string status)
        {
            var match = await _matchRepository.GetByIdAsync(id);
            if (match == null) return null;

            if (!Enum.TryParse<MatchStatus>(status, ignoreCase: true, out var newStatus))
                throw new InvalidOperationException("Estado no válido");

            if (match.Status == MatchStatus.Finalizado)
                throw new InvalidOperationException("No se puede cambiar el estado de un partido finalizado");

            match.Status = newStatus;
            match.UpdatedAt = DateTime.UtcNow;

            await _matchRepository.UpdateAsync(match);
            var updated = await _matchRepository.GetByIdAsync(id);
            return updated != null ? MapToDto(updated) : null;
        }

        public async Task<List<TeamLeagueStatsDto>> GetLeagueStatsAsync(Guid leagueId)
        {
            var stats = new List<TeamLeagueStatsDto>();
            var matches = await _matchRepository.GetAllAsync(new MatchFilterDto { LeagueId = leagueId });

            var teamIds = matches
                .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
                .Distinct()
                .ToList();

            foreach (var teamId in teamIds)
            {
                var teamMatches = matches.Where(m =>
                    (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                    m.Status == MatchStatus.Finalizado &&
                    m.HomeGoals.HasValue && m.AwayGoals.HasValue);

                int played = teamMatches.Count();
                int won = 0, drawn = 0, lost = 0, goalsFor = 0, goalsAgainst = 0;

                foreach (var m in teamMatches)
                {
                    bool isHome = m.HomeTeamId == teamId;
                    int myGoals = isHome ? m.HomeGoals!.Value : m.AwayGoals!.Value;
                    int otherGoals = isHome ? m.AwayGoals!.Value : m.HomeGoals!.Value;

                    goalsFor += myGoals;
                    goalsAgainst += otherGoals;

                    if (myGoals > otherGoals) won++;
                    else if (myGoals == otherGoals) drawn++;
                    else lost++;
                }

                var firstMatch = teamMatches.FirstOrDefault();
                string teamName = firstMatch != null
                    ? (firstMatch.HomeTeamId == teamId ? firstMatch.HomeTeam.Name : firstMatch.AwayTeam.Name)
                    : string.Empty;
                string? teamLogo = firstMatch != null
                    ? (firstMatch.HomeTeamId == teamId ? firstMatch.HomeTeam.LogoUrl : firstMatch.AwayTeam.LogoUrl)
                    : null;
                string leagueName = firstMatch?.League.Name ?? string.Empty;

                stats.Add(new TeamLeagueStatsDto
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    TeamName = teamName,
                    TeamLogo = teamLogo,
                    LeagueId = leagueId,
                    LeagueName = leagueName,
                    Played = played,
                    Won = won,
                    Drawn = drawn,
                    Lost = lost,
                    GoalsFor = goalsFor,
                    GoalsAgainst = goalsAgainst,
                    Points = (won * 3) + drawn
                });
            }

            return stats.OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .ToList();
        }

        private async Task UpdateStatsForMatch(Match match)
        {
            var homeStats = await _matchRepository.GetStatsAsync(match.HomeTeamId, match.LeagueId)
                ?? new TeamLeagueStats
                {
                    Id = Guid.NewGuid(),
                    TeamId = match.HomeTeamId,
                    LeagueId = match.LeagueId
                };

            var awayStats = await _matchRepository.GetStatsAsync(match.AwayTeamId, match.LeagueId)
                ?? new TeamLeagueStats
                {
                    Id = Guid.NewGuid(),
                    TeamId = match.AwayTeamId,
                    LeagueId = match.LeagueId
                };

            bool isNewHome = homeStats.Played == 0 && homeStats.Won == 0 && homeStats.Drawn == 0 && homeStats.Lost == 0;
            bool isNewAway = awayStats.Played == 0 && awayStats.Won == 0 && awayStats.Drawn == 0 && awayStats.Lost == 0;

            homeStats.Played++;
            awayStats.Played++;
            homeStats.GoalsFor += match.HomeGoals!.Value;
            homeStats.GoalsAgainst += match.AwayGoals!.Value;
            awayStats.GoalsFor += match.AwayGoals!.Value;
            awayStats.GoalsAgainst += match.HomeGoals!.Value;

            if (match.HomeGoals > match.AwayGoals)
            {
                homeStats.Won++;
                homeStats.Points += 3;
                awayStats.Lost++;
            }
            else if (match.HomeGoals == match.AwayGoals)
            {
                homeStats.Drawn++;
                homeStats.Points++;
                awayStats.Drawn++;
                awayStats.Points++;
            }
            else
            {
                awayStats.Won++;
                awayStats.Points += 3;
                homeStats.Lost++;
            }

            if (isNewHome)
                await _matchRepository.AddStatsAsync(homeStats);
            else
                await _matchRepository.UpdateStatsAsync(homeStats);

            if (isNewAway)
                await _matchRepository.AddStatsAsync(awayStats);
            else
                await _matchRepository.UpdateStatsAsync(awayStats);
        }

        private static MatchDto MapToDto(Match match)
        {
            return new MatchDto
            {
                Id = match.Id,
                LeagueId = match.LeagueId,
                LeagueName = match.League.Name,
                HomeTeamId = match.HomeTeamId,
                HomeTeamName = match.HomeTeam.Name,
                HomeTeamLogo = match.HomeTeam.LogoUrl,
                AwayTeamId = match.AwayTeamId,
                AwayTeamName = match.AwayTeam.Name,
                AwayTeamLogo = match.AwayTeam.LogoUrl,
                HomeGoals = match.HomeGoals,
                AwayGoals = match.AwayGoals,
                MatchDate = match.MatchDate,
                Status = match.Status,
                Round = match.Round,
                Notes = match.Notes,
                CreatedAt = match.CreatedAt
            };
        }
    }
}
