using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.RentOrderModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoMoOrderController : ControllerBase
    {
        private readonly IRentOrderService _rentOrderService;

        public MoMoOrderController(IRentOrderService rentOrderService)
        {
            _rentOrderService = rentOrderService;
        }

        // Bước 1: Lấy URL thanh toán MoMo
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreateMoMoPayment([FromBody] RentOrderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            request.UserId = userId;
            return await _rentOrderService.PrepareMoMoOrderAsync(request);
        }

        // Bước 2: Nhận callback MoMo khi thanh toán thành công, mới tạo đơn hàng
        [AllowAnonymous]
        [HttpPost("callback")]
        public async Task<IActionResult> MoMoCallback([FromBody] JsonElement data)
        {
            var resultCode = data.GetProperty("resultCode").GetInt32();
            if (resultCode != 0)
                return Ok(new { message = "Thanh toán thất bại" });

            var extraData = data.GetProperty("extraData").GetString();
            if (string.IsNullOrEmpty(extraData))
                return BadRequest("Thiếu dữ liệu đơn hàng");

            var request = JsonSerializer.Deserialize<RentOrderRequest>(extraData);
            if (request == null)
                return BadRequest("Dữ liệu đơn hàng không hợp lệ");

            await _rentOrderService.CreateRentOrderAfterMoMoAsync(request);
            return Ok(new { message = "Đơn hàng đã được tạo sau khi thanh toán thành công" });
        }
    }
}
