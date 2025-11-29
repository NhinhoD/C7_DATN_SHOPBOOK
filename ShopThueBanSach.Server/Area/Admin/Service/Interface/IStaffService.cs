using Microsoft.AspNetCore.Http;
using ShopThueBanSach.Server.Area.Admin.Entities;
using ShopThueBanSach.Server.Area.Admin.Model.StaffModel;
using ShopThueBanSach.Server.Models.AuthModel;
using ShopThueBanSach.Server.Models.StaffModel; // ⬅ Thêm namespace chứa StaffDto
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopThueBanSach.Server.Area.Admin.Service.Interface
{
    public interface IStaffService
    {
        // Lấy toàn bộ nhân viên có Role = "Staff"
        Task<IEnumerable<Staff>> GetAllAsync();

        // Lấy chi tiết một nhân viên theo StaffId
        Task<Staff?> GetByIdAsync(string id);

        // Thêm mới một nhân viên từ form upload (gồm ảnh)
        Task<Staff> AddAsync(StaffDto dto); // ⬅ Đổi từ Staff -> StaffDto

		// Cập nhật thông tin nhân viên (gồm ảnh mới nếu có)

		Task<Staff?> UpdateAsync(UpdateStaffDto dto, string id);

		// Xóa nhân viên
		Task<bool> DeleteAsync(string id);

        // Kiểm tra sự tồn tại của StaffId
        Task<bool> ExistsAsync(string staffId);

        // Lấy StaffId thông qua userId (Id trong bảng AspNetUsers)
        Task<string?> GetStaffIdByIdAsync(string userId);

        // Lấy StaffId thông qua email (trả về null nếu không tìm thấy)
        Task<string?> GetStaffIdByEmailAsync(string email);

        // Xoá nhân viên theo Email
        Task<bool> DeleteByEmailAsync(string email);

        // Xoá nhân viên theo Id (bổ sung)
        Task<bool> DeleteByIdAsync(string id);


        // Lấy toàn bộ user có role là "Staff" dưới dạng DTO
        Task<IEnumerable<UserDto>> GetAllStaffUsersAsync();
    }
}
