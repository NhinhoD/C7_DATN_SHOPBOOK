using ShopThueBanSach.Server.Models.CartRentModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface ICartRentService
    {
        /// <summary>
        /// Lấy toàn bộ giỏ hàng hiện tại từ session.
        /// </summary>
        List<CartItemRent> GetCart();

        /// <summary>
        /// Thêm sách thuê (RentBookItem) vào giỏ hàng theo ID.
        /// </summary>
        Task<bool> AddToCartAsync(string rentBookItemId);

        /// <summary>
        /// Xóa 1 mục khỏi giỏ hàng.
        /// </summary>
        void RemoveFromCart(string rentBookItemId);

        /// <summary>
        /// Đánh dấu chọn / bỏ chọn 1 sản phẩm để thanh toán.
        /// </summary>
        void ToggleSelect(string rentBookItemId);

        /// <summary>
        /// Xóa toàn bộ giỏ hàng khỏi session.
        /// </summary>
        void ClearCart();

        /// <summary>
        /// Lấy danh sách các sản phẩm được chọn để thanh toán.
        /// </summary>
        List<CartItemRent> GetSelectedItems();

        /// <summary>
        /// Cập nhật lại phí thuê trong giỏ hàng theo số ngày.
        /// </summary>
        void RecalculateRentalFee(DateTime startDate, DateTime endDate);
    }
}
