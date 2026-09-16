using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Domain.Enums;

namespace FutData.Application.Services
{
    public class SimulationService : ISimulationService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly IMatchService _matchService;
        private static readonly Random _random = new();

        public SimulationService(
            IMatchRepository matchRepository,
            ILeagueRepository leagueRepository,
            IMatchService matchService)
        {
            _matchRepository = matchRepository;
            _leagueRepository = leagueRepository;
            _matchService = matchService;
        }

        public async Task<SimulationResult> SimulateAsync(SimulationRequest request)
        {
            ValidateProbabilities(request.HomeWinProbability, request.DrawProbability, request.AwayWinProbability);

            if (request.HomeTeamId == request.AwayTeamId)
                throw new InvalidOperationException("Los equipos deben ser distintos");

            var league = await _leagueRepository.GetByIdAsync(request.LeagueId);
            if (league == null)
                throw new InvalidOperationException("Liga no encontrada");

            if (!await _leagueRepository.LeagueHasTeamAsync(request.LeagueId, request.HomeTeamId))
                throw new InvalidOperationException("El equipo local no pertenece a la liga");

            if (!await _leagueRepository.LeagueHasTeamAsync(request.LeagueId, request.AwayTeamId))
                throw new InvalidOperationException("El equipo visitante no pertenece a la liga");

            var homeTeam = await _leagueRepository.GetTeamByIdAsync(request.HomeTeamId);
            var awayTeam = await _leagueRepository.GetTeamByIdAsync(request.AwayTeamId);

            var (homeGoals, awayGoals, result) = SimulateGoals(
                request.HomeWinProbability,
                request.DrawProbability,
                request.AwayWinProbability);

            var match = new Match
            {
                Id = Guid.NewGuid(),
                LeagueId = request.LeagueId,
                HomeTeamId = request.HomeTeamId,
                AwayTeamId = request.AwayTeamId,
                HomeGoals = homeGoals,
                AwayGoals = awayGoals,
                MatchDate = request.MatchDate,
                Status = MatchStatus.Finalizado,
                Round = request.Round,
                Notes = "Simulado por el sistema",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _matchRepository.AddAsync(match);

            string resultMessage = result switch
            {
                SimulatedResult.Local => $"Victoria de {homeTeam?.Name}",
                SimulatedResult.Empate => "Empate",
                SimulatedResult.Visitante => $"Victoria de {awayTeam?.Name}",
                _ => "Resultado desconocido"
            };

            return new SimulationResult
            {
                MatchId = match.Id,
                HomeTeamName = homeTeam?.Name ?? string.Empty,
                HomeTeamLogo = homeTeam?.LogoUrl,
                AwayTeamName = awayTeam?.Name ?? string.Empty,
                AwayTeamLogo = awayTeam?.LogoUrl,
                SimulatedHomeGoals = homeGoals,
                SimulatedAwayGoals = awayGoals,
                Result = result,
                Probabilities = new SimulationProbabilities
                {
                    HomeWin = request.HomeWinProbability,
                    Draw = request.DrawProbability,
                    AwayWin = request.AwayWinProbability
                },
                Message = resultMessage,
                SimulatedAt = DateTime.UtcNow
            };
        }

        public async Task<BatchSimulationResult> SimulateBatchAsync(BatchSimulationRequest request)
        {
            var results = new List<SimulationResult>();

            foreach (var item in request.Matches)
            {
                var simRequest = new SimulationRequest
                {
                    LeagueId = request.LeagueId,
                    HomeTeamId = item.HomeTeamId,
                    AwayTeamId = item.AwayTeamId,
                    HomeWinProbability = item.HomeWinProbability,
                    DrawProbability = item.DrawProbability,
                    AwayWinProbability = item.AwayWinProbability,
                    MatchDate = DateTime.UtcNow.AddDays(1),
                    Round = request.Round
                };

                var result = await SimulateAsync(simRequest);
                results.Add(result);
            }

            return new BatchSimulationResult
            {
                Results = results,
                TotalSimulated = results.Count,
                Message = $"Se simularon {results.Count} partidos de la jornada {request.Round}"
            };
        }

        public async Task<List<SimulationHistoryDto>> GetHistoryAsync(Guid? leagueId = null)
        {
            var filters = new MatchFilterDto { LeagueId = leagueId };
            var matches = await _matchRepository.GetAllAsync(filters);

            return matches
                .Where(m => m.Status == MatchStatus.Finalizado && m.Notes == "Simulado por el sistema")
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new SimulationHistoryDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    AwayTeamName = m.AwayTeam.Name,
                    HomeGoals = m.HomeGoals ?? 0,
                    AwayGoals = m.AwayGoals ?? 0,
                    Result = (m.HomeGoals ?? 0) > (m.AwayGoals ?? 0) ? "Local" :
                             (m.HomeGoals ?? 0) == (m.AwayGoals ?? 0) ? "Empate" : "Visitante",
                    LeagueName = m.League.Name,
                    Round = m.Round,
                    SimulatedAt = m.CreatedAt
                })
                .ToList();
        }

        public async Task<SimulationStatsDto> GetStatsAsync(Guid? leagueId = null)
        {
            var history = await GetHistoryAsync(leagueId);
            var total = history.Count;

            if (total == 0)
            {
                return new SimulationStatsDto();
            }

            int homeWins = history.Count(h => h.Result == "Local");
            int draws = history.Count(h => h.Result == "Empate");
            int awayWins = history.Count(h => h.Result == "Visitante");

            int totalHomeGoals = history.Sum(h => h.HomeGoals);
            int totalAwayGoals = history.Sum(h => h.AwayGoals);

            return new SimulationStatsDto
            {
                TotalSimulations = total,
                HomeWins = homeWins,
                Draws = draws,
                AwayWins = awayWins,
                HomeWinRate = Math.Round((decimal)homeWins / total * 100, 1),
                DrawRate = Math.Round((decimal)draws / total * 100, 1),
                AwayWinRate = Math.Round((decimal)awayWins / total * 100, 1),
                AvgHomeGoals = Math.Round((decimal)totalHomeGoals / total, 2),
                AvgAwayGoals = Math.Round((decimal)totalAwayGoals / total, 2)
            };
        }

        private static void ValidateProbabilities(decimal home, decimal draw, decimal away)
        {
            if (home < 0 || draw < 0 || away < 0)
                throw new InvalidOperationException("Las probabilidades no pueden ser negativas");

            var sum = home + draw + away;
            if (Math.Abs(sum - 100) > 0.01m)
                throw new InvalidOperationException($"Las probabilidades deben sumar 100 (actual: {sum})");
        }

        private static (int homeGoals, int awayGoals, SimulatedResult result) SimulateGoals(
            decimal homeWinProb, decimal drawProb, decimal awayWinProb)
        {
            var randomValue = (decimal)(_random.NextDouble() * 100);

            SimulatedResult result;
            if (randomValue <= homeWinProb)
                result = SimulatedResult.Local;
            else if (randomValue <= homeWinProb + drawProb)
                result = SimulatedResult.Empate;
            else
                result = SimulatedResult.Visitante;

            int homeGoals, awayGoals;

            switch (result)
            {
                case SimulatedResult.Local:
                    homeGoals = _random.Next(1, 5);
                    awayGoals = _random.Next(0, 4);
                    if (awayGoals >= homeGoals) awayGoals = homeGoals - 1;
                    break;

                case SimulatedResult.Empate:
                    homeGoals = _random.Next(0, 5);
                    awayGoals = homeGoals;
                    break;

                case SimulatedResult.Visitante:
                    homeGoals = _random.Next(0, 4);
                    awayGoals = _random.Next(1, 5);
                    if (homeGoals >= awayGoals) homeGoals = awayGoals - 1;
                    break;

                default:
                    homeGoals = 0;
                    awayGoals = 0;
                    break;
            }

            return (homeGoals, awayGoals, result);
        }
    }
}
