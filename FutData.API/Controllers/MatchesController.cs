using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using FutData.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutData.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchesController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? leagueId,
            [FromQuery] Guid? teamId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] MatchStatus? status,
            [FromQuery] int? round)
        {
            var filters = new MatchFilterDto
            {
                LeagueId = leagueId,
                TeamId = teamId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
                Round = round
            };

            var matches = await _matchService.GetAllAsync(filters);
            return Ok(matches);
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var match = await _matchService.GetByIdAsync(id);
            if (match == null)
                return NotFound(new { message = "Partido no encontrado" });

            return Ok(match);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMatchDto dto)
        {
            try
            {
                var match = await _matchService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = match.Id }, match);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMatchDto dto)
        {
            try
            {
                var match = await _matchService.UpdateAsync(id, dto);
                if (match == null)
                    return NotFound(new { message = "Partido no encontrado" });

                return Ok(match);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/result")]
        public async Task<IActionResult> RegisterResult(Guid id, [FromBody] ResultDto dto)
        {
            try
            {
                var match = await _matchService.RegisterResultAsync(id, dto);
                if (match == null)
                    return NotFound(new { message = "Partido no encontrado" });

                return Ok(match);
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
                var deleted = await _matchService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { message = "Partido no encontrado" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateMatchStatusDto dto)
        {
            try
            {
                var match = await _matchService.UpdateStatusAsync(id, dto.Status);
                if (match == null)
                    return NotFound(new { message = "Partido no encontrado" });

                return Ok(match);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("stats/{leagueId:guid}")]
        public async Task<IActionResult> GetLeagueStats(Guid leagueId)
        {
            var stats = await _matchService.GetLeagueStatsAsync(leagueId);
            return Ok(stats);
        }
    }
}
