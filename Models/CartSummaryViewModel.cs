namespace Webbanhang.Models
{
    public class CartSummaryViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public List<Product> RecommendedProducts { get; set; } = new();
        public string? CouponCode { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public bool FreeShipping => ShippingFee <= 0;
    }
}
