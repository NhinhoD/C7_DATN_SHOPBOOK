using ShopThueBanSach.Server.Models.BooksModel.DiscountCode;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IDiscountCodeService
    {
        Task<List<DiscountCodeDTO>> GetAllAsync();
        Task<DiscountCodeDTO?> GetByIdAsync(string id);
        Task<bool> CreateAsync(CreateDiscountCodeDto model);
        Task<bool> UpdateAsync(string id, UpdateDiscountCodeDto model);
        Task<bool> DeleteAsync(string id);
        Task<List<DiscountCodeDTO>> GetAvailableForExchangeAsync();
    }
}
