using FutData.Application.DTOs;
using FutData.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutData.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SimulationController : ControllerBase
    {
        private readonly ISimulationService _simulationService;

        public SimulationController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromBody] SimulationRequest request)
        {
            try
            {
                var result = await _simulationService.SimulateAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> SimulateBatch([FromBody] BatchSimulationRequest request)
        {
            try
            {
                var result = await _simulationService.SimulateBatchAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] Guid? leagueId)
        {
            var history = await _simulationService.GetHistoryAsync(leagueId);
            return Ok(history);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] Guid? leagueId)
        {
            var stats = await _simulationService.GetStatsAsync(leagueId);
            return Ok(stats);
        }
    }
}
