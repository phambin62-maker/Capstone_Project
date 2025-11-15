using BE_Capstone_Project.Infrastructure;
using BE_Capstone_Project.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ClosedXML.Excel;
using BE_Capstone_Project.Application.Report.Services.Interfaces;
using BE_Capstone_Project.Application.Report.DTOs;
using BE_Capstone_Project.Domain.Enums;
using System.Collections.Generic; 
using System.Threading.Tasks;  
using System.IO;              
using System.Linq;
using System;

namespace BE_Capstone_Project.Application.Report.Services
{
    public class ReportService : IReportService
    {
        private readonly OtmsdbContext _context;

        public ReportService(OtmsdbContext context)
        {
            _context = context;
        }

        public async Task<RevenueOverviewDto> GetRevenueOverviewAsync(DateOnly from, DateOnly to, int topN = 5)
        {
            var paid = PaidBookingsInRange(from, to);
            Console.WriteLine($"The total number of bookings: {paid.Count()}");

            var totalBookings = await paid.CountAsync();
            var totalRevenue = await paid.SumAsync(b => b.TotalPrice) ?? 0m;
            var uniqueUsers = await paid.Select(b => b.UserId).Distinct().CountAsync();

            var bookingIds = await paid.Select(b => b.Id).ToListAsync();
            int uniqueCustomers = 0;
            if (bookingIds.Any())
            {
                uniqueCustomers = await _context.BookingCustomers
                    .Where(bc => bookingIds.Contains(bc.BookingId))
                    .Select(bc => bc.Email ?? bc.PhoneNumber)
                    .Distinct()
                    .CountAsync();
            }

            //  TÍNH ĐÁNH GIÁ TRUNG BÌNH ===
            var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
            var toDateTime = to.ToDateTime(TimeOnly.MinValue);

            var averageRating = await _context.Reviews
                .Where(r => r.CreatedDate >= fromDateTime && r.CreatedDate < toDateTime)
                .AverageAsync(r => (decimal?)r.Stars) ?? 0m;

            var tourRevenueQuery = from b in paid
                                   join ts in _context.TourSchedules on b.TourScheduleId equals ts.Id
                                   join t in _context.Tours on ts.TourId equals t.Id
                                   group new { b, t } by new { t.Id, t.Name } into g
                                   select new
                                   {
                                       TourId = g.Key.Id,
                                       TourName = g.Key.Name,
                                       BookingsCount = g.Count(),
                                       Revenue = g.Sum(x => x.b.TotalPrice)
                                   };

            var topByRevenue = await tourRevenueQuery
                .OrderByDescending(x => x.Revenue)
                .Take(topN)
                .Select(x => new TourRevenueDto
                {
                    TourId = x.TourId,
                    TourName = x.TourName,
                    BookingsCount = x.BookingsCount,
                    Revenue = x.Revenue,
                    SeatsSold = 0
                }).ToListAsync();

            var topByBookings = await tourRevenueQuery
                .OrderByDescending(x => x.BookingsCount)
                .Take(topN)
                .Select(x => new TourPopularityDto
                {
                    TourId = x.TourId,
                    TourName = x.TourName,
                    BookingsCount = x.BookingsCount,
                    Revenue = x.Revenue
                }).ToListAsync();

            return new RevenueOverviewDto
            {
                From = from,
                To = to.AddDays(-1), // Trừ 1 ngày trả về cho nhất quán
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                UniqueBookingUsers = uniqueUsers,
                UniqueCustomers = uniqueCustomers,
                TopToursByRevenue = topByRevenue,
                TopToursByBookings = topByBookings,
                AverageRating = averageRating
            };
        }

        public async Task<List<TourPopularityDto>> GetTopToursByBookingsAsync(DateOnly from, DateOnly to, int topN = 10)
        {
            var paid = PaidBookingsInRange(from, to);

            var q = from b in paid
                    join ts in _context.TourSchedules on b.TourScheduleId equals ts.Id
                    join t in _context.Tours on ts.TourId equals t.Id
                    group new { b, t } by new { t.Id, t.Name } into g
                    select new TourPopularityDto
                    {
                        TourId = g.Key.Id,
                        TourName = g.Key.Name,
                        BookingsCount = g.Count(),
                        Revenue = g.Sum(x => x.b.TotalPrice)
                    };

            return await q.OrderByDescending(x => x.BookingsCount).Take(topN).ToListAsync();
        }

        public async Task<List<TourRevenueDto>> GetTopToursByRevenueAsync(DateOnly from, DateOnly to, int topN = 10)
        {
            var paid = PaidBookingsInRange(from, to);

            var q = from b in paid
                    join ts in _context.TourSchedules on b.TourScheduleId equals ts.Id
                    join t in _context.Tours on ts.TourId equals t.Id
                    group new { b, t } by new { t.Id, t.Name } into g
                    select new TourRevenueDto
                    {
                        TourId = g.Key.Id,
                        TourName = g.Key.Name,
                        BookingsCount = g.Count(),
                        Revenue = g.Sum(x => x.b.TotalPrice),
                        SeatsSold = 0
                    };

            return await q.OrderByDescending(x => x.Revenue).Take(topN).ToListAsync();
        }

