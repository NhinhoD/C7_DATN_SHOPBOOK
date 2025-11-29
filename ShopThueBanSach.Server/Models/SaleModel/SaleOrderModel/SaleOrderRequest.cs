using ShopThueBanSach.Server.Models.CartRentModel;

namespace ShopThueBanSach.Server.Models.SaleModel.SaleOrderModel
{
    public class SaleOrderRequest
    {
        public string UserId { get; set; } = null!;
		public string? Username { get; set; } 
		public string PaymentMethod { get; set; } = "cash";
        public bool HasShippingFee { get; set; }  // Người dùng chọn vận chuyển hay không
        public string? Address { get; set; }      // Nếu có vận chuyển, yêu cầu nhập địa chỉ
        public string? Phone { get; set; }
        public string? VoucherCode { get; set; } // 🆕 Mã voucher người dùng nhập
        public List<string> SelectedProductIds { get; set; } = new(); // Chỉ các sản phẩm được chọn
    }
}
