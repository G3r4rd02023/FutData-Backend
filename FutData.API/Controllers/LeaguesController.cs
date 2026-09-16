using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutData.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaguesController : ControllerBase
    {
        private readonly ILeagueService _leagueService;
        private readonly IRankingService _rankingService;

        public LeaguesController(ILeagueService leagueService, IRankingService rankingService)
        {
            _leagueService = leagueService;
            _rankingService = rankingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leagues = await _leagueService.GetAllAsync();
            return Ok(leagues);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var league = await _leagueService.GetByIdAsync(id);
            if (league == null)
                return NotFound(new { message = "Liga no encontrada" });

            return Ok(league);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            var leagues = await _leagueService.SearchAsync(searchTerm);
            return Ok(leagues);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeagueDto dto)
        {
            try
            {
                var league = await _leagueService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = league.Id }, league);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeagueDto dto)
        {
            try
            {
                var league = await _leagueService.UpdateAsync(id, dto);
                if (league == null)
                    return NotFound(new { message = "Liga no encontrada" });

                return Ok(league);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _leagueService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Liga no encontrada" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{leagueId:guid}/teams/{teamId:guid}")]
        public async Task<IActionResult> AddTeam(Guid leagueId, Guid teamId)
        {
            try
            {
                var league = await _leagueService.AddTeamAsync(leagueId, teamId);
                if (league == null)
                    return NotFound(new { message = "Liga o equipo no encontrado" });

                return Ok(league);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{leagueId:guid}/teams/{teamId:guid}")]
        public async Task<IActionResult> RemoveTeam(Guid leagueId, Guid teamId)
        {
            try
            {
                var league = await _leagueService.RemoveTeamAsync(leagueId, teamId);
                if (league == null)
                    return NotFound(new { message = "Liga o equipo no encontrado" });

                return Ok(league);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{leagueId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid leagueId, [FromBody] UpdateLeagueStatusDto dto)
        {
            try
            {
                var league = await _leagueService.UpdateStatusAsync(leagueId, dto);
                if (league == null)
                    return NotFound(new { message = "Liga no encontrada" });

                return Ok(league);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id:guid}/standings")]
        public async Task<IActionResult> GetStandings(Guid id)
        {
            var standings = await _rankingService.GetLeagueStandingsAsync(id);
            return Ok(standings);
        }
    }
}
