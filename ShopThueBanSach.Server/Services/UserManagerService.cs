using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Area.Admin.Entities;
using ShopThueBanSach.Server.Area.Admin.Service;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.AuthModel;
using ShopThueBanSach.Server.Models.StaffModel;
using ShopThueBanSach.Server.Models.UserModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class UserManagerService : IUserManagerService
    {
		private readonly UserManager<User> _userManager;
		private readonly IStaffService _staffService;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IPhotoService _photoService;

		public UserManagerService(UserManager<User> userManager, IStaffService staffService, RoleManager<IdentityRole> roleManager, IPhotoService photoService)
		{
			_userManager = userManager;
			_staffService = staffService;
			_roleManager = roleManager;
			_photoService = photoService;
		}

		public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Hiển thị tất cả trừ Admin và Staff
                if (!roles.Contains("Admin") && !roles.Contains("Staff"))
                {
                    result.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        UserName = user.UserName,
                        Address = user.Address,
                        PhoneNumber = user.PhoneNumber,
                        DateOfBirth = user.DateOfBirth,
                        Points = user.Points,
                        ImageUser = user.ImageUser,
                        Role = roles.FirstOrDefault()
                    });
                }
            }

            return result;
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;
            return await MapToDto(user, _userManager);
        }

		public async Task<bool> UpdateAsync(string id, UpdateUserDto dto)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return false;

			user.Address = dto.Address ?? user.Address;
			user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
			user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
			user.Points = dto.Points ?? user.Points;

			if (dto.ImageFile != null && dto.ImageFile.Length > 0)
			{
				if (!string.IsNullOrEmpty(user.ImageUser))
				{
					var oldPublicId = Path.GetFileNameWithoutExtension(new Uri(user.ImageUser).AbsolutePath);
					await _photoService.DeleteImageAsync("UserAvatars/" + oldPublicId);
				}

				var (imageUrl, publicIdNew) = await _photoService.UploadImageAsync(dto.ImageFile, "UserAvatars");
				if (imageUrl != null)
				{
					user.ImageUser = imageUrl;
				}
			}

			if (!string.IsNullOrWhiteSpace(dto.Role))
			{
				if (!await _roleManager.RoleExistsAsync(dto.Role))
				{
					await _roleManager.CreateAsync(new IdentityRole(dto.Role));
				}

				var currentRoles = await _userManager.GetRolesAsync(user);
				if (currentRoles.Any())
				{
					var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
					if (!removeResult.Succeeded) return false;
				}

				var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
				if (!addResult.Succeeded) return false;

				if (dto.Role == "Staff")
				{
					var staffExists = await _staffService.ExistsAsync(user.Id);
					if (!staffExists)
					{
						var staffDto = new StaffDto
						{
							StaffId = user.Id,
							FullName = user.UserName ?? "",
							Email = user.Email,
							Address = user.Address,
							PhoneNumber = user.PhoneNumber,
							DateOfBirth = user.DateOfBirth,
							ImageFile = null,
							Password = ""
						};

						await _staffService.AddAsync(staffDto);
					}
				}
			}

			var updateResult = await _userManager.UpdateAsync(user);
			return updateResult.Succeeded;
		}

		public async Task<bool> DeleteAsync(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return false;

			var roles = await _userManager.GetRolesAsync(user);
			if (!roles.Contains("Customer")) return false;
			if (!string.IsNullOrEmpty(user.ImageUser))
			{
				var publicId = Path.GetFileNameWithoutExtension(new Uri(user.ImageUser).AbsolutePath);
				await _photoService.DeleteImageAsync("UserAvatars/" + publicId);
			}
			var result = await _userManager.DeleteAsync(user);
			return result.Succeeded;
		}

		public async Task<bool> UpdateCustomerAsync(string id, UpdateCustomerDto dto)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return false;

			var roles = await _userManager.GetRolesAsync(user);
			if (!roles.Contains("Customer")) return false;

			user.Address = dto.Address ?? user.Address;
			user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
			user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
			user.Points = dto.Points ?? user.Points;

			if (dto.ImageFile != null && dto.ImageFile.Length > 0)
			{
				if (!string.IsNullOrEmpty(user.ImageUser))
				{
					var oldPublicId = Path.GetFileNameWithoutExtension(new Uri(user.ImageUser).AbsolutePath);
					await _photoService.DeleteImageAsync("UserAvatars/" + oldPublicId);
				}

				var (imageUrl, publicIdNew) = await _photoService.UploadImageAsync(dto.ImageFile, "UserAvatars");
				if (imageUrl != null)
				{
					user.ImageUser = imageUrl;
				}
			}

			var result = await _userManager.UpdateAsync(user);
			return result.Succeeded;
		}

		private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Users");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/Images/Users/{uniqueFileName}";
        }

        private static async Task<UserDto> MapToDto(User user, UserManager<User> userManager)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Points = user.Points,
                ImageUser = user.ImageUser,
                Role = roles.FirstOrDefault()
            };
        }
    }
}
