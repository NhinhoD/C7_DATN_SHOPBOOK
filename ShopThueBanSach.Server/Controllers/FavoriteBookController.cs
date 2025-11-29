using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteBookController : ControllerBase
    {
        private readonly IFavoriteBookService _favoriteBookService;

        public FavoriteBookController(IFavoriteBookService favoriteBookService)
        {
            _favoriteBookService = favoriteBookService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Invalid user");

            var result = await _favoriteBookService.GetFavoriteBooksAsync(userId);
            return Ok(result);
        }



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteBookCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Invalid user");

            var success = await _favoriteBookService.AddFavoriteBookAsync(userId, dto.SaleBookId);
            if (!success) return BadRequest("Book already in favorites.");
            return Ok("Book added to favorites.");
        }



        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite([FromQuery] string saleBookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Invalid user");

            var success = await _favoriteBookService.RemoveFavoriteBookAsync(userId, saleBookId);
            if (!success) return NotFound("Book not found in favorites.");
            return Ok("Book removed from favorites.");
        }
    }
}
