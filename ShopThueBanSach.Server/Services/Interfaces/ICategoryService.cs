using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(string id);
        Task<CategoryDto?> CreateAsync(CategoryDto dto);
        Task<CategoryDto?> UpdateAsync(string id, CategoryDto dto);
        Task<bool> DeleteAsync(string id);
    }
}
