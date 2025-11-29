using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShopThueBanSach.Server.Models.CartRentModel;
using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Entities
{
    public class RentOrder
    {
        [Key]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string UserName { get; set; }

        public string UserId { get; set; }  // FK

        [ForeignKey("UserId")]
        public User? User { get; set; }     // Navigation property ✅

        [NotMapped]
        public List<CartItemRent> Items { get; set; } = new();

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RentalDays { get; set; }

        public bool HasShippingFee { get; set; } = false;
        public decimal ShippingFee { get; set; } = 0;

        public decimal TotalFee { get; set; }
        public decimal TotalDeposit { get; set; }

        public string ? Payment { get; set; }
		public string? Address { get; set; }
		public string? Phone { get; set; }
		public DateTime? ActualReturnDate { get; set; }
        public decimal? ActualRefundAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        // ✅ Thêm dòng này để EF hiểu mối quan hệ 1-n với RentOrderDetail
        public ICollection<RentOrderDetail> RentOrderDetails { get; set; } = new List<RentOrderDetail>();
    }
}