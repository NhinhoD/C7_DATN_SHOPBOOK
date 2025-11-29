using ShopThueBanSach.Server.Models.BooksModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<string> ExchangePointsForVoucherAsync(string userId, string discountCodeId);
        Task<List<VoucherDto>> GetUserCurrentVouchers(string userId); // Các mã chưa dùng
        Task<List<VoucherHistoryDto>> GetUserVoucherHistory(string userId); // Toàn bộ lịch sử
    }
}
