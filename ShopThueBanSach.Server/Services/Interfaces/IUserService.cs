using ShopThueBanSach.Server.Models.UserModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetProfileAsync();
        Task<string?> UpdateProfileAsync(UpdateProfileDto model);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
