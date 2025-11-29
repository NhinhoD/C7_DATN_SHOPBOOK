using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteRentBookController : ControllerBase
    {
        private readonly IFavoriteRentBookService _service;

        public FavoriteRentBookController(IFavoriteRentBookService service)
        {
            _service = service;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bạn cần đăng nhập." });

            var favorites = await _service.GetFavoritesByUserAsync(userId);
            return Ok(favorites);
        }

        [HttpPost("toggle/{rentBookId}")]
        public async Task<IActionResult> ToggleFavorite(string rentBookId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bạn cần đăng nhập." });

            var result = await _service.ToggleFavoriteAsync(userId, rentBookId);
            return result
                ? Ok(new { message = "Đã cập nhật trạng thái yêu thích." })
                : BadRequest(new { message = "Cập nhật thất bại." });
        }

        [HttpDelete("{rentBookId}")]
        public async Task<IActionResult> RemoveFavorite(string rentBookId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Bạn cần đăng nhập." });

            var result = await _service.RemoveFromFavoritesAsync(userId, rentBookId);
            return result
                ? Ok(new { message = "Đã xóa khỏi yêu thích." })
                : NotFound(new { message = "Không tìm thấy yêu thích." });
        }
    }
}
