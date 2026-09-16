using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FutData.Infraestructure.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly FutDataDbContext _context;

        public MatchRepository(FutDataDbContext context)
        {
            _context = context;
        }

        public async Task<Match?> GetByIdAsync(Guid id)
        {
            return await _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.League)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Match>> GetAllAsync(MatchFilterDto? filters)
        {
            var query = _context.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Include(m => m.League)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.LeagueId.HasValue)
                    query = query.Where(m => m.LeagueId == filters.LeagueId.Value);

                if (filters.TeamId.HasValue)
                    query = query.Where(m => m.HomeTeamId == filters.TeamId.Value || m.AwayTeamId == filters.TeamId.Value);

                if (filters.DateFrom.HasValue)
                    query = query.Where(m => m.MatchDate >= filters.DateFrom.Value);

                if (filters.DateTo.HasValue)
                    query = query.Where(m => m.MatchDate <= filters.DateTo.Value);

                if (filters.Status.HasValue)
                    query = query.Where(m => m.Status == filters.Status.Value);

                if (filters.Round.HasValue)
                    query = query.Where(m => m.Round == filters.Round.Value);
            }

            return await query.OrderByDescending(m => m.MatchDate).ToListAsync();
        }

        public async Task<bool> ExistsDuplicateAsync(Guid leagueId, Guid homeTeamId, Guid awayTeamId, DateTime matchDate, Guid? excludeId = null)
        {
            return await _context.Matches
                .AnyAsync(m => m.LeagueId == leagueId
                    && m.HomeTeamId == homeTeamId
                    && m.AwayTeamId == awayTeamId
                    && m.MatchDate.Date == matchDate.Date
                    && (!excludeId.HasValue || m.Id != excludeId.Value));
        }

        public async Task<bool> TeamsAreInLeagueAsync(Guid leagueId, Guid homeTeamId, Guid awayTeamId)
        {
            var homeInLeague = await _context.LeagueTeams
                .AnyAsync(lt => lt.LeagueId == leagueId && lt.TeamId == homeTeamId);
            var awayInLeague = await _context.LeagueTeams
                .AnyAsync(lt => lt.LeagueId == leagueId && lt.TeamId == awayTeamId);
            return homeInLeague && awayInLeague;
        }

        public async Task<TeamLeagueStats?> GetStatsAsync(Guid teamId, Guid leagueId)
        {
            return await _context.TeamLeagueStats
                .FirstOrDefaultAsync(s => s.TeamId == teamId && s.LeagueId == leagueId);
        }

        public async Task AddAsync(Match match)
        {
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Match match)
        {
            _context.Matches.Update(match);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Match match)
        {
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
        }

        public async Task AddStatsAsync(TeamLeagueStats stats)
        {
            await _context.TeamLeagueStats.AddAsync(stats);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatsAsync(TeamLeagueStats stats)
        {
            _context.TeamLeagueStats.Update(stats);
            await _context.SaveChangesAsync();
        }
    }
}
