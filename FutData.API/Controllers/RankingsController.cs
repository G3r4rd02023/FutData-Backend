using FutData.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutData.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingsController : ControllerBase
    {
        private readonly IRankingService _rankingService;

        public RankingsController(IRankingService rankingService)
        {
            _rankingService = rankingService;
        }

        [Authorize]
        [HttpGet("general")]
        public async Task<IActionResult> GetGeneralRanking()
        {
            var rankings = await _rankingService.GetGeneralRankingAsync();
            return Ok(rankings);
        }

        [Authorize]
        [HttpGet("top-teams")]
        public async Task<IActionResult> GetTopTeams([FromQuery] int count = 10)
        {
            var rankings = await _rankingService.GetTopTeamsAsync(count);
            return Ok(rankings);
        }

        [Authorize]
        [HttpGet("team/{teamId:guid}")]
        public async Task<IActionResult> GetTeamStats(Guid teamId)
        {
            var stats = await _rankingService.GetTeamStatsAsync(teamId);
            if (stats == null)
                return NotFound(new { message = "Equipo no encontrado o sin partidos jugados" });

            return Ok(stats);
        }
    }
}
