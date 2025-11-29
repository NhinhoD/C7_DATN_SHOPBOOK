using ShopThueBanSach.Server.Models.BooksModel.SaleBooks;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface ISaleBookService
    {
        Task<List<SaleBookDto>> GetAllAsync();
        Task<SaleBookDto?> GetByIdAsync(string id);
        Task<string> CreateAsync(CreateSaleBookDto dto);
        Task<bool> UpdateAsync(string id, UpdateSaleBookDto dto);  // Đổi từ SaleBookDto sang UpdateSaleBookDto

        Task<bool> DeleteAsync(string id);
        Task<bool> SetVisibilityAsync(string id, bool isHidden);
        Task<bool> CheckTitleExistsAsync(string title, string? excludeId = null);
    }
}
