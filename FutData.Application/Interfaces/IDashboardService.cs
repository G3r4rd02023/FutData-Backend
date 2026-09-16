using FutData.Application.DTOs;

namespace FutData.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task<DashboardSummaryOnlyDto> GetSummaryAsync();
        Task<DashboardChartsDto> GetChartsAsync();
    }
}
