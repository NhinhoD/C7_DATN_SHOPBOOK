using ShopThueBanSach.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ShopThueBanSach.Server.Entities
{
    public class SaleOrder
    {
        [Key]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = null!;
        public string? Username { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
		public string? Description { get; set; }
		public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public bool HasShippingFee { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal OriginalTotalAmount { get; set; }
		public string? PaymentMethod { get; set; }
		public decimal TotalAmount { get; set; }
        public User User { get; set; } // hoặc tên entity tương ứng với khách hàng

        [NotMapped]
        public List<SaleOrderDetail> Details { get; set; } = new();
        [JsonIgnore]
        public virtual ICollection<SaleOrderDetail> SaleOrderDetails { get; set; }  // <-- Quan trọng!
    }
}
