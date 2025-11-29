using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;

namespace ShopThueBanSach.Server.Models.Vnpay
{
	public class PaymentResponseModel
	{
		public string UserId { get; set; }
		public List<CartItemSale> CartItems { get; set; }
		public decimal Amount { get; set; }
		public string OrderDescription { get; set; }
		public string Tick { get; set; }

		// Nếu cần thêm các trường khác từ VNPAY:
		public string TransactionId { get; set; }
		public string OrderId { get; set; }
		public string PaymentMethod { get; set; }
		public string PaymentId { get; set; }
		public bool Success { get; set; }
		public string Token { get; set; }
		public string VnPayResponseCode { get; set; }
	}
}