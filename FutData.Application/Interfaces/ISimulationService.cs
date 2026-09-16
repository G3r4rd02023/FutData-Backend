using FutData.Application.DTOs;

namespace FutData.Application.Interfaces
{
    public interface ISimulationService
    {
        Task<SimulationResult> SimulateAsync(SimulationRequest request);
        Task<BatchSimulationResult> SimulateBatchAsync(BatchSimulationRequest request);
        Task<List<SimulationHistoryDto>> GetHistoryAsync(Guid? leagueId = null);
        Task<SimulationStatsDto> GetStatsAsync(Guid? leagueId = null);
    }
}
