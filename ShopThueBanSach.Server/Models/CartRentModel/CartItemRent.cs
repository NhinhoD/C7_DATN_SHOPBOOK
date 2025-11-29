namespace ShopThueBanSach.Server.Models.CartRentModel
{
    public class CartItemRent
    {
        public string RentBookItemId { get; set; } // mới: ID của RentBookItem
        public string RentBookTitle { get; set; }
        public decimal BookPrice { get; set; }
        public int Condition { get; set; }  // Tình trạng sách
        public string imageUrl { get; set; } // Hình ảnh sách, có thể null nếu không có
		public decimal RentalFee { get; set; }
        public decimal TotalFee { get; set; }
        public bool IsSelected { get; set; } = true;
        public int Quantity { get; set; } = 1;
    }
}
