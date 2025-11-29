using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;

namespace ShopThueBanSach.Server.Models.Vnpay
{
	public class PaymentInformationModel
	{
		public string Name { get; set; } = string.Empty;
		public string OrderDescription { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public string OrderType { get; set; } = "other"; // default nếu không có
		public string UserId { get; set; } = string.Empty;
		public List<CartItemSale> CartItems { get; set; } = new();

	}
}