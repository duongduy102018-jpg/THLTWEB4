using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webbanhang.Models;

namespace Webbanhang.Controllers
{
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Revenue()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var validOrders = orders
                .Where(o => !string.Equals(o.Status, "Đã hủy", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var completedOrders = orders
                .Where(o => IsCompletedStatus(o.Status))
                .ToList();

            var chartStartDate = DateTime.Today.AddDays(-6);
            var chartLabels = Enumerable.Range(0, 7)
                .Select(i => chartStartDate.AddDays(i))
                .ToList();

            var chartData = chartLabels
                .Select(day => validOrders
                    .Where(o => o.CreatedAt.Date == day.Date)
                    .Sum(o => o.Total))
                .ToList();

            var topSellingProducts = validOrders
                .SelectMany(o => o.Items)
                .GroupBy(i => new { i.ProductId, i.ProductName })
                .Select(g => new TopSellingProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Total)
                })
                .OrderByDescending(x => x.QuantitySold)
                .ThenByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var model = new RevenueReportViewModel
            {
                TotalOrders = orders.Count,
                TotalRevenue = validOrders.Sum(o => o.Total),
                CompletedRevenue = completedOrders.Sum(o => o.Total),
                AverageOrderValue = validOrders.Any() ? validOrders.Average(o => o.Total) : 0m,
                ConversionRate = orders.Any() ? Math.Round(completedOrders.Count * 100m / orders.Count, 2) : 0m,
                PendingOrders = orders.Count(o => IsProcessingStatus(o.Status)),
                SentOrders = orders.Count(o => IsSentStatus(o.Status)),
                ShippingOrders = orders.Count(o => string.Equals(o.Status, "Đang vận chuyển", StringComparison.OrdinalIgnoreCase) || string.Equals(o.Status, "Đang giao", StringComparison.OrdinalIgnoreCase)),
                CompletedOrders = completedOrders.Count,
                CancelledOrders = orders.Count(o => string.Equals(o.Status, "Đã hủy", StringComparison.OrdinalIgnoreCase)),
                RevenueChartLabelsJson = JsonSerializer.Serialize(chartLabels.Select(d => d.ToString("dd/MM"))),
                RevenueChartDataJson = JsonSerializer.Serialize(chartData),
                TopSellingProducts = topSellingProducts,
                RecentOrders = orders.Take(20).ToList()
            };

            return View(model);
        }

        private static bool IsCompletedStatus(string? status)
        {
            return string.Equals(status, "Đã giao", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Hoàn tất", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Hoàn thành", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsProcessingStatus(string? status)
        {
            return string.Equals(status, "Đang xử lý", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Đã xác nhận", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Chờ xác nhận", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSentStatus(string? status)
        {
            return string.Equals(status, "Đã gửi", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Đã gửi hàng", StringComparison.OrdinalIgnoreCase);
        }
    }
}
