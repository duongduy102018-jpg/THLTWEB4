namespace Webbanhang.Models
{
    public class RevenueReportViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CompletedRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal ConversionRate { get; set; }
        public int PendingOrders { get; set; }
        public int SentOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public string RevenueChartLabelsJson { get; set; } = "[]";
        public string RevenueChartDataJson { get; set; } = "[]";
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
    }

    public class TopSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}
