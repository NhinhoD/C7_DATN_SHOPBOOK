using Microsoft.AspNetCore.Identity;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.UserModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Services
{
    public class UserService(UserManager<User> userManager, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, IPhotoService photoService) : IUserService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IWebHostEnvironment _env = env;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
		private readonly IPhotoService _photoService = photoService;

		public async Task<UserProfileDto?> GetProfileAsync()
        {
			var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId)) return null;

			var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
				UserId = user.Id,
				UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhonNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Points = user.Points,
                ImageUser = user.ImageUser,
            };
        }

		public async Task<string> UpdateProfileAsync(UpdateProfileDto dto)
		{
			var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return "Không xác định được người dùng.";

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return "Người dùng không tồn tại.";

			// Cập nhật thông tin
			user.UserName = dto.UserName;
			user.PhoneNumber = dto.PhoneNumber;
			user.Address = dto.Address;
			user.DateOfBirth = dto.DateOfBirth;

			// Upload ảnh lên Cloudinary nếu có ảnh mới
			if (dto.ImageUser != null && dto.ImageUser.Length > 0)
			{
				if (!string.IsNullOrEmpty(user.ImageUser))
				{
					var publicId = Path.GetFileNameWithoutExtension(new Uri(user.ImageUser).AbsolutePath);
					await _photoService.DeleteImageAsync("UserAvatars/" + publicId);
				}

				var (imageUrl, publicIdNew) = await _photoService.UploadImageAsync(dto.ImageUser, "UserAvatars");
				if (imageUrl != null)
				{
					user.ImageUser = imageUrl;
				}
			}

			var result = await _userManager.UpdateAsync(user);
			return result.Succeeded
				? "Cập nhật hồ sơ thành công."
				: $"Lỗi: {string.Join(", ", result.Errors.Select(e => e.Description))}";
		}

		public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }
    }
}
