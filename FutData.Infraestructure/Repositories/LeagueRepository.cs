using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FutData.Infraestructure.Repositories
{
    public class LeagueRepository : ILeagueRepository
    {
        private readonly FutDataDbContext _context;

        public LeagueRepository(FutDataDbContext context)
        {
            _context = context;
        }

        public async Task<League?> GetByIdAsync(Guid id)
        {
            return await _context.Leagues.FindAsync(id);
        }

        public async Task<League?> GetByNameAsync(string name)
        {
            return await _context.Leagues.FirstOrDefaultAsync(l => l.Name == name);
        }

        public async Task<IEnumerable<League>> GetAllAsync()
        {
            return await _context.Leagues.ToListAsync();
        }

        public async Task<IEnumerable<League>> SearchAsync(string searchTerm)
        {
            return await _context.Leagues
                .Where(l => l.Name.Contains(searchTerm) ||
                           (l.Description != null && l.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<(IEnumerable<League> Items, int TotalCount)> GetAllPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            var query = _context.Leagues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l =>
                    l.Name.Contains(searchTerm) ||
                    (l.Description != null && l.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(l => l.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            return await _context.Leagues
                .AnyAsync(l => l.Name == name && (!excludeId.HasValue || l.Id != excludeId.Value));
        }

        public async Task<int> GetTeamCountAsync(Guid leagueId)
        {
            return await _context.LeagueTeams.CountAsync(lt => lt.LeagueId == leagueId);
        }

        public async Task<List<Team>> GetTeamsByLeagueAsync(Guid leagueId)
        {
            return await _context.LeagueTeams
                .Where(lt => lt.LeagueId == leagueId)
                .Select(lt => lt.Team)
                .ToListAsync();
        }

        public async Task<bool> LeagueHasTeamAsync(Guid leagueId, Guid teamId)
        {
            return await _context.LeagueTeams
                .AnyAsync(lt => lt.LeagueId == leagueId && lt.TeamId == teamId);
        }

        public async Task<Team?> GetTeamByIdAsync(Guid teamId)
        {
            return await _context.Teams.FindAsync(teamId);
        }

        public async Task AddTeamToLeagueAsync(LeagueTeam leagueTeam)
        {
            await _context.LeagueTeams.AddAsync(leagueTeam);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTeamFromLeagueAsync(LeagueTeam leagueTeam)
        {
            _context.LeagueTeams.Remove(leagueTeam);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasMatchesAsync(Guid leagueId)
        {
            return await _context.Matches.AnyAsync(m => m.LeagueId == leagueId);
        }

        public async Task AddAsync(League league)
        {
            await _context.Leagues.AddAsync(league);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(League league)
        {
            _context.Leagues.Update(league);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(League league)
        {
            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();
        }
    }
}
