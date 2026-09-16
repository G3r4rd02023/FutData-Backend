using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Enums;

namespace FutData.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ILeagueRepository _leagueRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IRankingService _rankingService;

        public DashboardService(
            ITeamRepository teamRepository,
            ILeagueRepository leagueRepository,
            IMatchRepository matchRepository,
            IRankingService rankingService)
        {
            _teamRepository = teamRepository;
            _leagueRepository = leagueRepository;
            _matchRepository = matchRepository;
            _rankingService = rankingService;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var teams = await _teamRepository.GetAllAsync();
            var leagues = (await _leagueRepository.GetAllAsync()).ToList();
            var allMatches = (await _matchRepository.GetAllAsync(null)).ToList();

            var finalizedMatches = allMatches
                .Where(m => m.Status == MatchStatus.Finalizado)
                .ToList();

            var activeLeagues = new List<ActiveLeagueDto>();
            foreach (var league in leagues.Where(l => l.Status == LeagueStatus.Activa))
            {
                var teamCount = await _leagueRepository.GetTeamCountAsync(league.Id);
                var matchCount = allMatches.Count(m => m.LeagueId == league.Id);
                activeLeagues.Add(new ActiveLeagueDto
                {
                    Id = league.Id,
                    Name = league.Name,
                    TeamsCount = teamCount,
                    MatchesCount = matchCount
                });
            }

            var recentMatches = finalizedMatches
                .OrderByDescending(m => m.MatchDate)
                .Take(5)
                .Select(m => new RecentMatchDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    HomeTeamLogo = m.HomeTeam.LogoUrl,
                    AwayTeamName = m.AwayTeam.Name,
                    AwayTeamLogo = m.AwayTeam.LogoUrl,
                    HomeGoals = m.HomeGoals ?? 0,
                    AwayGoals = m.AwayGoals ?? 0,
                    MatchDate = m.MatchDate,
                    LeagueName = m.League.Name
                })
                .ToList();

            var upcomingMatches = allMatches
                .Where(m => m.Status == MatchStatus.Programado && m.MatchDate >= DateTime.UtcNow)
                .OrderBy(m => m.MatchDate)
                .Take(5)
                .Select(m => new UpcomingMatchDto
                {
                    Id = m.Id,
                    HomeTeamName = m.HomeTeam.Name,
                    HomeTeamLogo = m.HomeTeam.LogoUrl,
                    AwayTeamName = m.AwayTeam.Name,
                    AwayTeamLogo = m.AwayTeam.LogoUrl,
                    MatchDate = m.MatchDate,
                    LeagueName = m.League.Name,
                    Round = m.Round
                })
                .ToList();

            var topTeamsRanking = await _rankingService.GetTopTeamsAsync(5);
            var topTeams = topTeamsRanking.Select((t, i) => new TopTeamDto
            {
                Rank = i + 1,
                TeamId = t.TeamId,
                TeamName = t.TeamName,
                TeamLogo = t.TeamLogo,
                Points = t.TotalPoints,
                WinRate = t.WinRate
            }).ToList();

            return new DashboardDto
            {
                Summary = new DashboardSummaryDto
                {
                    TotalTeams = teams.Count,
                    TotalLeagues = leagues.Count,
                    TotalMatches = allMatches.Count,
                    MatchesPlayed = finalizedMatches.Count,
                    MatchesPending = allMatches.Count - finalizedMatches.Count
                },
                ActiveLeagues = activeLeagues,
                RecentMatches = recentMatches,
                UpcomingMatches = upcomingMatches,
                TopTeams = topTeams,
                Charts = await GetChartsInternalAsync(allMatches)
            };
        }

        public async Task<DashboardSummaryOnlyDto> GetSummaryAsync()
        {
            var teams = await _teamRepository.GetAllAsync();
            var leagues = (await _leagueRepository.GetAllAsync()).ToList();
            var allMatches = (await _matchRepository.GetAllAsync(null)).ToList();
            var finalizedMatches = allMatches.Count(m => m.Status == MatchStatus.Finalizado);

            return new DashboardSummaryOnlyDto
            {
                TotalTeams = teams.Count,
                TotalLeagues = leagues.Count,
                TotalMatches = allMatches.Count,
                MatchesPlayed = finalizedMatches,
                MatchesPending = allMatches.Count - finalizedMatches
            };
        }

        public async Task<DashboardChartsDto> GetChartsAsync()
        {
            var allMatches = (await _matchRepository.GetAllAsync(null)).ToList();
            return await GetChartsInternalAsync(allMatches);
        }

        private static async Task<DashboardChartsDto> GetChartsInternalAsync(List<Domain.Entities.Match> allMatches)
        {
            var finalizedMatches = allMatches
                .Where(m => m.Status == MatchStatus.Finalizado && m.HomeGoals.HasValue && m.AwayGoals.HasValue)
                .ToList();

            var now = DateTime.UtcNow;
            var monthlyData = new List<MonthlyDataDto>();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var count = finalizedMatches.Count(m =>
                    m.MatchDate.Month == month.Month && m.MatchDate.Year == month.Year);
                monthlyData.Add(new MonthlyDataDto
                {
                    Month = month.ToString("MMM yyyy"),
                    Count = count
                });
            }

            int wins = 0, draws = 0, losses = 0;
            var processedMatches = new HashSet<Guid>();
            foreach (var m in finalizedMatches)
            {
                if (processedMatches.Contains(m.Id)) continue;
                processedMatches.Add(m.Id);

                if (m.HomeGoals > m.AwayGoals) wins++;
                else if (m.HomeGoals == m.AwayGoals) draws++;
                else losses++;
            }

            return new DashboardChartsDto
            {
                MatchesByMonth = monthlyData,
                ResultDistribution = new ResultDistributionDto
                {
                    Wins = wins,
                    Draws = draws,
                    Losses = losses
                }
            };
        }
    }
}
