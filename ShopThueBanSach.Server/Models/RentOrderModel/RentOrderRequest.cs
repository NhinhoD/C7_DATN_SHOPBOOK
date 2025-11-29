using ShopThueBanSach.Server.Models.CartRentModel;

namespace ShopThueBanSach.Server.Models.RentOrderModel
{
    public class RentOrderRequest
    {
        public string? UserId { get; set; } // UserId sẽ được gán từ token khi gọi API
        public string? UserName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool HasShippingFee { get; set; }

        public string? Address { get; set; }
        public string? Phone { get; set; }

        public string PaymentMethod { get; set; } // e.g. "Cash", "BankTransfer"
        public List<CartItemRent> CartItems { get; set; } = new();
    }
}
