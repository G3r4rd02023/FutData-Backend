using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Domain.Enums;

namespace FutData.Application.Services
{
    public class RankingService : IRankingService
    {
        private readonly IMatchRepository _matchRepository;

        public RankingService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public async Task<List<StandingDto>> GetLeagueStandingsAsync(Guid leagueId)
        {
            var matches = await _matchRepository.GetAllAsync(new MatchFilterDto { LeagueId = leagueId });
            var finalizedMatches = matches
                .Where(m => m.Status == MatchStatus.Finalizado && m.HomeGoals.HasValue && m.AwayGoals.HasValue)
                .OrderBy(m => m.MatchDate)
                .ToList();

            var teamIds = finalizedMatches
                .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
                .Distinct()
                .ToList();

            var standings = new List<StandingDto>();

            foreach (var teamId in teamIds)
            {
                var teamMatches = finalizedMatches
                    .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                    .ToList();

                int played = teamMatches.Count;
                int won = 0, drawn = 0, lost = 0, goalsFor = 0, goalsAgainst = 0;
                var lastResults = new List<char>();

                foreach (var m in teamMatches)
                {
                    bool isHome = m.HomeTeamId == teamId;
                    int myGoals = isHome ? m.HomeGoals!.Value : m.AwayGoals!.Value;
                    int otherGoals = isHome ? m.AwayGoals!.Value : m.HomeGoals!.Value;

                    goalsFor += myGoals;
                    goalsAgainst += otherGoals;

                    if (myGoals > otherGoals)
                    {
                        won++;
                        lastResults.Add('W');
                    }
                    else if (myGoals == otherGoals)
                    {
                        drawn++;
                        lastResults.Add('D');
                    }
                    else
                    {
                        lost++;
                        lastResults.Add('L');
                    }
                }

                string form = new string(lastResults.TakeLast(5).ToArray());

                string teamName = string.Empty;
                string? teamLogo = null;

                var firstMatch = teamMatches.FirstOrDefault();
                if (firstMatch != null)
                {
                    teamName = firstMatch.HomeTeamId == teamId
                        ? firstMatch.HomeTeam.Name
                        : firstMatch.AwayTeam.Name;
                    teamLogo = firstMatch.HomeTeamId == teamId
                        ? firstMatch.HomeTeam.LogoUrl
                        : firstMatch.AwayTeam.LogoUrl;
                }

                standings.Add(new StandingDto
                {
                    TeamId = teamId,
                    TeamName = teamName,
                    TeamLogo = teamLogo,
                    Played = played,
                    Won = won,
                    Drawn = drawn,
                    Lost = lost,
                    GoalsFor = goalsFor,
                    GoalsAgainst = goalsAgainst,
                    Points = (won * 3) + drawn,
                    Form = form,
                    PositionChange = PositionChange.Igual
                });
            }

            standings = standings
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .ThenBy(s => s.GoalsAgainst)
                .ToList();

            for (int i = 0; i < standings.Count; i++)
            {
                standings[i].Position = i + 1;
            }

            return standings;
        }

        public async Task<List<GeneralRankingDto>> GetGeneralRankingAsync()
        {
            var allMatches = await _matchRepository.GetAllAsync(null);
            var finalizedMatches = allMatches
                .Where(m => m.Status == MatchStatus.Finalizado && m.HomeGoals.HasValue && m.AwayGoals.HasValue)
                .ToList();

            var teamData = new Dictionary<Guid, TeamGeneralData>();

            foreach (var match in finalizedMatches)
            {
                ProcessTeam(match, match.HomeTeamId, match.HomeTeam, match.AwayTeam,
                    match.HomeGoals!.Value, match.AwayGoals!.Value, teamData);
                ProcessTeam(match, match.AwayTeamId, match.AwayTeam, match.HomeTeam,
                    match.AwayGoals!.Value, match.HomeGoals!.Value, teamData);
            }

            var rankings = teamData.Values
                .Select(d => new GeneralRankingDto
                {
                    TeamId = d.TeamId,
                    TeamName = d.TeamName,
                    TeamLogo = d.TeamLogo,
                    TotalPoints = d.TotalPoints,
                    TotalMatches = d.TotalMatches,
                    TotalWon = d.TotalWon,
                    WinRate = d.TotalMatches > 0
                        ? Math.Round((decimal)d.TotalWon / d.TotalMatches * 100, 1)
                        : 0,
                    AvgGoals = d.TotalMatches > 0
                        ? Math.Round((decimal)d.TotalGoalsFor / d.TotalMatches, 2)
                        : 0,
                    LeaguesCount = d.LeagueIds.Count
                })
                .OrderByDescending(r => r.TotalPoints)
                .ThenByDescending(r => r.WinRate)
                .ThenByDescending(r => r.AvgGoals)
                .ToList();

            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
            }

            return rankings;
        }

        public async Task<List<GeneralRankingDto>> GetTopTeamsAsync(int count = 10)
        {
            var allRankings = await GetGeneralRankingAsync();
            return allRankings.Take(count).ToList();
        }

