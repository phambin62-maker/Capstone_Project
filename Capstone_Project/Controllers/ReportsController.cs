using BE_Capstone_Project.Services.Interfaces;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BE_Capstone_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetRevenueOverview([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int topN = 5)
        {
            if(from > to) return BadRequest("Invalid date range");
            var result = await _reportService.GetRevenueOverviewAsync(from, to.AddDays(1), topN);
            return Ok(result);
        }

        [HttpGet("top-tours")]
        public async Task<IActionResult> GetTopTours([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int topN = 10)
        {
            var list = await _reportService.GetTopToursByBookingsAsync(from, to.AddDays(1), topN);
            return Ok(list);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int year)
        {
            var list = await _reportService.GetMonthlyRevenueAsync(year);
            return Ok(list);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportRevenueOverview([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            if (from > to) return BadRequest("Invalid date range");
            var fileBytes = await _reportService.ExportRevenueOverviewToExcelAsync(from, to.AddDays(1));
            var fileName = $"Revenue_Overview_{from:ddMMyyyy}_{to:ddMMyyyy}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
