using ShopThueBanSach.Server.Models.CartRentModel;

namespace ShopThueBanSach.Server.Models.Vnpay
{
	public class PaymentInformationRentModel
	{
		public string Name { get; set; } = string.Empty;
		public string OrderDescription { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public string OrderType { get; set; } = "rent";
		public string UserId { get; set; } = string.Empty;
		public string ReturnUrl { get; set; } = string.Empty;
		public List<CartItemRent> CartItemsRent { get; set; } = new(); // ✅ Dành cho đơn thuê
	}
}