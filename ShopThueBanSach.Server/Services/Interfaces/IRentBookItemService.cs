using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;
using ShopThueBanSach.Server.Models.BooksModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IRentBookItemService
    {
        Task<List<RentBookItemDto>> GetAllAsync();
        Task<RentBookItemDto?> GetByIdAsync(string id);
        Task<RentBookItemDto?> CreateAsync(RentBookItemDto dto);
        Task<RentBookItemDto?> UpdateAsync(string id, RentBookItemDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
