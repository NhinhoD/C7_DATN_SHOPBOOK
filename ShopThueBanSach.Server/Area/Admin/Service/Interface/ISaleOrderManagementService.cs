using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Service.Interface
{
    public interface ISaleOrderManagementService
    {
        // Lấy danh sách tất cả đơn hàng
        Task<List<SaleOrder>> GetAllSaleOrdersAsync();

        // Lấy danh sách theo trạng thái (Pending, Confirmed,...)
        Task<List<SaleOrder>> GetSaleOrdersByStatusAsync(OrderStatus status);

        // Lấy chi tiết 1 đơn hàng theo ID
        Task<SaleOrder?> GetSaleOrderByIdAsync(string orderId);

        // Lấy danh sách chi tiết sản phẩm trong đơn
        Task<List<SaleOrderDetail>> GetSaleOrderDetailsAsync(string orderId);

        // Cập nhật trạng thái đơn (Pending → Confirmed → Shipping → Delivered → ...)
        Task<bool> UpdateSaleOrderStatusAsync(string orderId, OrderStatus newStatus);

        // Cập nhật tổng tiền đơn hàng nếu cần (VD: admin chỉnh sửa giá ship, voucher,...)
        Task<bool> UpdateSaleOrderAmountAsync(string orderId, decimal newTotal);

        // Xoá đơn hàng (tuỳ trường hợp cho phép hoặc chỉ đánh dấu IsCanceled)
        Task<bool> CancelSaleOrderAsync(string orderId);
    }
}
