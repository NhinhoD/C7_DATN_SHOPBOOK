using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpPost("exchange-discount/{discountCodeId}")]
        public async Task<IActionResult> ExchangeDiscountCode(string discountCodeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _voucherService.ExchangePointsForVoucherAsync(userId, discountCodeId);
            if (result.Contains("thành công"))
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }

        // GET: api/voucher/history
        [HttpGet("history")]
        public async Task<IActionResult> GetVoucherHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var history = await _voucherService.GetUserVoucherHistory(userId);
            return Ok(history);
        }
    }
}
