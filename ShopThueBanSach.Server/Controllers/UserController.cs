using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.UserModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Cần đăng nhập mới được truy cập
    public class UserController(IUserService userService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext!.User.FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("UserId không tìm thấy");
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {

			var profile = await _userService.GetProfileAsync();
			if (profile == null) return NotFound("Không tìm thấy người dùng");
            return Ok(profile);
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var message = await _userService.UpdateProfileAsync(dto);
            if (message!.StartsWith("Cập nhật"))
                return Ok(new { message });

            return BadRequest(new { error = message });
        }



        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = GetUserId();
            var success = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
            if (!success)
                return BadRequest(new { message = "Đổi mật khẩu thất bại hoặc mật khẩu hiện tại không đúng" });

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }
    }
}
