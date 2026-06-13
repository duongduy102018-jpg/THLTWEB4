using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webbanhang.Helpers;
using Webbanhang.Models;
using Webbanhang.Repositories;

namespace Webbanhang.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "HTPFoodCart";
        private const string CouponSessionKey = "HTPFoodCoupon";

        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(BuildSummary());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            AddProductToCart(product, quantity);

            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";

            var referer = Request.Headers.Referer.ToString();

            return string.IsNullOrWhiteSpace(referer)
                ? RedirectToAction("Index", "Product")
                : Redirect(referer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int id, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            AddProductToCart(product, quantity);

            TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng. Vui lòng kiểm tra giỏ hàng trước khi thanh toán.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CouponSessionKey);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyCoupon(string? couponCode)
        {
            var code = couponCode?.Trim().ToUpperInvariant();
            var subtotal = GetCart().Sum(x => x.Total);

            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "Vui lòng nhập mã giảm giá.";
            }
            else if (code == "FRESH10")
            {
                HttpContext.Session.SetString(CouponSessionKey, code);
                TempData["Success"] = "Áp dụng mã FRESH10 thành công: giảm 10%, tối đa 50.000 VNĐ.";
            }
            else if (code == "COMBO50" && subtotal >= 300000)
            {
                HttpContext.Session.SetString(CouponSessionKey, code);
                TempData["Success"] = "Áp dụng mã COMBO50 thành công: giảm 50.000 VNĐ.";
            }
            else if (code == "COMBO50")
            {
                TempData["Error"] = "Mã COMBO50 chỉ áp dụng cho đơn từ 300.000 VNĐ.";
            }
            else
            {
                TempData["Error"] = "Mã giảm giá không hợp lệ. Gợi ý: FRESH10 hoặc COMBO50.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.Remove(CouponSessionKey);

            TempData["Success"] = "Đã bỏ mã giảm giá.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var summary = BuildSummary();

            if (!summary.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            return View(summary);
        }

        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            var data = await _context.Provinces
                .AsNoTracking()
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name)
                .Select(p => new { id = p.Id, name = p.Name })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var data = await _context.Districts
                .AsNoTracking()
                .Where(d => d.ProvinceId == provinceId)
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var data = await _context.Wards
                .AsNoTracking()
                .Where(w => w.DistrictId == districtId)
                .OrderBy(w => w.Name)
                .Select(w => new { id = w.Id, name = w.Name })
                .ToListAsync();

            return Json(data);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(
            string customerName,
            string phone,
            string? streetAddress,
            string? city,
            string? district,
            string? ward,
            string? address,
            string paymentMethod,
            string? note)
        {
            var summary = BuildSummary();

            if (!summary.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            var fullAddress = BuildFullAddress(streetAddress, ward, district, city, address);

            if (string.IsNullOrWhiteSpace(customerName) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(fullAddress))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ giao hàng.";
                return RedirectToAction(nameof(Checkout));
            }

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user?.Id,
                CustomerName = customerName.Trim(),
                Phone = phone.Trim(),
                Address = fullAddress,
                Note = note?.Trim(),
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "COD" : paymentMethod,
                CouponCode = summary.CouponCode,
                Subtotal = summary.Subtotal,
                Discount = summary.Discount,
                ShippingFee = summary.ShippingFee,
                Total = summary.GrandTotal,
                Status = "Đã xác nhận",
                CreatedAt = DateTime.Now,
                Items = summary.Items.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Total = x.Total
                }).ToList()
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            RememberOrderForCurrentUser(order.Id);

            HttpContext.Session.Remove(CartSessionKey);
            HttpContext.Session.Remove(CouponSessionKey);

            TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #{order.Id}.";

            return RedirectToAction(nameof(Success), new { id = order.Id });
        }

        [Authorize]
        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!CanViewOrder(order))
            {
                TempData["Error"] = "Bạn không có quyền xem đơn hàng này.";
                return Redirect("/Identity/Account/AccessDenied");
            }

            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> Orders(string? statusFilter, string? keyword)
        {
            var isAdmin = User.IsInRole(SD.Role_Admin);
            var isEmployee = User.IsInRole(SD.Role_Employee);
            var canManageOrders = isAdmin || isEmployee;

            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsEmployee = isEmployee;
            ViewBag.CanManageOrders = canManageOrders;
            ViewBag.StatusFilter = statusFilter ?? "Tất cả";
            ViewBag.Keyword = keyword ?? string.Empty;

            IQueryable<Order> query = _context.Orders
                .Include(o => o.Items)
                .Include(o => o.User);

            if (!canManageOrders)
            {
                var userId = _userManager.GetUserId(User);
                var ids = GetMyOrderIds();

                query = query.Where(o => o.UserId == userId || ids.Contains(o.Id));
            }

            var allVisibleOrders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orders = canManageOrders
                ? ApplyOrderStatusFilter(allVisibleOrders, statusFilter)
                : allVisibleOrders;

            if (canManageOrders && !string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim();
                orders = orders.Where(o =>
                    o.Id.ToString().Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerName.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    o.Phone.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    o.Address.Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (canManageOrders)
            {
                var validOrders = allVisibleOrders.Where(o => !IsCancelledStatus(o.Status)).ToList();
                ViewBag.TotalRevenue = validOrders.Sum(o => o.Total);
                ViewBag.CompletedRevenue = allVisibleOrders.Where(o => IsCompletedStatus(o.Status)).Sum(o => o.Total);
                ViewBag.TotalOrders = allVisibleOrders.Count;
                ViewBag.ProcessingOrders = allVisibleOrders.Count(o => IsProcessingStatus(o.Status));
                ViewBag.SentOrders = allVisibleOrders.Count(o => IsSentStatus(o.Status));
                ViewBag.CompletedOrders = allVisibleOrders.Count(o => IsCompletedStatus(o.Status));
                ViewBag.CancelledOrders = allVisibleOrders.Count(o => IsCancelledStatus(o.Status));
            }

            var validCustomerOrders = allVisibleOrders
                .Where(o => !string.Equals(o.Status, "Đã hủy", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var memberPoints = CalculateMemberPoints(validCustomerOrders);
            ViewBag.MemberPoints = memberPoints;
            ViewBag.MemberTier = GetMemberTier(memberPoints);
            ViewBag.NextTierText = GetNextTierText(memberPoints);

            return View(orders);
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public async Task<IActionResult> ExportOrders(string? statusFilter, string format = "csv")
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            orders = ApplyOrderStatusFilter(orders, statusFilter);

            if (string.Equals(format, "excel", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(format, "xls", StringComparison.OrdinalIgnoreCase))
            {
                var html = BuildOrdersExcelHtml(orders);
                var excelBytes = Encoding.UTF8.GetBytes(html);
                return File(excelBytes, "application/vnd.ms-excel", $"don-hang-{DateTime.Now:yyyyMMddHHmm}.xls");
            }

            var csv = BuildOrdersCsv(orders);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
            return File(bytes, "text/csv", $"don-hang-{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!CanViewOrder(order))
            {
                TempData["Error"] = "Bạn không có quyền xem đơn hàng này.";
                return Redirect("/Identity/Account/AccessDenied");
            }

            var userId = _userManager.GetUserId(User);
            var myOrderIds = GetMyOrderIds();
            var myOrders = await _context.Orders
                .Where(o => (o.UserId == userId || myOrderIds.Contains(o.Id)) && o.Status != "Đã hủy")
                .ToListAsync();

            var memberPoints = CalculateMemberPoints(myOrders);
            ViewBag.MemberPoints = memberPoints;
            ViewBag.MemberTier = GetMemberTier(memberPoints);

            return View(order);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReOrderItem(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                TempData["Error"] = "Sản phẩm này hiện không còn tồn tại trong cửa hàng.";
                return RedirectToAction(nameof(Orders));
            }

            AddProductToCart(product, Math.Max(1, quantity));
            TempData["Success"] = "Đã thêm lại sản phẩm cũ vào giỏ hàng.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (!CanViewOrder(order))
            {
                TempData["Error"] = "Bạn không có quyền đặt lại đơn hàng này.";
                return Redirect("/Identity/Account/AccessDenied");
            }

            var addedCount = 0;
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    AddProductToCart(product, Math.Max(1, item.Quantity));
                    addedCount++;
                }
            }

            TempData[addedCount > 0 ? "Success" : "Error"] = addedCount > 0
                ? $"Đã thêm lại {addedCount} mặt hàng từ đơn #{order.Id} vào giỏ hàng."
                : "Các sản phẩm trong đơn cũ hiện không còn tồn tại trong cửa hàng.";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                order.Status = status;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật trạng thái đơn #{order.Id}.";

            return RedirectToAction(nameof(Orders));
        }

        private void AddProductToCart(Product product, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == product.Id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = Math.Max(1, quantity)
                });
            }
            else
            {
                item.Quantity += Math.Max(1, quantity);
            }

            SaveCart(cart);
        }

        private CartSummaryViewModel BuildSummary()
        {
            var items = GetCart();
            var subtotal = items.Sum(x => x.Total);
            var coupon = HttpContext.Session.GetString(CouponSessionKey);
            var discount = CalculateDiscount(coupon, subtotal);
            var shippingFee = subtotal == 0 || subtotal >= 500000 ? 0 : 25000;

            var cartProductIds = items.Select(x => x.ProductId).ToList();
            var recommendedProducts = _context.Products
                .AsNoTracking()
                .Where(p => !cartProductIds.Contains(p.Id))
                .OrderBy(p => p.Id)
                .Take(4)
                .ToList();

            return new CartSummaryViewModel
            {
                Items = items,
                RecommendedProducts = recommendedProducts,
                CouponCode = coupon,
                Subtotal = subtotal,
                Discount = discount,
                ShippingFee = shippingFee,
                GrandTotal = Math.Max(0, subtotal - discount + shippingFee)
            };
        }

        private static decimal CalculateDiscount(string? coupon, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(coupon) || subtotal <= 0)
            {
                return 0;
            }

            return coupon.ToUpperInvariant() switch
            {
                "FRESH10" => Math.Min(subtotal * 0.10m, 50000m),
                "COMBO50" when subtotal >= 300000m => 50000m,
                _ => 0m
            };
        }


        private static string BuildFullAddress(string? streetAddress, string? ward, string? district, string? city, string? fallbackAddress)
        {
            var parts = new[] { streetAddress, ward, district, city }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim());

            var fullAddress = string.Join(", ", parts);

            return string.IsNullOrWhiteSpace(fullAddress)
                ? fallbackAddress?.Trim() ?? string.Empty
                : fullAddress;
        }

        private static List<Order> ApplyOrderStatusFilter(List<Order> orders, string? statusFilter)
        {
            if (string.IsNullOrWhiteSpace(statusFilter) || statusFilter == "Tất cả")
            {
                return orders;
            }

            return statusFilter switch
            {
                "Đang xử lý" => orders.Where(o => IsProcessingStatus(o.Status)).ToList(),
                "Đã gửi" => orders.Where(o => IsSentStatus(o.Status)).ToList(),
                "Đã giao" => orders.Where(o => IsCompletedStatus(o.Status)).ToList(),
                "Đã hủy" => orders.Where(o => IsCancelledStatus(o.Status)).ToList(),
                _ => orders.Where(o => string.Equals(o.Status, statusFilter, StringComparison.OrdinalIgnoreCase)).ToList()
            };
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

        private static bool IsCompletedStatus(string? status)
        {
            return string.Equals(status, "Đã giao", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Hoàn tất", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Hoàn thành", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCancelledStatus(string? status)
        {
            return string.Equals(status, "Đã hủy", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildOrdersCsv(IEnumerable<Order> orders)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Ma don,Khach hang,So dien thoai,Dia chi,Ngay dat,Trang thai,Thanh toan,Tam tinh,Giam gia,Phi ship,Tong tien,San pham");

            foreach (var order in orders)
            {
                var items = string.Join(" | ", order.Items.Select(i => $"{i.ProductName} x {i.Quantity}"));
                var values = new[]
                {
                    order.Id.ToString(),
                    order.CustomerName,
                    order.Phone,
                    order.Address,
                    order.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    order.Status,
                    order.PaymentMethod,
                    order.Subtotal.ToString("0"),
                    order.Discount.ToString("0"),
                    order.ShippingFee.ToString("0"),
                    order.Total.ToString("0"),
                    items
                };

                sb.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return sb.ToString();
        }

        private static string BuildOrdersExcelHtml(IEnumerable<Order> orders)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'></head><body><table border='1'>");
            sb.AppendLine("<tr><th>Mã đơn</th><th>Khách hàng</th><th>Số điện thoại</th><th>Địa chỉ</th><th>Ngày đặt</th><th>Trạng thái</th><th>Thanh toán</th><th>Tạm tính</th><th>Giảm giá</th><th>Phí ship</th><th>Tổng tiền</th><th>Sản phẩm</th></tr>");

            foreach (var order in orders)
            {
                var items = string.Join(" | ", order.Items.Select(i => $"{i.ProductName} x {i.Quantity}"));
                sb.AppendLine($"<tr><td>{order.Id}</td><td>{HtmlEncode(order.CustomerName)}</td><td>{HtmlEncode(order.Phone)}</td><td>{HtmlEncode(order.Address)}</td><td>{order.CreatedAt:dd/MM/yyyy HH:mm}</td><td>{HtmlEncode(order.Status)}</td><td>{HtmlEncode(order.PaymentMethod)}</td><td>{order.Subtotal:0}</td><td>{order.Discount:0}</td><td>{order.ShippingFee:0}</td><td>{order.Total:0}</td><td>{HtmlEncode(items)}</td></tr>");
            }

            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }

        private static string EscapeCsv(string? value)
        {
            value ??= string.Empty;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        private static string HtmlEncode(string? value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static int CalculateMemberPoints(IEnumerable<Order> orders)
        {
            return (int)Math.Floor(orders.Sum(o => o.Total) / 10000m);
        }

        private static string GetMemberTier(int points)
        {
            return points switch
            {
                >= 1000 => "Hạng Thành viên Kim Cương",
                >= 500 => "Hạng Thành viên Vàng",
                >= 200 => "Hạng Thành viên Bạc",
                _ => "Hạng Thành viên Đồng"
            };
        }

        private static string GetNextTierText(int points)
        {
            return points switch
            {
                < 200 => $"Còn {200 - points} điểm để lên Hạng Bạc",
                < 500 => $"Còn {500 - points} điểm để lên Hạng Vàng",
                < 1000 => $"Còn {1000 - points} điểm để lên Hạng Kim Cương",
                _ => "Bạn đang ở hạng cao nhất"
            };
        }

        private bool CanViewOrder(Order order)
        {
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                return true;
            }

            var userId = _userManager.GetUserId(User);

            return (!string.IsNullOrWhiteSpace(userId) && order.UserId == userId)
                || GetMyOrderIds().Contains(order.Id);
        }

        private void RememberOrderForCurrentUser(int orderId)
        {
            var ids = GetMyOrderIds();

            if (!ids.Contains(orderId))
            {
                ids.Add(orderId);
            }

            HttpContext.Session.SetString(AuthSession.MyOrderIdsKey, JsonSerializer.Serialize(ids));
        }

        private List<int> GetMyOrderIds()
        {
            var json = HttpContext.Session.GetString(AuthSession.MyOrderIdsKey);

            return string.IsNullOrWhiteSpace(json)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);

            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}