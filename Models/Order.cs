using System.ComponentModel.DataAnnotations;

namespace Webbanhang.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required, StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Chờ xác nhận";

        [StringLength(30)]
        public string PaymentMethod { get; set; } = "COD";

        [StringLength(50)]
        public string? CouponCode { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<OrderItem> Items { get; set; } = new();
    }
}
