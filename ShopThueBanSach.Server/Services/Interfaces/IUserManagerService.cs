using ShopThueBanSach.Server.Models.AuthModel;
using ShopThueBanSach.Server.Models.UserModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IUserManagerService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string id);
        Task<bool> UpdateAsync(string id, UpdateUserDto dto);

        Task<bool> DeleteAsync(string id);
        Task<bool> UpdateCustomerAsync(string id, UpdateCustomerDto dto);


    }
}
