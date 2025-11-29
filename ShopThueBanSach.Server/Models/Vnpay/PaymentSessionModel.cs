using ShopThueBanSach.Server.Models.CartRentModel;
using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;

namespace ShopThueBanSach.Server.Models.Vnpay
{
	public class PaymentSessionModel
	{
		public string? UserId { get; set; }

		public List<CartItemRent>? RentItems { get; set; }
	public string? UserName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public List<CartItemSale> CartItems { get; set; }
		public decimal Amount { get; set; }
		public string OrderDescription { get; set; }
		public string Tick { get; set; }

		public string TxnRef { get; set; } = string.Empty; // BẮT BUỘC có

		// Thêm để xử lý phí ship và liên lạc
		public bool HasShippingFee { get; set; }
		public string? Address { get; set; }
		public string? Phone { get; set; }
		public string? Voucher {  get; set; }
	}
}
