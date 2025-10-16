using BE_Capstone_Project.DTOs;

namespace BE_Capstone_Project.Services.Interfaces
{
    public interface IReportService
    {
        Task<RevenueOverviewDto> GetRevenueOverviewAsync(DateOnly from, DateOnly to, int topN = 5);
        Task<List<TourPopularityDto>> GetTopToursByBookingsAsync(DateOnly from, DateOnly to, int topN = 10);
        Task<List<TourRevenueDto>> GetTopToursByRevenueAsync(DateOnly from, DateOnly to, int topN = 10);
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year);
        Task<byte[]> ExportRevenueOverviewToExcelAsync(DateOnly from, DateOnly to);
    }
}
