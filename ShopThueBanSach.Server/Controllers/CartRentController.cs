using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartRentController : ControllerBase
    {
        private readonly ICartRentService _cartService;

        public CartRentController(ICartRentService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            return Ok(_cartService.GetCart());
        }

        [HttpPost("addrent")]
        public async Task<IActionResult> AddToCart([FromBody] string rentBookItemId)
        {
            var success = await _cartService.AddToCartAsync(rentBookItemId);
            if (!success) return BadRequest("Không thể thêm sản phẩm vào giỏ");

            return Ok(_cartService.GetCart());
        }

        /* [HttpPut("update-quantity")]
         public IActionResult UpdateQuantity([FromBody] UpdateQuantityDto dto)
         {
             _cartService.(dto.BookId, dto.Quantity);
             return Ok(_cartService.GetCart());
         }
 */
        [HttpDelete("removerent/{bookId}")]
        public IActionResult Remove(string bookId)
        {
            _cartService.RemoveFromCart(bookId);
            return Ok(_cartService.GetCart());
        }

        [HttpDelete("clearrent")]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return Ok();
        }
    }
}
