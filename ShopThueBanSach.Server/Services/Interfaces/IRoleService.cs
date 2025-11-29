using Microsoft.AspNetCore.Identity;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<IdentityRole?> GetByIdAsync(string id);
        Task<bool> CreateAsync(string roleName);
        Task<bool> UpdateAsync(string id, string newName);
        Task<bool> DeleteAsync(string id);
    }
}
