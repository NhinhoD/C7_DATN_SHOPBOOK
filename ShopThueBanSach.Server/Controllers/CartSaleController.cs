using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartSaleController : ControllerBase
    {
        private readonly ISaleCartService _cartService;

        public CartSaleController(ISaleCartService cartService)
        {
            _cartService = cartService;
        }

        // ✅ Lấy giỏ hàng hiện tại
        [HttpGet("get")]
        public IActionResult GetCart()
        {
            return Ok(_cartService.GetCart());
        }

        // ✅ Thêm sản phẩm vào giỏ với số lượng
        [HttpPost("add")]
        public IActionResult AddToCart([FromBody] AddToSaleCartDto dto)
        {
            _cartService.AddToCart(dto.ProductId, dto.Quantity);
            return Ok(_cartService.GetCart());
        }

        // ✅ Tăng số lượng sản phẩm
        [HttpPost("increase/{productId}")]
        public IActionResult IncreaseQuantity(string productId)
        {
            _cartService.IncreaseQuantity(productId);
            return Ok(_cartService.GetCart());
        }

        // ✅ Giảm số lượng sản phẩm
        [HttpPost("decrease/{productId}")]
        public IActionResult DecreaseQuantity(string productId)
        {
            _cartService.DecreaseQuantity(productId);
            return Ok(_cartService.GetCart());
        }

        // ✅ Xóa sản phẩm khỏi giỏ
        [HttpDelete("remove/{productId}")]
        public IActionResult RemoveFromCart(string productId)
        {
            _cartService.RemoveFromCart(productId);
            return Ok(_cartService.GetCart());
        }

        // ✅ Xóa toàn bộ giỏ hàng
        [HttpDelete("clear")]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return Ok("Đã xóa toàn bộ giỏ hàng.");
        }

        // ✅ Tổng tiền
        [HttpGet("total")]
        public IActionResult GetTotal()
        {
            var total = _cartService.GetTotal();
            return Ok(new { total });
        }
    }
}