        public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int year)
        {
            var q = _context.Bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Completed
                && b.PaymentDate != null && b.PaymentDate.Value.Year == year)
                .Select(b => new
                {
                    Date = b.PaymentDate.Value,
                    b.TotalPrice,
                });

            var grouped = await q
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(x => x.TotalPrice) ?? 0m,
                    BookingsCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return grouped;
        }

        public async Task<byte[]> ExportRevenueOverviewToExcelAsync(DateOnly from, DateOnly to)
        {
            var overview = await GetRevenueOverviewAsync(from, to, topN: 50);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("RevenueOverview");

            // Header
            ws.Cell(1, 1).Value = "From";
            ws.Cell(1, 2).Value = "To";
            ws.Cell(2, 1).Value = overview.From.ToString("o");
            ws.Cell(2, 2).Value = overview.To.ToString("o");

            ws.Cell(4, 1).Value = "TotalBookings";
            ws.Cell(4, 2).Value = overview.TotalBookings;
            ws.Cell(5, 1).Value = "TotalRevenue";
            ws.Cell(5, 2).Value = overview.TotalRevenue;
            ws.Cell(6, 1).Value = "UniqueBookingUsers";
            ws.Cell(6, 2).Value = overview.UniqueBookingUsers;
            ws.Cell(7, 1).Value = "UniqueCustomers";
            ws.Cell(7, 2).Value = overview.UniqueCustomers;
            ws.Cell(8, 1).Value = "AverageRating";
            ws.Cell(8, 2).Value = overview.AverageRating;


            // Convert to DataTables
            var dtTopRevenue = ToDataTable(overview.TopToursByRevenue);
            var dtTopBookings = ToDataTable(overview.TopToursByBookings);

            // Insert Top Tours by Revenue
            var startRow = 10;
            ws.Cell(startRow, 1).Value = "Top Tours by Revenue";
            ws.Cell(startRow + 1, 1).InsertTable(dtTopRevenue, true);

            // Insert Top Tours by Bookings
            var startRow2 = startRow + dtTopRevenue.Rows.Count + 4;
            ws.Cell(startRow2, 1).Value = "Top Tours by Bookings";
            ws.Cell(startRow2 + 1, 1).InsertTable(dtTopBookings, true);

            // Auto-fit
            ws.Columns().AdjustToContents();

            // Save to byte[]
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


        public static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in data)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private IQueryable<Booking> PaidBookingsInRange(DateOnly from, DateOnly to)
        {
            var toDateTime = to.ToDateTime(TimeOnly.MinValue);
            return _context.Bookings.Where(b => b.PaymentStatus == PaymentStatus.Completed &&
                  b.PaymentDate != null &&
                  b.PaymentDate >= from && 
                  b.PaymentDate < to
            );
        }

        //PHÂN TÍCH KHÁCH HÀNG ===
        public async Task<CustomerAnalysisDto> GetCustomerAnalysisAsync(DateOnly from, DateOnly to)
        {
            var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
            var toDateTime = to.ToDateTime(TimeOnly.MinValue);

            // 1. Lấy Role "Customer"
            var customerRole = await _context.Roles.AsNoTracking()
                                         .FirstOrDefaultAsync(r => r.RoleName == "Customer");

            if (customerRole == null)
            {
                // Nếu không có role Customer, trả về DTO rỗng
                return new CustomerAnalysisDto();
            }

            // 2. Tạo truy vấn cơ sở cho tất cả khách hàng (tính all-time)
            var allCustomersQuery = _context.Users
                                          .AsNoTracking()
                                          .Where(u => u.RoleId == customerRole.Id);

            // 3. Tính "Tổng khách hàng" (all-time)
            var totalCustomers = await allCustomersQuery.CountAsync();

            if (totalCustomers == 0)
            {
                // Tránh lỗi chia cho 0
                return new CustomerAnalysisDto { TotalCustomers = 0, LoyalCustomers = 0, NewCustomersInRange = 0, ReturnRate = 0 };
            }

            //  Tính "Khách mới tháng này" (Khách hàng được tạo TRONG KHOẢNG NGÀY)
            var newCustomersInRange = await allCustomersQuery
                .Where(u => u.CreatedDate != null && // <-- THÊM KIỂM TRA NULL
                u.CreatedDate.Value >= fromDateTime && // <-- DÙNG .Value
                u.CreatedDate.Value < toDateTime) // <-- DÙNG .Value
                .CountAsync();

            //  Tính "Khách hàng thân thiết" (có > 1 booking đã thanh toán, tính all-time)
            var loyalCustomers = await _context.Bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Completed)
                .Join(allCustomersQuery,
                      b => b.UserId,
                      u => u.Id,
                      (b, u) => u.Id) 
                .GroupBy(userId => userId)
                .Where(g => g.Count() > 1) // Lọc những user có > 1 booking
                .CountAsync(); // Đếm số lượng user đó

            //  Tính "Tỷ lệ quay lại"
            var returnRate = (decimal)loyalCustomers / totalCustomers;

            return new CustomerAnalysisDto
            {
                TotalCustomers = totalCustomers,
                LoyalCustomers = loyalCustomers,
                NewCustomersInRange = newCustomersInRange,
                ReturnRate = returnRate
            };
        }
    }
}