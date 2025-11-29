using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<List<AuthorDto>> GetAllAsync();
        Task<AuthorDto?> GetByIdAsync(string id);
        Task<AuthorDto?> CreateAsync(CreateAuthorDto dto);
        Task<AuthorDto?> UpdateAsync(string id, AuthorUpdateDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> CheckNameExistsAsync(string name, string? excludeId = null);
    }
}