        public async Task<TeamStatsDto?> GetTeamStatsAsync(Guid teamId)
        {
            var allMatches = await _matchRepository.GetAllAsync(null);
            var teamMatches = allMatches
                .Where(m => (m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                    && m.Status == MatchStatus.Finalizado
                    && m.HomeGoals.HasValue && m.AwayGoals.HasValue)
                .OrderBy(m => m.MatchDate)
                .ToList();

            if (!teamMatches.Any()) return null;

            var firstMatch = teamMatches.First();
            string teamName = firstMatch.HomeTeamId == teamId
                ? firstMatch.HomeTeam.Name
                : firstMatch.AwayTeam.Name;
            string? teamLogo = firstMatch.HomeTeamId == teamId
                ? firstMatch.HomeTeam.LogoUrl
                : firstMatch.AwayTeam.LogoUrl;

            int totalPlayed = teamMatches.Count;
            int totalWon = 0, totalDrawn = 0, totalLost = 0;
            int totalGoalsFor = 0, totalGoalsAgainst = 0;

            var leagueParticipations = new Dictionary<Guid, LeagueParticipationDto>();

            foreach (var match in teamMatches)
            {
                bool isHome = match.HomeTeamId == teamId;
                int myGoals = isHome ? match.HomeGoals!.Value : match.AwayGoals!.Value;
                int otherGoals = isHome ? match.AwayGoals!.Value : match.HomeGoals!.Value;

                totalGoalsFor += myGoals;
                totalGoalsAgainst += otherGoals;

                char result;
                if (myGoals > otherGoals) { totalWon++; result = 'W'; }
                else if (myGoals == otherGoals) { totalDrawn++; result = 'D'; }
                else { totalLost++; result = 'L'; }

                if (!leagueParticipations.ContainsKey(match.LeagueId))
                {
                    leagueParticipations[match.LeagueId] = new LeagueParticipationDto
                    {
                        LeagueId = match.LeagueId,
                        LeagueName = match.League.Name
                    };
                }

                var lp = leagueParticipations[match.LeagueId];
                lp.Played++;
                lp.GoalsFor += myGoals;
                lp.GoalsAgainst += otherGoals;

                if (result == 'W') lp.Won++;
                else if (result == 'D') lp.Drawn++;
                else lp.Lost++;

                lp.Points = (lp.Won * 3) + lp.Drawn;
            }

            foreach (var lp in leagueParticipations.Values)
            {
                var leagueMatches = teamMatches
                    .Where(m => m.LeagueId == lp.LeagueId)
                    .OrderBy(m => m.MatchDate)
                    .ToList();

                var lastResults = new List<char>();
                foreach (var m in leagueMatches)
                {
                    bool isHome = m.HomeTeamId == teamId;
                    int myGoals = isHome ? m.HomeGoals!.Value : m.AwayGoals!.Value;
                    int otherGoals = isHome ? m.AwayGoals!.Value : m.HomeGoals!.Value;

                    if (myGoals > otherGoals) lastResults.Add('W');
                    else if (myGoals == otherGoals) lastResults.Add('D');
                    else lastResults.Add('L');
                }

                lp.Form = new string(lastResults.TakeLast(5).ToArray());
            }

            return new TeamStatsDto
            {
                TeamId = teamId,
                TeamName = teamName,
                TeamLogo = teamLogo,
                TotalMatches = totalPlayed,
                TotalWon = totalWon,
                TotalDrawn = totalDrawn,
                TotalLost = totalLost,
                TotalGoalsFor = totalGoalsFor,
                TotalGoalsAgainst = totalGoalsAgainst,
                TotalPoints = (totalWon * 3) + totalDrawn,
                WinRate = totalPlayed > 0 ? Math.Round((decimal)totalWon / totalPlayed * 100, 1) : 0,
                AvgGoalsFor = totalPlayed > 0 ? Math.Round((decimal)totalGoalsFor / totalPlayed, 2) : 0,
                AvgGoalsAgainst = totalPlayed > 0 ? Math.Round((decimal)totalGoalsAgainst / totalPlayed, 2) : 0,
                Leagues = leagueParticipations.Values.OrderByDescending(l => l.Points).ToList()
            };
        }

        private static void ProcessTeam(
            Match match, Guid teamId, Domain.Entities.Team team, Domain.Entities.Team opponent,
            int myGoals, int otherGoals, Dictionary<Guid, TeamGeneralData> data)
        {
            if (!data.ContainsKey(teamId))
            {
                data[teamId] = new TeamGeneralData
                {
                    TeamId = teamId,
                    TeamName = team.Name,
                    TeamLogo = team.LogoUrl
                };
            }

            var d = data[teamId];
            d.TotalMatches++;

            if (myGoals > otherGoals)
            {
                d.TotalWon++;
                d.TotalPoints += 3;
            }
            else if (myGoals == otherGoals)
            {
                d.TotalPoints += 1;
            }

            d.TotalGoalsFor += myGoals;
            d.TotalGoalsAgainst += otherGoals;

            if (!d.LeagueIds.Contains(match.LeagueId))
                d.LeagueIds.Add(match.LeagueId);
        }

        private class TeamGeneralData
        {
            public Guid TeamId { get; set; }
            public string TeamName { get; set; } = string.Empty;
            public string? TeamLogo { get; set; }
            public int TotalMatches { get; set; }
            public int TotalWon { get; set; }
            public int TotalPoints { get; set; }
            public int TotalGoalsFor { get; set; }
            public int TotalGoalsAgainst { get; set; }
            public List<Guid> LeagueIds { get; set; } = new();
        }
    }
}
