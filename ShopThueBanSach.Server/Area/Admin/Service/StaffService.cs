using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Area.Admin.Entities;
using ShopThueBanSach.Server.Area.Admin.Model.StaffModel;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.AuthModel;
using ShopThueBanSach.Server.Models.StaffModel;

namespace ShopThueBanSach.Server.Area.Admin.Service
{
    public class StaffService : IStaffService
    {
        private readonly AppDBContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        public StaffService(AppDBContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
        {
            return await _context.Staffs.ToListAsync();
        }

        public async Task<Staff?> GetByIdAsync(string id)
        {
            return await _context.Staffs.FindAsync(id);
        }

        public async Task<Staff> AddAsync(StaffDto dto)
        {
            // Tạo StaffId nếu chưa có
            var staffId = dto.StaffId ?? Guid.NewGuid().ToString();

            // Kiểm tra xem user đã tồn tại chưa
            var existingUser = await _userManager.FindByIdAsync(staffId);
            if (existingUser == null)
            {
                var user = new User
                {
                    Id = staffId,
                    UserName = dto.FullName,
                    Email = dto.Email ?? $"{Guid.NewGuid()}@placeholder.local",
                    Address = dto.Address,
                    DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    throw new Exception("Tạo tài khoản Staff thất bại: " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));

                await EnsureRoleExistsAsync("Staff");
                await _userManager.AddToRoleAsync(user, "Staff");
            }
            else
            {
                var roles = await _userManager.GetRolesAsync(existingUser);
                if (!roles.Contains("Staff"))
                {
                    await EnsureRoleExistsAsync("Staff");
                    await _userManager.AddToRoleAsync(existingUser, "Staff");
                }
            }

            // Tạo đối tượng Staff
            var staff = new Staff
            {
                StaffId = staffId,
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow,
                Role = "Staff"
            };

            if (dto.ImageFile != null)
            {
                staff.ImageUser = await SaveImageAsync(dto.ImageFile);
            }

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            return staff;
        }

		public async Task<Staff?> UpdateAsync(UpdateStaffDto dto, string id)
		{
              
			var existing = await _context.Staffs.FindAsync(id);
			if (existing == null) return null;

            existing.FullName = dto.FullName;
            existing.PhoneNumber = dto.PhoneNumber;
            existing.Address = dto.Address;
            existing.DateOfBirth = dto.DateOfBirth;
           

            if (dto.ImageFile != null)
            {
                // Xoá ảnh cũ
                if (!string.IsNullOrEmpty(existing.ImageUser))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, existing.ImageUser.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                existing.ImageUser = await SaveImageAsync(dto.ImageFile);
            }

            var user = await _userManager.FindByIdAsync(existing.StaffId);
            if (user != null)
            {
                user.UserName = existing.FullName;
                user.Address = existing.Address;
                user.PhoneNumber = existing.PhoneNumber;
                user.DateOfBirth = existing.DateOfBirth ?? DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(existing.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, existing.Password);
                }

                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Staff"))
                {
                    await EnsureRoleExistsAsync("Staff");
                    await _userManager.AddToRoleAsync(user, "Staff");
                }
            }

            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task<bool> DeleteAsync(string id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null) return false;

            var user = await _userManager.FindByIdAsync(staff.StaffId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string staffId)
        {
            return await _context.Staffs.AnyAsync(s => s.StaffId == staffId);
        }

        public async Task<string?> GetStaffIdByIdAsync(string userId)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffId == userId);
            return staff?.StaffId;
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffId == id);
            if (staff == null) return false;

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllStaffUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Staff"))
                {
                    result.Add(MapToDto(user, "Staff"));
                }
            }
            return result;
        }

        public async Task<string?> GetStaffIdByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffId == user.Id);
            return staff?.StaffId;
        }

        public async Task<bool> DeleteByEmailAsync(string email)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == email);
            if (staff == null) return false;

            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        // Gán Role nếu chưa tồn tại
        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static UserDto MapToDto(User user, string role) => new()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = role,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            DateOfBirth = user.DateOfBirth,
            Points = user.Points,
            ImageUser = user.ImageUser
        };
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "Images", "Staffs");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/Images/Staffs/{uniqueName}";
        }
    }
}