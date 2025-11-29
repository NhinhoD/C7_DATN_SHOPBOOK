namespace ShopThueBanSach.Server.Models.SaleModel.CartSaleModel
{
    public class CartItemSale
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
		public string? ImageUrl { get; set; } // Hình ảnh sản phẩm, có thể null nếu không có
		public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal SubTotal => UnitPrice * Quantity;

        public bool IsSelected { get; set; } = true; // ✅ Mặc định là chưa chọn
    }
}
