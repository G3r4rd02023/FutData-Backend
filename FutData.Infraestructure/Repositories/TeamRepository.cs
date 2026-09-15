using FutData.Application.Interfaces;
using FutData.Domain.Entities;
using FutData.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FutData.Infraestructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly FutDataDbContext _context;

        public TeamRepository(FutDataDbContext context)
        {
            _context = context;
        }

        public async Task<Team?> GetByIdAsync(Guid id)
        {
            return await _context.Teams.FindAsync(id);
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _context.Teams
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Team>> SearchAsync(string? searchTerm)
        {
            var query = _context.Teams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(term) ||
                    t.City.ToLower().Contains(term));
            }

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task AddAsync(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            return await _context.Teams.AnyAsync(t =>
                t.Name.ToLower() == name.ToLower() &&
                (!excludeId.HasValue || t.Id != excludeId.Value));
        }

        public async Task<int> GetMatchCountAsync(Guid teamId)
        {
            return await _context.Matches.CountAsync(m =>
                m.HomeTeamId == teamId || m.AwayTeamId == teamId);
        }
    }
}
