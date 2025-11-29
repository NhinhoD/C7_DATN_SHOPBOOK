using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel.RentBooks;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IRentBookService
    {
        Task<List<RentBookDto>> GetAllAsync();
        Task<RentBookDto?> GetByIdAsync(string id);
        Task<string> CreateAsync(CreateRentBookDto dto, IFormFile? imageFile); // ✅ chuẩn hóa
        Task<bool> UpdateAsync(string id, UpdateRentBookDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> SetVisibilityAsync(string id, bool isHidden);
        Task<bool> CheckTitleExistsAsync(string title, string? excludeId = null);
    }

}
