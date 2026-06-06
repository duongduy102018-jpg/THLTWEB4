using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webbanhang.Models;

namespace Webbanhang.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
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

            var model = new RevenueReportViewModel
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Total),
                CompletedRevenue = orders
                    .Where(o => string.Equals(o.Status, "Hoàn tất", StringComparison.OrdinalIgnoreCase))
                    .Sum(o => o.Total),
                PendingOrders = orders.Count(o => o.Status == "Chờ xác nhận" || o.Status == "Đang chuẩn bị"),
                ShippingOrders = orders.Count(o => o.Status == "Đang giao"),
                CompletedOrders = orders.Count(o => o.Status == "Hoàn tất"),
                CancelledOrders = orders.Count(o => o.Status == "Đã hủy"),
                RecentOrders = orders.Take(20).ToList()
            };

            return View(model);
        }
    }
}
